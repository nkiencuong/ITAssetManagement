using ITAssetManagement.Models.Entities;
using ITAssetManagement.Models.Entitis;
using ITAssetManagement.Repo.Interfaces; // 👈 Thêm
using ITAssetManagement.Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ITAssetManagement.API.Controllers.Api
{
    [Route("api/users")]
    [ApiController]
    public class ApiUsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IUnitOfWork _unitOfWork; // 👈 Thêm UnitOfWork

        // 👇 Inject thêm UnitOfWork vào constructor
        public ApiUsersController(IUserService userService, IUnitOfWork unitOfWork)
        {
            _userService = userService;
            _unitOfWork = unitOfWork;
        }

        // 1. POST: Đăng nhập (Giữ nguyên hoặc bỏ qua nếu đã có AuthController)

        // 2. GET: Xem thông tin bản thân
        [HttpGet("profile/{username}")]
        [Authorize]
        public async Task<IActionResult> GetMyProfile(string username)
        {
            var user = await _userService.GetUserByUsernameAsync(username);
            if (user == null) return NotFound();
            return Ok(new { user.FullName, user.Email, user.Role });
        }

        // 3. PUT: Đổi mật khẩu cá nhân (User tự đổi)
        [HttpPut("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest req)
        {
            try
            {
                // Lấy ID người đang đăng nhập
                var userIdClaim = User.FindFirst("UserID") ?? User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null) return Unauthorized();
                int userId = int.Parse(userIdClaim.Value);

                var repo = _unitOfWork.GetRepository<User>();
                var user = await repo.GetByIdAsync(userId);
                if (user == null) return NotFound("User không tồn tại");

                // Kiểm tra mật khẩu cũ (Nếu hệ thống có mã hóa thì phải mã hóa req.OldPassword trước khi so sánh)
                if (user.PasswordHash != req.OldPassword)
                {
                    return BadRequest("Mật khẩu cũ không đúng!");
                }

                user.PasswordHash = req.NewPassword;
                repo.Update(user);
                await _unitOfWork.CommitAsync();

                return Ok(new { message = "Đổi mật khẩu thành công!" });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // Class DTO nhận dữ liệu
        public class ChangePasswordRequest
        {
            public string OldPassword { get; set; }
            public string NewPassword { get; set; }
        }
    }
}