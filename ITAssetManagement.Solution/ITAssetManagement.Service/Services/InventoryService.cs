using ITAssetManagement.Models.Entities; // Hoặc .Entitis tùy project của bạn
using ITAssetManagement.Models.Entitis;
using ITAssetManagement.Repo.Interfaces;
using ITAssetManagement.Response;
using ITAssetManagement.Response.InventoryCheck; // Nếu có namespace này
using ITAssetManagement.Service.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ITAssetManagement.Service.Services
{
    public class InventoryService : IInventoryService
    {
        private readonly IUnitOfWork _unitOfWork;

        public InventoryService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<InventoryCheckResponse>> GetAllChecksAsync()
        {
            var checks = await _unitOfWork.GetRepository<InventoryCheck>().GetAllAsync();
            var users = await _unitOfWork.GetRepository<User>().GetAllAsync();
            var assets = await _unitOfWork.GetRepository<Asset>().GetAllAsync();

            var query = from c in checks
                        join u in users on c.UserID equals u.UserID into uGroup
                        from u in uGroup.DefaultIfEmpty()

                        join a in assets on c.AssetID equals a.AssetID into aGroup
                        from a in aGroup.DefaultIfEmpty()

                        orderby c.CheckDate descending

                        select new InventoryCheckResponse
                        {
                            CheckID = c.CheckID,
                            CheckDate = c.CheckDate,
                            UserName = u != null ? u.FullName : "N/A",
                            AssetName = a != null ? a.AssetName : $"Tài sản đã xóa (#{c.AssetID})",
                            ActualStatus = c.ActualStatus,
                            Discrepancy = c.Discrepancy,
                            Note = c.Note
                        };

            return query.ToList();
        }

        public async Task CreateCheckAsync(InventoryCheck check)
        {
            // 1. Gán thời gian hiện tại
            check.CheckDate = DateTime.Now;

            // 2. Lưu phiếu kiểm kê
            await _unitOfWork.GetRepository<InventoryCheck>().AddAsync(check);

            // 3. Nếu có sai lệch, tự động cập nhật trạng thái tài sản gốc
            if (check.Discrepancy)
            {
                var assetRepo = _unitOfWork.GetRepository<Asset>();
                var asset = await assetRepo.GetByIdAsync(check.AssetID);

                if (asset != null)
                {
                    // --- ĐÃ SỬA: Chuyển đổi từ String sang Int ---
                    // Bạn hãy sửa các số (0, 1, 2...) bên dưới cho khớp với quy ước trong Database của bạn
                    int newStatusId = 0;

                    switch (check.ActualStatus)
                    {
                        case "Sẵn sàng":
                        case "Mới":
                            newStatusId = 1;
                            break;
                        case "Đang sử dụng":
                            newStatusId = 2;
                            break;
                        case "Hỏng":
                        case "Cần sửa chữa":
                            newStatusId = 3;
                            break;
                        case "Mất":
                        case "Thất lạc":
                            newStatusId = 4;
                            break;
                        case "Thanh lý":
                            newStatusId = 5;
                            break;
                        default:
                            newStatusId = 0; // Trạng thái không xác định
                            break;
                    }

                    asset.Status = newStatusId; // Gán ID đã chuyển đổi

                    assetRepo.Update(asset);
                }
            }

            await _unitOfWork.CommitAsync();
        }
    }
}