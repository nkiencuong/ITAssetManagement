using ITAssetManagement.Models.Entities;
using ITAssetManagement.Models.Entitis;
using ITAssetManagement.Response;
using ITAssetManagement.Response.InventoryCheck;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ITAssetManagement.Service.Interfaces
{
    public interface IInventoryService
    {
        // Dành cho Admin: Xem lịch sử kiểm kê
        Task<IEnumerable<InventoryCheckResponse>> GetAllChecksAsync();

        // Dành cho Client/App: Gửi kết quả kiểm kê
        Task CreateCheckAsync(InventoryCheck check);
    }
}