using ITAssetManagement.Models.Entitis;
using ITAssetManagement.Repo.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace ITAssetManagement.Controllers.Admin
{
    [Route("api/admin/repairs")]
    [ApiController]
    public class RepairsController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public RepairsController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // GET: api/admin/repairs
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var query = _unitOfWork.GetRepository<RepairTicket>().GetAll()
                                   .Include(x => x.Asset)
                                   .Include(x => x.Department)
                                   .AsNoTracking();

            var roleClaim = User.FindFirst(ClaimTypes.Role);
            var userIdClaim = User.FindFirst("UserID") ?? User.FindFirst(ClaimTypes.NameIdentifier);
            int currentUserId = 0;
            if (userIdClaim != null) int.TryParse(userIdClaim.Value, out currentUserId);

            // 🚀 ĐÃ SỬA: Bổ sung thêm quyền SuperAdmin vào đây để Sếp được thấy tất cả
            bool isBoss = roleClaim != null && (roleClaim.Value == "Admin" || roleClaim.Value == "SuperAdmin");

            if (isBoss)
            {
                // Sếp thì lấy hết
                query = query.OrderByDescending(x => x.CreatedDate);
            }
            else
            {
                // Nhân viên thì chỉ lấy phiếu của mình 
                query = query.Where(x => x.UserID == currentUserId || x.AssignedToUserID == currentUserId)
                             .OrderByDescending(x => x.CreatedDate);
            }

            var data = await query.ToListAsync();

            // 🚀 ĐÃ SỬA: SuperAdmin cũng được quyền xem Chi phí (Cost)
            if (!isBoss)
            {
                data.ForEach(x => x.Cost = 0);
            }

            return Ok(data);
        }
    }
}