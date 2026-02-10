using ITAssetManagement.Service.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace ITAssetManagement.Controllers.admin
{
    // Đường dẫn: api/admin/auditlog
    [Route("api/admin/auditlog")]
    [ApiController]
    public class AuditLogController : ControllerBase
    {
        private readonly IAuditLogService _auditLogService;

        public AuditLogController(IAuditLogService auditLogService)
        {
            _auditLogService = auditLogService;
        }

        // GET: Xem nhật ký hệ thống
        [HttpGet]
        public async Task<IActionResult> GetLogs()
        {
            var data = await _auditLogService.GetLogsAsync();
            return Ok(data);
        }
    }
}