using Microsoft.AspNetCore.Mvc;
using ITAssetManagement.Models.Entitis;
using ITAssetManagement.Repo.Interfaces; // Gọi Interface Repository

namespace ITAssetManagement.Controllers.Admin
{
    [Route("api/admin/departments")]
    [ApiController]
    public class DepartmentsController : ControllerBase
    {
        // Thay vì dùng DbContext trực tiếp, ta dùng Repository cho chuyên nghiệp
        private readonly IGenericRepository<Department> _deptRepo;

        public DepartmentsController(IGenericRepository<Department> deptRepo)
        {
            _deptRepo = deptRepo;
        }

        // API: GET api/admin/departments
        [HttpGet]
        public async Task<IActionResult> GetDepartments()
        {
            try
            {
                // Lấy toàn bộ danh sách phòng ban
                var depts = await _deptRepo.GetAllAsync();

                // Trả về danh sách (JSON sẽ có dạng: { "departmentID": 1, "deptName": "Khoa Nội"... })
                return Ok(depts);
            }
            catch (Exception ex)
            {
                return BadRequest("Lỗi lấy danh sách phòng: " + ex.Message);
            }
        }
    }
}