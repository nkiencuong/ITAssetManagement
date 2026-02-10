using ITAssetManagement.Models.Entities; // (Check lại namespace Entity của bác)
using ITAssetManagement.Models.Entitis;
using ITAssetManagement.Repo.Interfaces;
using ITAssetManagement.Service.Interfaces;
using ITAssetManagement.Request.User; // 👈 THÊM DÒNG NÀY ĐỂ HẾT LỖI ĐỎ
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore; // Để dùng FirstOrDefaultAsync

namespace ITAssetManagement.Service.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;

        public AuthService(IUnitOfWork unitOfWork, IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _configuration = configuration;
        }

        public async Task<LoginResponse> LoginAsync(LoginRequest request)
        {
            // 1. Tìm user
            // Lưu ý: Đảm bảo Repo của bác có GetAll() trả về IQueryable để dùng được FirstOrDefaultAsync
            // Hoặc dùng: await _unitOfWork.GetRepository<User>().GetAsync(u => u.Username == request.Username);
            var users = await _unitOfWork.GetRepository<User>().GetAllAsync();
            var user = users.FirstOrDefault(u => u.Username == request.Username);

            // 2. Kiểm tra mật khẩu (Sau này nhớ dùng mã hóa nhé)
            if (user == null || user.PasswordHash != request.Password)
            {
                return null;
            }

            // 3. Tạo Token
            var token = GenerateJwtToken(user);

            // 4. Trả về kết quả (Khớp với class LoginResponse ở Bước 1)
            return new LoginResponse
            {
                UserID = user.UserID,
                Username = user.Username,
                FullName = user.FullName,
                Role = user.Role,
                Token = token,
                MustChangePassword = user.MustChangePassword // Trả cờ này về
            };
        }

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