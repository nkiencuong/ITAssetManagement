using ITAssetManagement.Models.Entitis;
using ITAssetManagement.Repo.Interfaces;
using ITAssetManagement.Request.Reports; // Nhớ trỏ vào DTO vừa tạo
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ITAssetManagement.API.Controllers
{
    [Route("api/dashboard")]
    [ApiController]
    [Authorize] // Phải đăng nhập mới xem được
    public class DashboardController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public DashboardController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetStats([FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
        {
            // 1. Xử lý ngày tháng (Nếu không chọn thì mặc định lấy tháng này)
            var start = fromDate ?? new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            var end = toDate ?? DateTime.Now;

            // Đảm bảo lấy hết ngày cuối cùng (đến 23:59:59)
            end = end.Date.AddDays(1).AddTicks(-1);

            // 2. Lấy dữ liệu
            var repo = _unitOfWork.GetRepository<RepairTicket>();
            var allTickets = await repo.GetAllAsync();

            // 3. Lọc theo ngày
            var filteredTickets = allTickets
                .Where(t => t.CreatedDate >= start && t.CreatedDate <= end)
                .ToList();

            // 4. Tính toán
            int total = filteredTickets.Count();
            int completed = filteredTickets.Count(t => t.Status == 2); // 2: Đã xong

            // Đang sửa = Tổng - (Đã xong + Hủy) 
            // Hoặc: Status == 0 (Mới) || Status == 1 (Đang sửa)
            int processing = filteredTickets.Count(t => t.Status == 0 || t.Status == 1);

            double rate = 0;
            if (total > 0)
            {
                rate = Math.Round(((double)completed / total) * 100, 1);
            }

            // 5. Trả về
            return Ok(new DashboardStatsResponse
            {
                TotalReceived = total,
                Processing = processing,
                Completed = completed,
                CompletionRate = rate
            });
        }
    }
}