using ITAssetManagement.Models.Entities;
using ITAssetManagement.Models.Entitis;
using ITAssetManagement.Service.Interfaces;
using ITAssetManagement.Repo.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ITAssetManagement.API.Controllers.Admin
{
    [Route("api/admin/users")]
    [ApiController]
    [Authorize(Roles = "SuperAdmin")] // Nếu có bảo mật JWT thì bác mở dòng này ra
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IUnitOfWork _unitOfWork;

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
        public async Task<IActionResult> CreateUser([FromBody] UserCreateReq req)
        {
            // Bốc dữ liệu từ giỏ (req) đổ sang thực thể Database (newUser)
            var newUser = new User
            {
                Username = req.Username,
                PasswordHash = req.PasswordHash,
                FullName = req.FullName,
                Role = req.Role,
                DepartmentID = req.DepartmentID,
                // Ép cứng 2 thằng này bằng null để Database không kêu ca
                Email = null,
                PhoneNumber = null
            };

            var result = await _userService.CreateUserAsync(newUser);
            if (!result) return BadRequest("Tên đăng nhập đã tồn tại.");
            return Ok(new { message = "Thêm thành công" });
        }

        // 3. DELETE: Xóa
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var result = await _userService.DeleteUserAsync(id);
            if (!result) return NotFound();
            return Ok("Đã xóa");
        }

        // 4. POST: Admin đổi mật khẩu cho user (Reset Password)
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] AdminResetPasswordReq req)
        {
            try
            {
                var repo = _unitOfWork.GetRepository<User>();
                var user = await repo.GetByIdAsync(req.UserID);
                if (user == null) return NotFound("User không tồn tại");

                user.PasswordHash = req.NewPassword;
                repo.Update(user);
                await _unitOfWork.CommitAsync();

                return Ok(new { message = "Đã đặt lại mật khẩu thành công!" });
            }
            catch (Exception ex)
            {
                return BadRequest("Lỗi: " + ex.Message);
            }
        }

        // 🚀 5. PUT: API CẬP NHẬT FULL THÔNG TIN (Họ tên, SĐT, Khoa, Quyền)
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UserUpdateReq req)
        {
            try
            {
                // 🚀 TẮT CHECK LỖI EMAIL
                ModelState.Remove("Email");

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var repo = _unitOfWork.GetRepository<User>();
                var user = await repo.GetByIdAsync(id);
                if (user == null) return NotFound("User không tồn tại");

                // Cập nhật các trường thông tin mới
                user.FullName = req.FullName;
                user.PhoneNumber = req.PhoneNumber;
                user.DepartmentID = req.DepartmentID;

                // Check quyền hợp lệ rồi mới cho đổi
                var validRoles = new[] { "User", "Admin", "SuperAdmin" };
                if (validRoles.Contains(req.Role))
                {
                    user.Role = req.Role;
                }

                repo.Update(user);
                await _unitOfWork.CommitAsync();

                return Ok(new { message = "Cập nhật thông tin thành công!" });
            }
            catch (Exception ex)
            {
                return BadRequest("Lỗi: " + ex.Message);
            }
        }

        // ================= CÁC LỚP REQUEST (DTO) =================

        public class AdminResetPasswordReq
        {
            public int UserID { get; set; }
            public string NewPassword { get; set; }
        }

        // 🚀 Lớp Request để hứng dữ liệu TẠO MỚI từ Web gửi lên
        public class UserCreateReq
        {
            public string Username { get; set; }
            public string PasswordHash { get; set; }
            public string FullName { get; set; }
            public string Role { get; set; }
            public int DepartmentID { get; set; }
        }

        // 🚀 Lớp Request để hứng dữ liệu SỬA THÔNG TIN từ Web gửi lên
        public class UserUpdateReq
        {
            public string FullName { get; set; }
            public string PhoneNumber { get; set; }
            public string Role { get; set; }
            public int DepartmentID { get; set; }
        }
    }
}