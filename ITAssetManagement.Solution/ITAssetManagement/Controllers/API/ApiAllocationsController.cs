using ITAssetManagement.Request.Allocations;
using ITAssetManagement.Service.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Security.Claims; // Cần thiết để lấy UserID
using System.Threading.Tasks;

namespace ITAssetManagement.Controllers.Api
{
    [Route("api/allocations")]
    [ApiController]
    public class ApiAllocationsController : ControllerBase
    {
        private readonly IAllocationService _allocationService;

        public ApiAllocationsController(IAllocationService allocationService)
        {
            _allocationService = allocationService;
        }

        // POST: api/allocations
        // Chức năng: Cấp phát tài sản (Đã cập nhật logic lấy UserID chuẩn)
        [HttpPost]
        public async Task<IActionResult> AllocateAssets([FromBody] AllocateAssetsRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                // 1. Lấy UserID chuẩn từ Token (Ưu tiên claim "UserID" chúng ta đã thêm ở AuthController)
                int actionUserId = 1; // Mặc định Admin
                var userIdClaim = User.FindFirst("UserID") ?? User.FindFirst(ClaimTypes.NameIdentifier);

                if (userIdClaim != null)
                {
                    int.TryParse(userIdClaim.Value, out actionUserId);
                }

                // 2. Gọi Service (Service sẽ tự xử lý logic Số lượng dựa vào request)
                var result = await _allocationService.AllocateAssetsAsync(request, actionUserId);

                if (result)
                {
                    return Ok(new { message = "Phân bổ tài sản thành công!" });
                }
                else
                {
                    return BadRequest(new { message = "Phân bổ thất bại." });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // POST: api/allocations/return/{id}
        // Chức năng: Thu hồi tài sản
        [HttpPost("return/{id}")]
        public async Task<IActionResult> ReturnAsset(int id, [FromBody] ReturnRequest request)
        {
            try
            {
                // Gọi Service thu hồi
                var result = await _allocationService.ReturnAssetAsync(id, request.Note, request.ReturnDate);

                if (result)
                {
                    return Ok(new { message = "Thu hồi tài sản thành công!" });
                }
                else
                {
                    return BadRequest(new { message = "Thu hồi thất bại." });
                }
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
        // PUT: api/allocations/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAllocation(int id, [FromBody] EditAllocationRequest request)
        {
            try
            {
                int actionUserId = 1; // Mặc định Admin
                var userIdClaim = User.FindFirst("UserID") ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                if (userIdClaim != null) int.TryParse(userIdClaim.Value, out actionUserId);

                var result = await _allocationService.UpdateAllocationAsync(id, request, actionUserId);
                if (result) return Ok(new { message = "Sửa phiếu cấp phát thành công!" });
                return BadRequest(new { message = "Sửa thất bại." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }

    // Class DTO nhận dữ liệu JSON khi thu hồi
    public class ReturnRequest
    {
        public string Note { get; set; } = string.Empty;
        public DateTime ReturnDate { get; set; } = DateTime.Now;
    }
}