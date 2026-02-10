using ITAssetManagement.Models.Entitis;
using ITAssetManagement.Response;
using ITAssetManagement.Response.AuditLog;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ITAssetManagement.Service.Interfaces
{
    public interface IAuditLogService
    {
        // Hàm cho Controller Admin gọi lấy danh sách
        Task<object> GetLogsAsync();

        // Hàm cho RepairService gọi ghi log
        Task CreateLogAsync(string action, string tableName, int recordId, string details, int userId);
    }
}