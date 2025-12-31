using ITAssetManagement.Request.Assets;
using ITAssetManagement.Service.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ITAssetManagement.Controllers.Api
{
    [Route("api/assets")] // Đường dẫn API chuẩn: domain/api/assets
    [ApiController]
    public class ApiAssetsController : ControllerBase
    {
        private readonly IAssetService _assetService;

        public ApiAssetsController(IAssetService assetService)
        {
            _assetService = assetService;
        }

        // POST: api/assets
        // Chức năng: Nhập tài sản mới + Tự động sinh phiếu kiểm nhập
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateAssetRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var result = await _assetService.CreateAssetAsync(request);
                // Trả về 200 OK kèm dữ liệu vừa tạo
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // Các hàm PUT, DELETE sẽ viết tiếp ở đây sau này...
    }
}