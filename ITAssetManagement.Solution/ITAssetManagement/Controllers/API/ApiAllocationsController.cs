using ITAssetManagement.Request.Allocations;
using ITAssetManagement.Service.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ITAssetManagement.Controllers.Api
{
    [Route("api/allocations")] // Đường dẫn API
    [ApiController]
    public class ApiAllocationsController : ControllerBase
    {
        private readonly IAllocationService _allocationService;

        public ApiAllocationsController(IAllocationService allocationService)
        {
            _allocationService = allocationService;
        }

        // POST: api/allocations
        // Input: Danh sách ID tài sản + ID phòng ban
        [HttpPost]
        public async Task<IActionResult> AllocateAssets([FromBody] AllocateAssetsRequest request)
        {
            // 1. Validate Model (Các trường required, type...)
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                // 2. Gọi Service xử lý
                var result = await _allocationService.AllocateAssetsAsync(request);

                if (result)
                {
                    return Ok(new { message = "Phân bổ tài sản thành công!" });
                }
                else
                {
                    return BadRequest(new { message = "Phân bổ thất bại (Lỗi không xác định)." });
                }
            }
            catch (Exception ex)
            {
                // 3. Bắt lỗi logic từ Service ném ra (Ví dụ: Máy không trong kho...)
                // Trả về lỗi 400 kèm thông báo chi tiết để hiện lên Web/Postman
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}