using ITAssetManagement.Models.Entities;
using ITAssetManagement.Models.Entitis;
using ITAssetManagement.Service.Interfaces;
using ITAssetManagement.Repo.Interfaces; // 👈 Thêm
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ITAssetManagement.API.Controllers.Admin
{
    [Route("api/admin/users")]
    [ApiController]
    // [Authorize(Roles = "Admin")] 
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IUnitOfWork _unitOfWork; // 👈 Thêm UnitOfWork

        public UsersController(IUserService userService, IUnitOfWork unitOfWork)
        {
            _userService = userService;
            _unitOfWork = unitOfWork;
        }

        // 1. GET: Lấy danh sách
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var users = await _userService.GetAllUsersAsync();
            return Ok(users);
        }

        // 2. POST: Tạo mới
        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] User newUser)
        {
            var result = await _userService.CreateUserAsync(newUser);
            if (!result) return BadRequest("Tên đăng nhập đã tồn tại.");
            return Ok("Thêm thành công");
        }

        // 3. DELETE: Xóa
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var result = await _userService.DeleteUserAsync(id);
            if (!result) return NotFound();
            return Ok("Đã xóa");
        }

        // 👇 4. POST: Admin đổi mật khẩu cho user (Reset Password)
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] AdminResetPasswordReq req)
        {
            try
            {
                var repo = _unitOfWork.GetRepository<User>();
                var user = await repo.GetByIdAsync(req.UserID);
                if (user == null) return NotFound("User không tồn tại");

                user.PasswordHash = req.NewPassword; // Reset thẳng, không cần pass cũ

                repo.Update(user);
                await _unitOfWork.CommitAsync();

                return Ok(new { message = "Đã đặt lại mật khẩu thành công!" });
            }
            catch (Exception ex)
            {
                return BadRequest("Lỗi: " + ex.Message);
            }
        }

        public class AdminResetPasswordReq
        {
            public int UserID { get; set; }
            public string NewPassword { get; set; }
        }
    }
}