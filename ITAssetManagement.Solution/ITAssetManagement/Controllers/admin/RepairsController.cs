using ITAssetManagement.Models.Entitis;
using ITAssetManagement.Repo.Interfaces; // 👈 Cần cái này để truy cập DB trực tiếp
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ITAssetManagement.Controllers.Admin
{
    [Route("api/admin/repairs")]
    [ApiController]
    public class RepairsController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork; // Dùng trực tiếp UnitOfWork cho nhanh

        public RepairsController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // GET: api/admin/repairs
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            // 1. Chuẩn bị câu truy vấn (Chưa chạy xuống DB ngay)
            var query = _unitOfWork.GetRepository<RepairTicket>().GetAll()
                                   .Include(x => x.Asset)
                                   .Include(x => x.Department)
                                   .AsNoTracking(); // ⚡ TỐI ƯU 1: Giúp đọc dữ liệu nhanh hơn 30%

            // 2. Lấy thông tin người dùng
            var roleClaim = User.FindFirst(ClaimTypes.Role);
            var userIdClaim = User.FindFirst("UserID") ?? User.FindFirst(ClaimTypes.NameIdentifier);

            int currentUserId = 0;
            if (userIdClaim != null) int.TryParse(userIdClaim.Value, out currentUserId);

            // 3. ⚡ TỐI ƯU 2: Lọc ngay tại Database (SQL) chứ không lọc trên RAM
            if (roleClaim != null && roleClaim.Value == "Admin")
            {
                // Admin: Lấy hết, nhưng sắp xếp giảm dần theo ngày
                query = query.OrderByDescending(x => x.CreatedDate);
            }
            else
            {
                // User: Chỉ lấy đúng phiếu của mình ngay từ câu lệnh SQL
                query = query.Where(x => x.UserID == currentUserId)
                             .OrderByDescending(x => x.CreatedDate);
            }

            // 4. Bây giờ mới thực sự chạy xuống DB lấy dữ liệu
            var data = await query.ToListAsync();

            // 5. Xử lý bảo mật (Ẩn giá tiền)
            if (roleClaim == null || roleClaim.Value != "Admin")
            {
                data.ForEach(x => x.Cost = 0);
            }

            return Ok(data);
        }
    }
}