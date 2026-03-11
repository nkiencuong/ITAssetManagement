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

        // --- 2. XEM LỊCH SỬ (Giữ nguyên) ---
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

                            // 🚀 BÁC THÊM 2 DÒNG NÀY VÀO CHỖ NÀY:
                            Quantity = h.Quantity,
                            DepartmentID = h.DepartmentID
                        };

            return query.OrderByDescending(x => x.AllocatedDate).ToList();
        }

        // --- 3. THU HỒI TÀI SẢN (Giữ nguyên) ---
        public async Task<bool> ReturnAssetAsync(int allocationId, string returnNote, DateTime returnDate)
        {
            try
            {
                var allocationRepo = _unitOfWork.GetRepository<AssetAllocation>();
                var assetRepo = _unitOfWork.GetRepository<Asset>();
                var transRepo = _unitOfWork.GetRepository<WarehouseTransaction>();

                var allocation = await allocationRepo.GetByIdAsync(allocationId);
                if (allocation == null) throw new Exception("Không tìm thấy phiếu cấp phát!");

                if (allocation.Status == 2) throw new Exception("Tài sản này đã được thu hồi rồi!");

                // Cập nhật trạng thái
                allocation.Status = 2; // Đã thu hồi
                allocation.ReturnedDate = returnDate;
                allocation.Note = string.IsNullOrEmpty(allocation.Note)
                                  ? $"Thu hồi: {returnNote}"
                                  : allocation.Note + $" | Thu hồi: {returnNote}";
                allocationRepo.Update(allocation);

                // Cộng lại kho
                var asset = await assetRepo.GetByIdAsync(allocation.AssetID);
                if (asset != null)
                {
                    asset.Quantity += allocation.Quantity;
                    if (asset.Quantity > 0) asset.Status = 0; // Sẵn sàng
                    assetRepo.Update(asset);
                }

                // Ghi lịch sử kho (IN)
                var transaction = new WarehouseTransaction
                {
                    AssetID = allocation.AssetID,
                    Type = "IN",
                    Quantity = allocation.Quantity,
                    Date = returnDate,
                    DepartmentID = allocation.DepartmentID,
                    UserID = allocation.UserID,
                    Note = $"Thu hồi: {returnNote}",
                    ReferenceNo = $"RETURN-{allocation.AllocationID}"
                };
                await transRepo.AddAsync(transaction);

                await _auditLogService.CreateLogAsync(
                    action: "Thu hồi",
                    tableName: "AssetAllocation",
                    recordId: allocation.AllocationID,
                    details: $"Đã thu hồi tài sản '{asset?.AssetName}' (Phiếu #{allocationId})",
                    userId: 1
                );

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

            asset.Status = asset.Quantity > 0 ? 0 : 1;

            allocationRepo.Update(allocation);
            assetRepo.Update(asset);
            await _unitOfWork.CompleteAsync();

            return true;
        }
    }
}