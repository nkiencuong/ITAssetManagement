using ITAssetManagement.Models.Entitis;
using ITAssetManagement.Repo.Interfaces;
using ITAssetManagement.Service.Interfaces;
using ITAssetManagement.Request.RepairTickets;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ITAssetManagement.Service.Services
{
    public class RepairService : IRepairService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuditLogService _auditLogService;

        public RepairService(IUnitOfWork unitOfWork, IAuditLogService auditLogService)
        {
            _unitOfWork = unitOfWork;
            _auditLogService = auditLogService;
        }

        // 1. LẤY DANH SÁCH (GIỮ NGUYÊN)
        public async Task<List<RepairTicket>> GetAllTicketsAsync()
        {
            var repo = _unitOfWork.GetRepository<RepairTicket>();
            return await repo.GetAll()
                             .Include(r => r.Asset)
                             .Include(r => r.Department)
                             .Include(r => r.User)
                             .Include(r => r.RepairDetails).ThenInclude(d => d.Asset)
                             .OrderByDescending(r => r.CreatedDate)
                             .ToListAsync();
        }

        // 2. LẤY CHI TIẾT (GIỮ NGUYÊN)
        public async Task<RepairTicket?> GetTicketByIdAsync(int id)
        {
            var repo = _unitOfWork.GetRepository<RepairTicket>();
            return await repo.GetAll()
                             .Include(r => r.Asset)
                             .Include(r => r.Department)
                             .Include(r => r.User)
                             .Include(r => r.RepairDetails).ThenInclude(d => d.Asset)
                             .FirstOrDefaultAsync(r => r.TicketID == id);
        }

        // 3. TẠO PHIẾU SỬA (🔥 ĐÃ SỬA: Xử lý AssetID nullable)
        public async Task<RepairTicket> CreateTicketAsync(RepairTicket ticket, int actionUserId)
        {
            if (ticket.UserID == null || ticket.UserID == 0) ticket.UserID = 1;
            if (ticket.CreatedDate == default) ticket.CreatedDate = DateTime.Now;

            ticket.Status = 0; ticket.Cost = 0;
            ticket.Asset = null; ticket.Department = null; ticket.User = null; ticket.RepairDetails = null;

            var assetRepo = _unitOfWork.GetRepository<Asset>();
            string assetName = "Thiết bị chưa xác định";

            // 👇 KIỂM TRA AssetID CÓ NULL KHÔNG TRƯỚC KHI GỌI HÀM
            if (ticket.AssetID.HasValue)
            {
                var asset = await assetRepo.GetByIdAsync(ticket.AssetID.Value);
                if (asset != null)
                {
                    assetName = asset.AssetName;
                    asset.Status = 2; // Đang sửa
                    assetRepo.Update(asset);
                }
            }

            var repairRepo = _unitOfWork.GetRepository<RepairTicket>();
            await repairRepo.AddAsync(ticket);
            await _unitOfWork.CompleteAsync();

            // GHI LOG BÁO HỎNG
            await _auditLogService.CreateLogAsync(
                action: "Báo hỏng",
                tableName: "RepairTicket",
                recordId: ticket.AssetID ?? 0, // Nếu null thì ghi 0
                details: $"Báo hỏng '{assetName}' - Lỗi: {ticket.Description} (Người báo trên phiếu: {ticket.ReporterName})",
                userId: actionUserId
            );

            return ticket;
        }

        // 4. HỦY PHIẾU (🔥 ĐÃ SỬA: Xử lý AssetID nullable)
        public async Task<bool> CancelTicketAsync(int ticketId, string reason)
        {
            var repairRepo = _unitOfWork.GetRepository<RepairTicket>();
            var assetRepo = _unitOfWork.GetRepository<Asset>();

            var ticket = await repairRepo.GetByIdAsync(ticketId);
            if (ticket == null) return false;

            ticket.Status = 3;
            ticket.Note = "Hủy: " + reason;

            // 👇 KIỂM TRA AssetID CÓ NULL KHÔNG TRƯỚC KHI GỌI HÀM
            if (ticket.AssetID.HasValue)
            {
                var asset = await assetRepo.GetByIdAsync(ticket.AssetID.Value);
                if (asset != null)
                {
                    asset.Status = 0; // Trả về trạng thái bình thường
                    assetRepo.Update(asset);
                }
            }

            repairRepo.Update(ticket);
            await _unitOfWork.CompleteAsync();

            // GHI LOG HỦY
            await _auditLogService.CreateLogAsync(
                action: "Hủy phiếu sửa",
                tableName: "RepairTicket",
                recordId: ticketId,
                details: $"Đã hủy phiếu #{ticketId}. Lý do: {reason}",
                userId: 1
            );

            return true;
        }

        // 5. HOÀN THÀNH SỬA CHỮA (🔥 ĐÃ SỬA: Xử lý AssetID nullable)
        public async Task<bool> CompleteRepairAsync(int ticketId, string damageStatus, string solution, List<RepairItemDto> parts, int userId)
        {
            var repairRepo = _unitOfWork.GetRepository<RepairTicket>();
            var assetRepo = _unitOfWork.GetRepository<Asset>();
            var detailRepo = _unitOfWork.GetRepository<RepairTicketDetail>();
            var transRepo = _unitOfWork.GetRepository<WarehouseTransaction>();

            var ticket = await repairRepo.GetByIdAsync(ticketId);
            if (ticket == null) return false;

            // 1. Lấy thông tin Máy Chính (Chỉ lấy khi AssetID có giá trị)
            Asset? mainAsset = null;
            if (ticket.AssetID.HasValue)
            {
                mainAsset = await assetRepo.GetByIdAsync(ticket.AssetID.Value);
            }

            // 2. Xác định ID phòng ban nhận linh kiện
            int? targetDepartmentID = ticket.DepartmentID;
            if ((targetDepartmentID == null || targetDepartmentID == 0) && mainAsset != null)
            {
                targetDepartmentID = mainAsset.DepartmentID;
            }

            ticket.Status = 2;
            ticket.RepairDate = DateTime.Now;
            ticket.Solution = solution;
            ticket.DamageStatus = damageStatus;

            decimal totalCost = 0;
            List<string> partNames = new List<string>();

            if (parts != null && parts.Count > 0)
            {
                foreach (var item in parts)
                {
                    var partInDb = await assetRepo.GetByIdAsync(item.AssetId);
                    if (partInDb != null && partInDb.Quantity >= item.Quantity)
                    {
                        partInDb.Quantity -= item.Quantity;
                        assetRepo.Update(partInDb);

                        await detailRepo.AddAsync(new RepairTicketDetail
                        {
                            TicketID = ticketId,
                            AssetID = item.AssetId,
                            Quantity = item.Quantity,
                            Price = partInDb.Price,
                            Note = ""
                        });

                        // Ghi Transaction
                        await transRepo.AddAsync(new WarehouseTransaction
                        {
                            AssetID = item.AssetId,
                            Type = "OUT_REPAIR",
                            Quantity = item.Quantity,
                            Date = DateTime.Now,
                            UserID = userId,
                            DepartmentID = targetDepartmentID,
                            Note = $"Thay thế cho máy '{mainAsset?.AssetName ?? "Không rõ"}' (Phiếu #{ticketId})",
                            ReferenceNo = $"REP-{ticketId}"
                        });

                        totalCost += (partInDb.Price * item.Quantity);
                        partNames.Add($"{partInDb.AssetName} ({item.Quantity})");
                    }
                }
            }

            if (partNames.Count > 0) ticket.Note = string.Join(", ", partNames);
            else ticket.Note = "Không thay thế linh kiện";

            ticket.Cost = totalCost;

            // Cập nhật trạng thái máy chính (nếu có)
            if (mainAsset != null)
            {
                mainAsset.Status = 0; // Sửa xong -> Trạng thái OK
                assetRepo.Update(mainAsset);
            }

            repairRepo.Update(ticket);
            await _unitOfWork.CompleteAsync();

            // GHI LOG SỬA XONG
            await _auditLogService.CreateLogAsync(
                action: "Sửa xong",
                tableName: "RepairTicket",
                recordId: ticketId,
                details: $"Đã sửa xong '{mainAsset?.AssetName ?? "Thiết bị chưa xác định"}'. Giải pháp: {solution}. Linh kiện: {ticket.Note}",
                userId: userId
            );

            return true;
        }
        // --- 6. TỰ NHẬN VIỆC (IT tự bấm) ---
        public async Task<bool> ClaimTicketAsync(int ticketId, int userId)
        {
            var repairRepo = _unitOfWork.GetRepository<RepairTicket>();
            var userRepo = _unitOfWork.GetRepository<User>(); // Thêm repo User để lấy tên

            var ticket = await repairRepo.GetByIdAsync(ticketId);
            if (ticket == null) throw new Exception("Không tìm thấy phiếu sửa chữa!");
            if (ticket.AssignedToUserID != null && ticket.AssignedToUserID != userId)
                throw new Exception("Chậm chân rồi bác ơi! Ca này đã có đồng nghiệp khác nhận!");

            // Lấy tên anh IT
            var linh = await userRepo.GetByIdAsync(userId);
            string tenLinh = linh?.FullName ?? $"Kỹ thuật viên (ID: {userId})";

            ticket.AssignedToUserID = userId;
            ticket.Status = 1;

            repairRepo.Update(ticket);
            await _unitOfWork.CompleteAsync();

            // Ghi Log có Tên
            await _auditLogService.CreateLogAsync(
                action: "Nhận việc",
                tableName: "RepairTicket",
                recordId: ticketId,
                details: $"{tenLinh} đã chủ động tiếp nhận xử lý ca này.",
                userId: userId
            );

            return true;
        }

        // --- 7. SẾP PHÂN CÔNG VIỆC (CÓ BẮN THÔNG BÁO) ---
        public async Task<bool> AssignTicketAsync(int ticketId, int assignToUserId, int actionUserId)
        {
            var repairRepo = _unitOfWork.GetRepository<RepairTicket>();
            var userRepo = _unitOfWork.GetRepository<User>();
            var notifRepo = _unitOfWork.GetRepository<Notification>(); // 🚀 GỌI BẢNG THÔNG BÁO RA

            var ticket = await repairRepo.GetByIdAsync(ticketId);
            if (ticket == null) throw new Exception("Không tìm thấy phiếu sửa chữa!");

            // Lấy tên Sếp và tên Anh IT để ghi Log và Thông báo cho hay
            var sep = await userRepo.GetByIdAsync(actionUserId);
            var linh = await userRepo.GetByIdAsync(assignToUserId);
            string tenSep = sep?.FullName ?? $"Sếp (ID: {actionUserId})";
            string tenLinh = linh?.FullName ?? $"Kỹ thuật viên (ID: {assignToUserId})";

            ticket.AssignedToUserID = assignToUserId;
            ticket.Status = 1;

            repairRepo.Update(ticket);

            // 🚀🚀🚀 TẠO THÔNG BÁO GỬI CHO ANH IT 🚀🚀🚀
            var thongBao = new Notification
            {
                UserID = assignToUserId, // Gửi đích danh cho anh IT này
                Title = "Sếp vừa giao việc mới! 🚨",
                Message = $"{tenSep} vừa phân công bạn xử lý sự cố #{ticketId}: {ticket.Description}",
                RelatedUrl = "/repairs", // Bấm vào thông báo sẽ nhảy ra trang này
                IsRead = false, // Chưa đọc (Màu đỏ)
                CreatedAt = DateTime.Now
            };
            await notifRepo.AddAsync(thongBao);

            // Lưu toàn bộ vào Database
            await _unitOfWork.CompleteAsync();

            // Ghi Log hệ thống
            await _auditLogService.CreateLogAsync(
                action: "Phân công",
                tableName: "RepairTicket",
                recordId: ticketId,
                details: $"{tenSep} đã chỉ định {tenLinh} xử lý ca này.",
                userId: actionUserId
            );

            return true;
        }
    }
}