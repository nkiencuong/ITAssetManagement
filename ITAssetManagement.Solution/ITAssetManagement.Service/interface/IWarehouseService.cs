using ITAssetManagement.Response.Warehouse;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITAssetManagement.Service.Interfaces
{
    public interface IWarehouseService
    {
        // Đã sửa: Thêm fromDate và toDate
        Task<IEnumerable<WarehouseHistoryResponse>> GetHistoryAsync(DateTime? fromDate, DateTime? toDate);

        Task<IEnumerable<WarehouseHistoryResponse>> GetHistoryByAssetIdAsync(int assetId);
    }
}
