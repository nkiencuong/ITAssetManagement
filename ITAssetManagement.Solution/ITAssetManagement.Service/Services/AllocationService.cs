using ITAssetManagement.Models.Entitis;
using ITAssetManagement.Repo.Interfaces;
using ITAssetManagement.Request.Allocations;
using ITAssetManagement.Response.Allocations;
using ITAssetManagement.Service.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ITAssetManagement.Service.Services
{
    public class AllocationService : IAllocationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuditLogService _auditLogService;

        public AllocationService(IUnitOfWork unitOfWork, IAuditLogService auditLogService)
        {
            _unitOfWork = unitOfWork;
            _auditLogService = auditLogService;
        }

        // --- 1. CẤP PHÁT TÀI SẢN (CẬP NHẬT: XỬ LÝ SỐ LƯỢNG) ---
        public async Task<bool> AllocateAssetsAsync(AllocateAssetsRequest request, int actionUserId)
        {
            try
            {
                if (request.AssetIds == null || !request.AssetIds.Any())
                    throw new Exception("Vui lòng chọn ít nhất một tài sản.");

                var departmentRepo = _unitOfWork.GetRepository<Department>();
                var assetRepo = _unitOfWork.GetRepository<Asset>();
                var allocationRepo = _unitOfWork.GetRepository<AssetAllocation>();
                var transRepo = _unitOfWork.GetRepository<WarehouseTransaction>();

                var department = await departmentRepo.GetByIdAsync(request.DepartmentID);
                if (department == null) throw new Exception($"Không tìm thấy phòng ban ID: {request.DepartmentID}");

                // 👇 Lấy số lượng từ request, nếu không có hoặc <=0 thì mặc định là 1
                int quantityToAllocate = request.Quantity > 0 ? request.Quantity : 1;

                foreach (var assetId in request.AssetIds)
                {
                    var asset = await assetRepo.GetByIdAsync(assetId);
                    if (asset == null) continue;

                    // Kiểm tra tồn kho
                    if (asset.Quantity < quantityToAllocate)
                    {
                        throw new Exception($"Tài sản '{asset.AssetName}' không đủ tồn kho! (Yêu cầu: {quantityToAllocate}, Còn: {asset.Quantity})");
                    }

                    // Trừ kho
                    asset.Quantity -= quantityToAllocate;
                    if (asset.Quantity == 0) asset.Status = 1; // Hết hàng
                    assetRepo.Update(asset);

                    // Tạo phiếu cấp phát
                    var allocation = new AssetAllocation
                    {
                        AssetID = assetId,
                        DepartmentID = request.DepartmentID,
                        UserID = request.UserID,
                        AllocatedDate = request.AllocatedDate,
                        Status = 1, // Đang dùng
                        Note = request.Note,
                        Quantity = quantityToAllocate // Lưu số lượng cấp
                    };
                    await allocationRepo.AddAsync(allocation);

                    // Ghi lịch sử kho (OUT)
                    var transaction = new WarehouseTransaction
                    {
                        AssetID = assetId,
                        Type = "OUT",
                        Quantity = quantityToAllocate, // Ghi số lượng xuất
                        Date = request.AllocatedDate,
                        DepartmentID = request.DepartmentID,
                        UserID = request.UserID,
                        Note = $"Cấp phát cho khoa: {department.DeptName}",
                        ReferenceNo = $"ALLOC-{DateTime.Now:yyyyMMddHHmm}"
                    };
                    await transRepo.AddAsync(transaction);

                    // Ghi Log
                    await _auditLogService.CreateLogAsync(
                        action: "Cấp phát",
                        tableName: "AssetAllocation",
                        recordId: assetId,
                        details: $"Đã cấp phát {quantityToAllocate} tài sản '{asset.AssetName}' cho '{department.DeptName}'",
                        userId: actionUserId
                    );
                }

                await _unitOfWork.CompleteAsync();
                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        // --- 2. XEM LỊCH SỬ (🚀 ĐÃ FIX LỖI 0đ) ---
        public async Task<IEnumerable<AllocationHistoryResponse>> GetAllocationHistoryAsync()
        {
            var allocations = await _unitOfWork.GetRepository<AssetAllocation>().GetAllAsync();
            var assets = await _unitOfWork.GetRepository<Asset>().GetAllAsync();
            var departments = await _unitOfWork.GetRepository<Department>().GetAllAsync();
            var users = await _unitOfWork.GetRepository<User>().GetAllAsync();

            var query = from h in allocations
                        join a in assets on h.AssetID equals a.AssetID
                        join d in departments on h.DepartmentID equals d.DepartmentID
                        join u in users on h.UserID equals u.UserID into userGroup
                        from u in userGroup.DefaultIfEmpty()
                        select new AllocationHistoryResponse
                        {
                            AllocationID = h.AllocationID,
                            AssetName = a.AssetName,
                            Serial = $"SL Cấp: {h.Quantity} {a.Unit}",
                            DepartmentName = d.DeptName,
                            ReceiverName = u != null ? u.FullName : "Không xác định",
                            AllocatedDate = h.AllocatedDate,
                            Note = h.Note,
                            Status = h.Status,
                            Quantity = h.Quantity,
                            DepartmentID = h.DepartmentID,
                            // 🚀 LẤY ĐƠN GIÁ TỪ BẢNG TÀI SẢN (ASSET) NÉM SANG FRONTEND
                            Price = a.Price
                        };


            return query.OrderByDescending(x => x.AllocatedDate).ToList();
        }

        // --- 3. THU HỒI TÀI SẢN (🚀 ĐÃ NÂNG CẤP THU HỒI 1 PHẦN) ---
        // Lưu ý: Đã thêm tham số returnQty
        public async Task<bool> ReturnAssetAsync(int allocationId, string returnNote, DateTime returnDate, int returnQty, bool isBroken)
        {
            try
            {
                var allocationRepo = _unitOfWork.GetRepository<AssetAllocation>();
                var assetRepo = _unitOfWork.GetRepository<Asset>();
                var transRepo = _unitOfWork.GetRepository<WarehouseTransaction>();

                var allocation = await allocationRepo.GetByIdAsync(allocationId);
                if (allocation == null) throw new Exception("Không tìm thấy phiếu cấp phát!");
                if (allocation.Status == 2) throw new Exception("Tài sản này đã được thu hồi hết rồi!");

                // Kiểm tra số lượng thu hồi hợp lệ không
                if (returnQty <= 0 || returnQty > allocation.Quantity)
                    throw new Exception("Số lượng thu hồi không hợp lệ!");

                // Trừ số lượng đang dùng
                allocation.Quantity -= returnQty;

                // Ghi log lý do
                string log = $"Thu hồi {returnQty} cái: {returnNote}";
                allocation.Note = string.IsNullOrEmpty(allocation.Note) ? log : allocation.Note + $" | {log}";

                // Nếu thu hồi hết sạch thì mới đổi trạng thái thành Đã Thu Hồi (2)
                if (allocation.Quantity == 0)
                {
                    allocation.Status = 2;
                    allocation.ReturnedDate = returnDate;
                }
                allocationRepo.Update(allocation);

                //  Cộng trả lại kho

                var asset = await assetRepo.GetByIdAsync(allocation.AssetID);
                if (asset != null)
                {
                    // Chỉ cộng vào kho nếu máy KHÔNG hỏng (isBroken == false)
                    if (!isBroken)
                    {
                        asset.Quantity += returnQty;
                        if (asset.Quantity > 0) asset.Status = 0; // Sẵn sàng sử dụng
                    }
                    else
                    {
                        // Nếu máy hỏng: Không cộng Quantity (Kho không tăng số lượng dùng được)
                        // Mình ghi chú thêm vào phiếu cấp phát để sau này biết tại sao thiếu máy
                        allocation.Note += $" | Thu hồi {returnQty} cái bị hỏng (Không nhập kho)";
                    }

                    // Luôn Update asset vì có thể trạng thái hoặc thông tin khác thay đổi
                    assetRepo.Update(asset);
                }

                // Ghi lịch sử kho (IN)
                var transaction = new WarehouseTransaction
                {
                    AssetID = allocation.AssetID,
                    Type = "IN",
                    Quantity = returnQty, // 🚀 Ghi đúng số lượng vừa thu hồi
                    Date = returnDate,
                    DepartmentID = allocation.DepartmentID,
                    UserID = allocation.UserID,
                    Note = log,
                    ReferenceNo = $"RETURN-{allocation.AllocationID}"
                };
                await transRepo.AddAsync(transaction);

                await _auditLogService.CreateLogAsync(
                    action: "Thu hồi",
                    tableName: "AssetAllocation",
                    recordId: allocation.AllocationID,
                    details: $"Đã thu hồi {returnQty} tài sản '{asset?.AssetName}' (Phiếu #{allocationId})",
                    userId: 1
                );

                await _unitOfWork.CompleteAsync();
                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }
        // --- 4. SỬA CẤP PHÁT (THUẬT TOÁN BÙ TRỪ KHO) ---
        public async Task<bool> UpdateAllocationAsync(int allocationId, EditAllocationRequest request, int actionUserId)
        {
            var allocationRepo = _unitOfWork.GetRepository<AssetAllocation>();
            var assetRepo = _unitOfWork.GetRepository<Asset>();

            var allocation = await allocationRepo.GetByIdAsync(allocationId);
            if (allocation == null) throw new Exception("Không tìm thấy phiếu cấp phát!");
            if (allocation.Status == 2) throw new Exception("Tài sản này đã thu hồi, không thể sửa!");

            var asset = await assetRepo.GetByIdAsync(allocation.AssetID);
            if (asset == null) throw new Exception("Tài sản không tồn tại trong kho!");

            // THUẬT TOÁN BÙ TRỪ SỐ LƯỢNG KHO
            int oldQty = allocation.Quantity;
            int newQty = request.Quantity;
            int diff = newQty - oldQty;

            if (diff > 0)
            {
                if (asset.Quantity < diff) throw new Exception($"Kho không đủ để cấp thêm! (Chỉ còn tồn {asset.Quantity} cái)");
                asset.Quantity -= diff;
            }
            else if (diff < 0)
            {
                asset.Quantity += Math.Abs(diff);
            }

            // Cập nhật lại thông tin phiếu
            allocation.Quantity = newQty;
            allocation.DepartmentID = request.DepartmentID;
            allocation.AllocatedDate = request.AllocatedDate;
            allocation.Note = request.ReceiverName;

            // 🚀🚀🚀 CẬP NHẬT ĐƠN GIÁ VÀO KHO 🚀🚀🚀
            asset.Price = request.Price;
            asset.Status = asset.Quantity > 0 ? 0 : 1;

            allocationRepo.Update(allocation);
            assetRepo.Update(asset);
            await _unitOfWork.CompleteAsync();

            return true;
        }
    }
}
    