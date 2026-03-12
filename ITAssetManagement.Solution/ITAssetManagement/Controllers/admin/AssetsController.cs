using ITAssetManagement.Service.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ITAssetManagement.Controllers.Admin
{
    // Route: api/admin/assets
    [Route("api/admin/assets")]
    [ApiController]
    public class AssetsController : ControllerBase
    {
        private readonly IAssetService _assetService;

        public AssetsController(IAssetService assetService)
        {
            _assetService = assetService;
        }

        // GET: api/admin/assets
        // Chức năng: Lấy danh sách cho Admin xem
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] DateTime? from, [FromQuery] DateTime? to)
        {
            // Truyền from, to vào cho thằng culi Service nó tính toán thuật toán cộng dồn
            var result = await _assetService.GetAllAssetsAsync(from, to);
            return Ok(result);
        }

        // --- MỚI THÊM: LẤY CHI TIẾT ---
        // GET: api/admin/assets/{id}
        // Chức năng: Lấy thông tin 1 tài sản để xem chi tiết hoặc đổ vào form sửa
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var asset = await _assetService.GetAssetByIdAsync(id);
            if (asset == null)
            {
                return NotFound(new { message = "Không tìm thấy tài sản" });
            }
            return Ok(asset);
        }
        // --- 🚀 MỚI THÊM: LẤY MÁY THEO KHOA PHÒNG ---
        // GET: api/admin/assets/department/{departmentId}
        // Chức năng: Lấy danh sách máy đang dùng của 1 khoa cụ thể để báo hỏng
        [HttpGet("department/{departmentId}")]
        public async Task<IActionResult> GetByDepartment(int departmentId)
        {
            var result = await _assetService.GetAssetsByDepartmentAsync(departmentId);
            return Ok(result);
        }
    }
}