using Microsoft.AspNetCore.Mvc;
using ITAssetManagement.Repo.Interfaces;
using ITAssetManagement.Models.Entitis; // Giữ nguyên namespace Entitis của bạn
using Xceed.Words.NET;
using Xceed.Document.NET;

namespace ITAssetManagement.Server.Controllers
{
    // 👇 ĐỔI ROUTE: Dành riêng cho Admin tải file
    [Route("admin/report")]
    [ApiController]
    public class ReportController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _env;

        public ReportController(IUnitOfWork unitOfWork, IWebHostEnvironment env)
        {
            _unitOfWork = unitOfWork;
            _env = env;
        }

        // 👇 GIỮ NGUYÊN LOGIC CŨ CỦA BẠN Ở ĐÂY
        [HttpGet("export-word/{ticketId}")]
        public async Task<IActionResult> ExportWord(int ticketId)
        {
            try
            {
                var ticket = await _unitOfWork.GetRepository<RepairTicket>().GetByIdAsync(ticketId);
                if (ticket == null) return NotFound("Không tìm thấy phiếu.");

                string webRootPath = _env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot");
                string templatePath = Path.Combine(webRootPath, "templates", "Bien_Ban_Hien_Trang.docx");

                if (!System.IO.File.Exists(templatePath)) return NotFound("Không tìm thấy file mẫu.");

                using (var stream = new MemoryStream())
                {
                    using (var doc = DocX.Load(templatePath))
                    {
                        // --- PHẦN CODE BẠN YÊU CẦU GIỮ NGUYÊN ---

                        // 1. Ngày giờ
                        doc.ReplaceText("{{Ngay}}", ticket.CreatedDate.Day.ToString("00"));
                        doc.ReplaceText("{{Thang}}", ticket.CreatedDate.Month.ToString("00"));
                        doc.ReplaceText("{{Nam}}", ticket.CreatedDate.Year.ToString());
                        doc.ReplaceText("{{Gio}}", ticket.CreatedDate.Hour.ToString("00"));

                        // 2. ĐIỀN TÊN USER BÁO HỎNG (LẤY TỪ DB)
                        string tenNguoiBao = !string.IsNullOrEmpty(ticket.ReporterName) ? ticket.ReporterName : "....................";
                        string chucVu = !string.IsNullOrEmpty(ticket.ReporterPosition) ? ticket.ReporterPosition : "....................";

                        doc.ReplaceText("{{Nguoi_Bao}}", tenNguoiBao);
                        doc.ReplaceText("{{Chuc_Vu}}", chucVu);

                        // 3. Thông tin chung
                        doc.ReplaceText("{{Ten_Khoa}}", ticket.Department?.DeptName ?? "....................");
                        doc.ReplaceText("{{Ten_Thiet_Bi}}", ticket.Asset?.AssetName ?? "....................");
                        doc.ReplaceText("{{Model}}", ticket.Asset?.Model ?? "(Không có)");
                        doc.ReplaceText("{{Tinh_Trang}}", ticket.Description ?? "");
                        doc.ReplaceText("{{Bien_Phap}}", ticket.Solution ?? "Đã kiểm tra và thay thế linh kiện");

                        // 4. Linh kiện
                        string linhKienStr = "";
                        if (ticket.RepairDetails != null && ticket.RepairDetails.Any())
                        {
                            foreach (var item in ticket.RepairDetails)
                            {
                                linhKienStr += $"- {item.Asset?.AssetName} (SL: {item.Quantity})\n";
                            }
                        }
                        else
                        {
                            linhKienStr = "(Không thay thế vật tư)";
                        }
                        doc.ReplaceText("{{DS_Linh_Kien}}", linhKienStr);

                        // --- KẾT THÚC PHẦN CODE CŨ ---

                        doc.SaveAs(stream);
                    }
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.wordprocessingml.document", $"BienBan_SC_{ticketId}.docx");
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi xử lý Word: {ex.Message}");
            }
        }
    }
}