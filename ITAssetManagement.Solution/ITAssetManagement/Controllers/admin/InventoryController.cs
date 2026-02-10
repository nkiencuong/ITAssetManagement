using ITAssetManagement.Service.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace ITAssetManagement.API.Controllers.Admin
{
    // Đường dẫn: api/admin/inventory
    [Route("api/admin/inventory")]
    [ApiController]
    public class InventoryController : ControllerBase
    {
        private readonly IInventoryService _inventoryService;

        public InventoryController(IInventoryService inventoryService)
        {
            _inventoryService = inventoryService;
        }

        // GET: Lấy danh sách lịch sử kiểm kê
        [HttpGet]
        public async Task<IActionResult> GetHistory()
        {
            var data = await _inventoryService.GetAllChecksAsync();
            return Ok(data);
        }
    }
}