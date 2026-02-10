using ITAssetManagement.Models.Entities;
using ITAssetManagement.Models.Entitis;
using ITAssetManagement.Repo.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace ITAssetManagement.API.Controllers
{
    [Route("api/admin/auth")]
    [ApiController]
    [Authorize(Roles = "Admin")] // Chỉ Admin mới vào được đây
    public class AuthController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public AuthController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // --- LẤY DANH SÁCH USER (GET) ---
        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _unitOfWork.GetRepository<User>().GetAllAsync();
            var depts = await _unitOfWork.GetRepository<Department>().GetAllAsync();

            var result = users.Select(u => new
            {
                u.UserID,
                u.Username,
                u.FullName,
                u.Email,
                u.Role,
                u.CreatedDate,
                u.PhoneNumber, // Hiển thị thêm SĐT cho Admin thấy
                DepartmentName = depts.FirstOrDefault(d => d.DepartmentID == u.DepartmentID)?.DeptName ?? "Chưa phân khoa"
            });

            return Ok(result);
        }
    }
}