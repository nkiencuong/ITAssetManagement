using ITAssetManagement.Models.Entitis;
using ITAssetManagement.Repo.Interfaces;
using ITAssetManagement.Request.Allocations;
using ITAssetManagement.Response.Allocations;
using ITAssetManagement.Service.Interfaces;
using Microsoft.EntityFrameworkCore; // Cần thiết để dùng Include nếu sửa lại logic sau này
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ITAssetManagement.Service.Services
{
    public class AllocationService : IAllocationService
    {
        private readonly IUnitOfWork _unitOfWork;

        public AllocationService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        
        public async Task<bool> AllocateAssetsAsync(AllocateAssetsRequest request)
        {
            try
            {
                if (request.AssetIds == null || !request.AssetIds.Any())
                {
                    throw new Exception("Vui lòng chọn ít nhất một tài sản.");
                }

                // Sửa DepartmentID khớp với file Request
                var departmentRepo = _unitOfWork.GetRepository<Department>();
                var department = await departmentRepo.GetByIdAsync(request.DepartmentID);
                if (department == null)
                    throw new Exception($"Không tìm thấy phòng ban ID: {request.DepartmentID}");

                var assetRepo = _unitOfWork.GetRepository<Asset>();
                var allocationRepo = _unitOfWork.GetRepository<AssetAllocation>();

                foreach (var assetId in request.AssetIds)
                {
                    var asset = await assetRepo.GetByIdAsync(assetId);
                    if (asset == null) throw new Exception($"Tài sản ID {assetId} không tồn tại.");

                    if (asset.Status != 0)
                    {
                        throw new Exception($"Tài sản '{asset.AssetName}' đang bận (Status={asset.Status}).");
                    }

                    var allocation = new AssetAllocation
                    {
                        AssetID = assetId,
                        // Dùng các biến khớp với Request vừa sửa
                        DepartmentID = request.DepartmentID,
                        UserID = request.UserID,
                        AllocatedDate = request.AllocatedDate,
                        Status = 1,
                        Note = request.Note
                    };

                    await allocationRepo.AddAsync(allocation);

                    asset.Status = 1;
                    // Nếu muốn lưu vị trí text: asset.Location = department.DeptName;
                    assetRepo.Update(asset);
                }

                await _unitOfWork.CompleteAsync();
                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        // ==========================================
        // 2. CHỨC NĂNG LỊCH SỬ (Sửa lỗi d.DeptName)
        // ==========================================
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
                            Serial = a.Serial,
                            // ĐÃ SỬA: Dùng d.DeptName thay vì d.Name
                            DepartmentName = d.DeptName,
                            ReceiverName = u != null ? u.FullName : "Không xác định",
                            AllocatedDate = h.AllocatedDate,
                            Note = h.Note
                        };

            return query.OrderByDescending(x => x.AllocatedDate).ToList();
        }
    }
}