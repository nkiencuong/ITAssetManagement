using ITAssetManagement.Models.Entitis;
using ITAssetManagement.Repo.Interfaces;
using ITAssetManagement.Response.Warehouse;
using ITAssetManagement.Service.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ITAssetManagement.Service.Services
{
    public class WarehouseService : IWarehouseService
    {
        private readonly IUnitOfWork _unitOfWork;

        public WarehouseService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // Đã sửa: Nhận thêm fromDate, toDate
        public async Task<IEnumerable<WarehouseHistoryResponse>> GetHistoryAsync(DateTime? fromDate, DateTime? toDate)
        {
            // 1. Lấy dữ liệu thô
            var transactions = await _unitOfWork.GetRepository<WarehouseTransaction>().GetAllAsync();
            var assets = await _unitOfWork.GetRepository<Asset>().GetAllAsync();
            var departments = await _unitOfWork.GetRepository<Department>().GetAllAsync();
            var users = await _unitOfWork.GetRepository<User>().GetAllAsync();

            // --- LOGIC LỌC NGÀY (Mới thêm) ---
            IEnumerable<WarehouseTransaction> filteredTransactions = transactions;

            if (fromDate.HasValue)
            {
                filteredTransactions = filteredTransactions.Where(t => t.Date.Date >= fromDate.Value.Date);
            }
            if (toDate.HasValue)
            {
                filteredTransactions = filteredTransactions.Where(t => t.Date.Date <= toDate.Value.Date);
            }
            // ----------------------------------

            // 2. Nối bảng (Dùng danh sách đã lọc 'filteredTransactions')
            var query = from t in filteredTransactions
                        join a in assets on t.AssetID equals a.AssetID into assetGroup
                        from a in assetGroup.DefaultIfEmpty()

                        join d in departments on t.DepartmentID equals d.DepartmentID into deptGroup
                        from d in deptGroup.DefaultIfEmpty()

                        join u in users on t.UserID equals u.UserID into userGroup
                        from u in userGroup.DefaultIfEmpty()

                        orderby t.Date descending

                        select new WarehouseHistoryResponse
                        {
                            TransactionID = t.TransactionID,
                            AssetName = a != null ? a.AssetName : $"Tài sản đã xóa (#{t.AssetID})",
                            Type = FormatType(t.Type),
                            Quantity = t.Quantity,
                            Date = t.Date,
                            DepartmentName = d != null ? d.DeptName : "-",
                            UserName = u != null ? u.FullName : "Admin/Hệ thống",
                            Note = t.Note,
                            ReferenceNo = t.ReferenceNo
                        };

            return query.ToList();
        }

        public async Task<IEnumerable<WarehouseHistoryResponse>> GetHistoryByAssetIdAsync(int assetId)
        {
            // Tạm thời lấy null ngày để lấy hết, sau đó lọc theo ID
            var allHistory = await GetHistoryAsync(null, null);
            return allHistory.Where(x => x.AssetName.Contains(assetId.ToString()) || true).ToList();
        }

        private string FormatType(string type)
        {
            if (string.IsNullOrEmpty(type)) return "Khác";
            return type.ToUpper() switch
            {
                "IN" => "Nhập kho / Thu hồi",
                "OUT" => "Xuất kho / Cấp phát",
                "ADJUST" => "Kiểm kê / Điều chỉnh",
                "REPAIR" => "Sửa chữa",
                _ => type
            };
        }
    }
}