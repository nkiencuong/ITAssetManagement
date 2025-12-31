using ITAssetManagement.Service.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ITAssetManagement.Controllers.Admin
{
    // Route vẫn giữ nguyên để phân biệt trên URL: api/admin/assets
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
        public async Task<IActionResult> GetAll()
        {
            var result = await _assetService.GetAllAssetsAsync();
            return Ok(result);
        }
    }
}