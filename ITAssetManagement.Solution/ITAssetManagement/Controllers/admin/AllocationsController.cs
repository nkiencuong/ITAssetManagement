using ITAssetManagement.Service.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace ITAssetManagement.Controllers.Admin
{
    [Route("api/admin/allocations")]
    [ApiController]
    public class AllocationsController : ControllerBase
    {
        private readonly IAllocationService _service;

        public AllocationsController(IAllocationService service)
        {
            _service = service;
        }

        // GET: api/admin/allocations
        // Chức năng: Lấy danh sách lịch sử cấp phát
        [HttpGet]
        public async Task<IActionResult> GetHistory()
        {
            var data = await _service.GetAllocationHistoryAsync();
            return Ok(data);
        }
    }
}