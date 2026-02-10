using ITAssetManagement.Models.Entities;
using ITAssetManagement.Models.Entitis;
using ITAssetManagement.Repo.Interfaces;
using ITAssetManagement.Request.User; // Trỏ vào DTO
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ITAssetManagement.API.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class ApiAuthController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;

        public ApiAuthController(IUnitOfWork unitOfWork, IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _configuration = configuration;
        }

        // --- 1. ĐĂNG NHẬP (POST) ---
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (request == null || string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
                return BadRequest("Vui lòng nhập đầy đủ thông tin.");

            var users = await _unitOfWork.GetRepository<User>().GetAllAsync();
            var user = users.FirstOrDefault(u => u.Username == request.Username);

            // Kiểm tra mật khẩu (Lưu ý: Bác đang so sánh chuỗi thường, sau này nên mã hóa MD5/Bcrypt nhé)
            if (user == null || user.PasswordHash != request.Password)
                return Unauthorized("Sai tài khoản hoặc mật khẩu!");

            var tokenString = GenerateJwtToken(user);

            // 👇 TRẢ VỀ CỜ MustChangePassword ĐỂ CLIENT BIẾT MÀ CHẶN
            return Ok(new LoginResponse
            {
                Token = tokenString,
                FullName = user.FullName,
                Role = user.Role,
                MustChangePassword = user.MustChangePassword
            });
        }

        // --- 2. CẬP NHẬT LẦN ĐẦU (POST) ---
        [HttpPost("first-login-update")]
        [Authorize] // Phải có Token mới được gọi
        public async Task<IActionResult> FirstLoginUpdate([FromBody] FirstLoginRequest request)
        {
            // Lấy Username từ Token của người đang đăng nhập
            var username = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (username == null) return Unauthorized();

            var userRepo = _unitOfWork.GetRepository<User>();
            var allUsers = await userRepo.GetAllAsync();
            var user = allUsers.FirstOrDefault(u => u.Username == username);

            if (user == null) return NotFound("Không tìm thấy user.");

            // 1. Check mật khẩu cũ
            if (user.PasswordHash != request.OldPassword)
                return BadRequest("Mật khẩu cũ không đúng!");

            // 2. Check mật khẩu mới
            if (request.NewPassword != request.ConfirmPassword)
                return BadRequest("Mật khẩu xác nhận không khớp!");

            // 3. Cập nhật vào DB
            user.PasswordHash = request.NewPassword; // Đổi pass
            user.PhoneNumber = request.PhoneNumber;  // Lưu SĐT
            user.MustChangePassword = false;         // Tắt cờ bắt buộc đổi

            userRepo.Update(user);
            await _unitOfWork.CompleteAsync();

            return Ok(new { message = "Cập nhật thành công!" });
        }

        // --- 3. HÀM TẠO TOKEN (Giữ nguyên) ---
        private string GenerateJwtToken(User user)
        {
            var jwtKey = _configuration["Jwt:Key"];
            var jwtIssuer = _configuration["Jwt:Issuer"];
            var jwtAudience = _configuration["Jwt:Audience"];

            if (string.IsNullOrEmpty(jwtKey)) jwtKey = "Khoa_Bi_Mat_Nay_Can_Phai_Dai_Hon_32_Ky_Tu_De_Bao_Mat_Nhe_123";
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[] {
                new Claim(ClaimTypes.NameIdentifier, user.Username),
                new Claim(ClaimTypes.Name, user.FullName ?? "Người dùng"),
                new Claim(ClaimTypes.Role, user.Role ?? "User"),
                new Claim("UserID", user.UserID.ToString())
            };

            var token = new JwtSecurityToken(issuer: jwtIssuer, audience: jwtAudience, claims: claims, expires: DateTime.Now.AddHours(4), signingCredentials: credentials);
            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}