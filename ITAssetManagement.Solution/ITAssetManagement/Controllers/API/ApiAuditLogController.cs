using Microsoft.AspNetCore.Mvc;
using ITAssetManagement.Service.Interfaces;
using ITAssetManagement.Models.Entitis;
using System.Threading.Tasks;

namespace ITAssetManagement.API.Controllers.Api
{
    [Route("api/auditlog")]
    [ApiController]
    public class ApiAuditLogController : ControllerBase
    {
        private readonly IAuditLogService _auditLogService;

        public ApiAuditLogController(IAuditLogService auditLogService)
        {
            _auditLogService = auditLogService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateClientLog([FromBody] AuditLog log)
        {
            if (log == null)
            {
                return BadRequest("Dữ liệu log không hợp lệ.");
            }

            // 👇 ĐÃ SỬA: Xóa "?? 0" ở UserID vì UserID trong Model của bạn là int (không null)
            await _auditLogService.CreateLogAsync(
                action: log.Action ?? "Client Log",
                tableName: log.TableName ?? "Unknown",

                // RecordID là int? (có thể null) nên cần ?? 0
                recordId: log.RecordID ?? 0,

                details: log.Details ?? "No details",

                // UserID là int (không thể null) nên truyền trực tiếp
                userId: log.UserID
            );

            return Ok(new
            {
                Success = true,
                Message = "Đã ghi nhận log hệ thống."
            });
        }
    }
}