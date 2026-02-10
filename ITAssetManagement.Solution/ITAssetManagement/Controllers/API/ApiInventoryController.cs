using ITAssetManagement.Models.Entities;
using ITAssetManagement.Models.Entitis;
using ITAssetManagement.Service.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace ITAssetManagement.API.Controllers.Api
{
    // Đường dẫn: api/inventory (Giữ nguyên cho App gọi cho dễ)
    [Route("api/inventory")]
    [ApiController]
    public class ApiInventoryController : ControllerBase
    {
        private readonly IInventoryService _inventoryService;

        public ApiInventoryController(IInventoryService inventoryService)
        {
            _inventoryService = inventoryService;
        }

        // POST: Gửi kết quả kiểm kê từ App về Server
        [HttpPost]
        public async Task<IActionResult> SubmitCheck([FromBody] InventoryCheck check)
        {
            if (check == null) return BadRequest();
            await _inventoryService.CreateCheckAsync(check);
            return Ok(new { message = "Kiểm kê thành công!" });
        }
    }
}