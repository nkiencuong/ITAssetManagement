using ITAssetManagement.Models.Entitis;
using ITAssetManagement.Service.Interfaces;
using ITAssetManagement.Repo.Interfaces;
using ITAssetManagement.Request.RepairTickets;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using MiniSoftware;
using System.Collections.Generic;
using System.IO;
using System;
using System.Threading.Tasks;
using System.Security.Claims;

namespace ITAssetManagement.Controllers.Api
{
    [Route("api/repairs")]
    [ApiController]
    public class ApiRepairsController : ControllerBase
    {
        private readonly IRepairService _repairService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _env;

        public ApiRepairsController(IRepairService repairService, IUnitOfWork unitOfWork, IWebHostEnvironment env)
        {
            _repairService = repairService;
            _unitOfWork = unitOfWork;
            _env = env;
        }

        // --- 1. POST: TẠO PHIẾU MỚI ---
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateRepairTicketRequest request)
        {
            if (request.AssetID == 0) return BadRequest("Vui lòng chọn tài sản bị hỏng!");

            try
            {
                // 👇 LẤY ID NGƯỜI DÙNG TỪ TOKEN
                int currentUserId = 0;

                // Tìm UserID trong các Claim chuẩn
                var claimId = User.FindFirst("UserID")?.Value
                           ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                           ?? User.FindFirst("sub")?.Value;

                if (!string.IsNullOrEmpty(claimId))
                {
                    int.TryParse(claimId, out currentUserId);
                }

                // Tạo phiếu mới
                var ticket = new RepairTicket
                {
                    AssetID = request.AssetID,
                    DepartmentID = request.DepartmentID,
                    Description = request.Description,
                    CreatedDate = request.ReportDate != default ? request.ReportDate : DateTime.Now,
                    ReporterName = request.ReporterName,
                    ReporterPosition = request.ReporterPosition,
                    DinhKemUrl = request.DinhKemUrl,
                    LoaiFile = request.LoaiFile,
                    Status = 0,
                    Cost = 0,
                    UserID = currentUserId // 👈 QUAN TRỌNG: Lưu ID người tạo
                };

                var result = await _repairService.CreateTicketAsync(ticket, currentUserId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                var realError = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                return BadRequest("Lỗi khi tạo phiếu: " + realError);
            }
        }

        // --- 2. POST: HOÀN THÀNH (Dành cho Admin) ---
        [HttpPost("{id}/complete")]
        public async Task<IActionResult> Complete(int id, [FromBody] CompleteRepairTicketRequest request)
        {
            try
            {
                int currentUserId = 1;
                var claimId = User.FindFirst("UserID")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!string.IsNullOrEmpty(claimId)) int.TryParse(claimId, out currentUserId);

                var result = await _repairService.CompleteRepairAsync(id, request.Solution, request.Parts, currentUserId);

                if (result) return Ok(new { message = "Đã cập nhật hoàn thành và trừ kho linh kiện!" });

                return BadRequest("Lỗi xử lý hoặc không tìm thấy phiếu.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // --- 3. PUT: CẬP NHẬT NỘI DUNG ---
        [HttpPut("{id}/update-content")]
        public async Task<IActionResult> UpdateContent(int id, [FromBody] RepairTicket request)
        {
            try
            {
                var repo = _unitOfWork.GetRepository<RepairTicket>();
                var ticket = await repo.GetByIdAsync(id);
                if (ticket == null) return NotFound("Không tìm thấy phiếu.");

                ticket.Description = request.Description;
                ticket.Solution = request.Solution;
                ticket.Note = request.Note;

                repo.Update(ticket);
                await _unitOfWork.CommitAsync();

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest("Lỗi update: " + ex.Message);
            }
        }

        // --- 4. GET: XUẤT FILE WORD ---
        [HttpGet("export-word/{id}")]
        public async Task<IActionResult> ExportWord(int id)
        {
            try
            {
                var repair = await _unitOfWork.GetRepository<RepairTicket>()
                                .GetAll()
                                .Include(x => x.Asset)
                                .Include(x => x.Department)
                                .FirstOrDefaultAsync(x => x.TicketID == id);

                if (repair == null) return NotFound("Không tìm thấy phiếu.");

                string templatePath = Path.Combine(_env.WebRootPath, "templates", "Bien_Ban_Hien_Trang.docx");
                if (!System.IO.File.Exists(templatePath))
                    return BadRequest($"Không tìm thấy file mẫu tại: {templatePath}");

                var time = DateTime.Now;
                var data = new Dictionary<string, object>
                {
                    ["Gio"] = time.Hour.ToString("00"),
                    ["Ngay"] = time.Day.ToString("00"),
                    ["Thang"] = time.Month.ToString("00"),
                    ["Nam"] = time.Year.ToString(),
                    ["Ten_Khoa"] = repair.Department?.DeptName ?? "....................",
                    ["Nguoi_Bao"] = repair.ReporterName ?? "....................",
                    ["Chuc_Vu"] = repair.ReporterPosition ?? "....................",
                    ["Ten_Thiet_Bi"] = repair.Asset?.AssetName ?? "....................",
                    ["Model"] = repair.Asset?.Model ?? "....................",
                    ["Tinh_Trang"] = repair.Description ?? "....................",
                    ["Bien_Phap"] = repair.Solution ?? "....................",
                    ["DS_Linh_Kien"] = repair.Note ?? "...................."
                };

                var memoryStream = new MemoryStream();
                MiniWord.SaveAsByTemplate(memoryStream, templatePath, data);
                memoryStream.Position = 0;

                return File(memoryStream,
                            "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                            $"BienBan_{id}.docx");
            }
            catch (Exception ex)
            {
                return BadRequest("Lỗi xuất file: " + ex.Message);
            }
        }
    }
}