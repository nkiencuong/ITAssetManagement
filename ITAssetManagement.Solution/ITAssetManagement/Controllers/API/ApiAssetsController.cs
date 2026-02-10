using ITAssetManagement.Models.Entitis; // Hoặc namespace chứa Asset model của bạn
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
        // Chức năng: Nhập tài sản mới
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateAssetRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                var result = await _assetService.CreateAssetAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // --- MỚI THÊM: SỬA (UPDATE) ---
        // PUT: api/assets/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] Asset request)
        {
            try
            {
                // Gọi Service xử lý cập nhật (Bạn cần đảm bảo Service có hàm UpdateAssetAsync)
                // Lưu ý: request ở đây có thể là Asset model hoặc UpdateAssetRequest tùy bạn định nghĩa
                var result = await _assetService.UpdateAssetAsync(id, request);

                if (result)
                    return Ok(new { message = "Cập nhật thành công" });
                else
                    return BadRequest(new { message = "Cập nhật thất bại" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // --- MỚI THÊM: XÓA (DELETE) ---
        // DELETE: api/assets/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                // Gọi Service xử lý xóa
                var result = await _assetService.DeleteAssetAsync(id);

                if (result)
                    return Ok(new { message = "Xóa thành công" });
                else
                    return NotFound(new { message = "Không tìm thấy tài sản để xóa" });
            }
            catch (Exception ex)
            {
                // Quan trọng: Bắt lỗi Foreign Key (Ràng buộc dữ liệu)
                // Nếu tài sản đã từng được cấp phát hoặc nhập kho, SQL sẽ không cho xóa cứng.
                // Trả về lỗi 400 để Frontend hiển thị thông báo.
                return BadRequest(new { message = "Không thể xóa tài sản này vì nó đang được sử dụng hoặc có lịch sử giao dịch!" });
            }
        }
    }
}