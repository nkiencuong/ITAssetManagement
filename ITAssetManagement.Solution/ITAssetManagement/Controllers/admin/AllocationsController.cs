using ITAssetManagement.Service.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ITAssetManagement.Controllers.Admin
{
    [Route("api/admin/allocations")] // Link riêng cho admin
    [ApiController]
    public class AllocationsController : ControllerBase
    {
        private readonly IAllocationService _service;

        public AllocationsController(IAllocationService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetHistory()
        {
            var data = await _service.GetAllocationHistoryAsync();
            return Ok(data);
        }

    }
}