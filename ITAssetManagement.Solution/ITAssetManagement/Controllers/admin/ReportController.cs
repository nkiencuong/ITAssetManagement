using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ITAssetManagement.Repo.Interfaces;
using ITAssetManagement.Models;
using ITAssetManagement.Models.Entitis;
using ITAssetManagement.Response.Warehouse;
using ClosedXML.Excel;
using Xceed.Words.NET;
using Xceed.Document.NET;

namespace ITAssetManagement.API.Controllers
{
    [Route("api/report")]
    [ApiController]
    public class ReportController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _env;
        private readonly ApplicationDbContext _context;

        public ReportController(IUnitOfWork unitOfWork, IWebHostEnvironment env, ApplicationDbContext context)
        {
            _unitOfWork = unitOfWork;
            _env = env;
            _context = context;
        }

        [HttpGet("warehouse-export")]
        public async Task<IActionResult> GetWarehouseExportData([FromQuery] DateTime? from, [FromQuery] DateTime? to, [FromQuery] string? types, [FromQuery] int? deptId, [FromQuery] string? transType)
        {
            try
            {
                var data = await GetMergedDataSafe(from, to, types, deptId, transType);
                return Ok(data);
            }
            catch (Exception ex) { return BadRequest("Lỗi server: " + ex.Message); }
        }

        // =========================================================================
        // 🚀 2. XUẤT EXCEL THÔNG MINH (TÁCH RIÊNG 2 SHEET NHẬP - XUẤT)
        // =========================================================================
        [HttpGet("download-monthly-report")]
        public async Task<IActionResult> DownloadMonthlyReport([FromQuery] DateTime? from, [FromQuery] DateTime? to, [FromQuery] string? types, [FromQuery] int? deptId, [FromQuery] string? transType)
        {
            try
            {
                var data = await GetMergedDataSafe(from, to, types, deptId, transType);
                if (!data.Any()) return BadRequest("Không có dữ liệu trong khoảng thời gian và bộ lọc này.");

                using var workbook = new XLWorkbook();

                bool exportAll = string.IsNullOrEmpty(transType);
                bool exportNhap = exportAll || transType == "Nhập";
                bool exportXuat = exportAll || transType == "Xuất";

                // --- 1. NẾU CÓ DỮ LIỆU NHẬP -> VẼ SHEET NHẬP KHO ---
                if (exportNhap)
                {
                    var dataNhap = data.Where(x => x.Type == "Nhập kho").ToList();
                    if (dataNhap.Any()) DrawSheetNhapKho(workbook.Worksheets.Add("BaoCao_NhapKho"), dataNhap, from, to);
                }

                // --- 2. NẾU CÓ DỮ LIỆU XUẤT -> VẼ SHEET XUẤT KHO (MA TRẬN KHOA) ---
                if (exportXuat)
                {
                    var dataXuat = data.Where(x => x.Type != "Nhập kho").ToList();
                    if (dataXuat.Any()) DrawSheetXuatKho(workbook.Worksheets.Add("BaoCao_XuatKho"), dataXuat, from, to);
                }

                using var stream = new MemoryStream();
                workbook.SaveAs(stream);

                string filePrefix = string.IsNullOrEmpty(transType) ? "Nhap_Xuat" : (transType == "Nhập" ? "NhapKho" : "XuatKho");
                return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"BaoCao_{filePrefix}_T{to?.Month}.xlsx");
            }
            catch (Exception ex) { return BadRequest("Lỗi xuất báo cáo: " + ex.Message); }
        }

        // =========================================================================
        // 🛠 HÀM VẼ SHEET NHẬP KHO (Chỉ nhập về Phòng CNTT)
        // =========================================================================
        private void DrawSheetNhapKho(IXLWorksheet ws, List<WarehouseHistoryResponse> data, DateTime? from, DateTime? to)
        {
            ws.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

            ws.Cell(1, 1).Value = "BẢNG TỔNG HỢP NHẬP VẬT TƯ, LINH KIỆN CÔNG NGHỆ THÔNG TIN";
            ws.Cell(1, 1).Style.Font.Bold = true; ws.Cell(1, 1).Style.Font.FontSize = 15;
            ws.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Row(1).Height = 30;

            ws.Cell(2, 1).Value = $"Từ ngày: {from?.ToString("dd/MM/yyyy")} - Đến ngày: {to?.ToString("dd/MM/yyyy")}";
            ws.Cell(2, 1).Style.Font.Italic = true; ws.Cell(2, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Row(2).Height = 25;

            ws.Range(1, 1, 1, 7).Merge(); ws.Range(2, 1, 2, 7).Merge();

            // Header Nhập
            int col = 1;
            ws.Cell(4, col++).Value = "STT";
            ws.Cell(4, col++).Value = "Tên Thiết bị / Linh kiện";
            ws.Cell(4, col++).Value = "ĐVT";
            ws.Cell(4, col++).Value = "Đơn giá";
            ws.Cell(4, col++).Value = "Nơi nhận";
            ws.Cell(4, col++).Value = "Tổng SL Nhập";
            ws.Cell(4, col).Value = "Thành tiền";

            var headerRange = ws.Range(4, 1, 4, col);
            headerRange.Style.Font.Bold = true; headerRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#4F81BD");
            headerRange.Style.Font.FontColor = XLColor.White; headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Row(4).Height = 35;

            var groupedData = data.GroupBy(x => new { x.AssetName, x.AssetTypeName })
                .Select(g => new {
                    AssetName = g.Key.AssetName,
                    IsMayMoc = new[] { "Máy tính", "Máy in", "Máy Scan", "Các loại khác" }.Contains(g.Key.AssetTypeName),
                    Price = g.Max(i => i.Price),
                    TotalQty = g.Sum(i => i.Quantity),
                    TotalAmount = g.Sum(i => i.Quantity * i.Price)
                }).ToList();

            int currentRow = 5;
            void DrawSection(string title, IEnumerable<dynamic> items)
            {
                if (!items.Any()) return;
                ws.Cell(currentRow, 1).Value = title; ws.Range(currentRow, 1, currentRow, 7).Merge();
                ws.Cell(currentRow, 1).Style.Font.Bold = true; ws.Cell(currentRow, 1).Style.Fill.BackgroundColor = XLColor.FromHtml("#F2F2F2");
                ws.Cell(currentRow, 1).Style.Alignment.Indent = 1; ws.Row(currentRow).Height = 28; currentRow++;

                int stt = 1;
                foreach (var item in items)
                {
                    ws.Row(currentRow).Height = 25;
                    ws.Cell(currentRow, 1).Value = stt++; ws.Cell(currentRow, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    ws.Cell(currentRow, 2).Value = item.AssetName; ws.Cell(currentRow, 2).Style.Alignment.Indent = 1;
                    ws.Cell(currentRow, 3).Value = item.IsMayMoc ? "Cái" : "Chiếc"; ws.Cell(currentRow, 3).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    ws.Cell(currentRow, 4).Value = item.Price; ws.Cell(currentRow, 4).Style.NumberFormat.Format = "#,##0";

                    // Nơi nhận mặc định
                    ws.Cell(currentRow, 5).Value = "Phòng CNTT"; ws.Cell(currentRow, 5).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                    ws.Cell(currentRow, 6).Value = item.TotalQty; ws.Cell(currentRow, 6).Style.Font.Bold = true; ws.Cell(currentRow, 6).Style.Font.FontColor = XLColor.FromHtml("#0070C0");
                    ws.Cell(currentRow, 6).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                    ws.Cell(currentRow, 7).Value = item.TotalAmount; ws.Cell(currentRow, 7).Style.Font.Bold = true; ws.Cell(currentRow, 7).Style.NumberFormat.Format = "#,##0";
                    ws.Cell(currentRow, 7).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                    currentRow++;
                }
            }

            DrawSection("I. MÁY MÓC (CCDC)", groupedData.Where(x => x.IsMayMoc).OrderBy(x => x.AssetName));
            DrawSection("II. LINH KIỆN VÀ VẬT TƯ THAY THẾ", groupedData.Where(x => !x.IsMayMoc).OrderBy(x => x.AssetName));

            ws.Row(currentRow).Height = 30;
            ws.Cell(currentRow, 1).Value = "TỔNG CỘNG NHẬP:"; ws.Range(currentRow, 1, currentRow, 5).Merge();
            ws.Cell(currentRow, 1).Style.Font.Bold = true; ws.Cell(currentRow, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
            ws.Cell(currentRow, 1).Style.Fill.BackgroundColor = XLColor.FromHtml("#E7E6E6");

            for (int c = 6; c <= 7; c++)
            {
                string colLetter = ws.Column(c).ColumnLetter(); ws.Cell(currentRow, c).FormulaA1 = $"SUM({colLetter}5:{colLetter}{currentRow - 1})";
                ws.Cell(currentRow, c).Style.Font.Bold = true; ws.Cell(currentRow, c).Style.Fill.BackgroundColor = XLColor.FromHtml("#E7E6E6"); ws.Cell(currentRow, c).Style.Font.FontColor = XLColor.FromHtml("#C00000");
                if (c == 7) ws.Cell(currentRow, c).Style.NumberFormat.Format = "#,##0";
                else ws.Cell(currentRow, c).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            }

            var tableRange = ws.Range(4, 1, currentRow, 7);
            tableRange.Style.Border.OutsideBorder = XLBorderStyleValues.Medium; tableRange.Style.Border.OutsideBorderColor = XLColor.FromHtml("#203764");
            tableRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin; tableRange.Style.Border.InsideBorderColor = XLColor.FromHtml("#BFBFBF");

            ws.Column(1).Width = 6; ws.Column(2).Width = 45; ws.Column(3).Width = 8; ws.Column(4).Width = 15;
            ws.Column(5).Width = 15; ws.Column(6).Width = 15; ws.Column(7).Width = 18;
        }

        // =========================================================================
        // 🛠 HÀM VẼ SHEET XUẤT KHO (Ma trận có chia các Khoa)
        // =========================================================================
        private void DrawSheetXuatKho(IXLWorksheet ws, List<WarehouseHistoryResponse> data, DateTime? from, DateTime? to)
        {
            ws.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;

            ws.Cell(1, 1).Value = "BẢNG TỔNG HỢP XUẤT VẬT TƯ, LINH KIỆN CÔNG NGHỆ THÔNG TIN";
            ws.Cell(1, 1).Style.Font.Bold = true; ws.Cell(1, 1).Style.Font.FontSize = 15;
            ws.Cell(1, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Row(1).Height = 30;

            ws.Cell(2, 1).Value = $"Từ ngày: {from?.ToString("dd/MM/yyyy")} - Đến ngày: {to?.ToString("dd/MM/yyyy")}";
            ws.Cell(2, 1).Style.Font.Italic = true; ws.Cell(2, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            ws.Row(2).Height = 25;

            var activeDepts = data.Select(x => x.DepartmentName).Distinct().OrderBy(x => x).ToList();
            int totalColumns = 5 + activeDepts.Count + 1; // STT, Tên, ĐVT, Giá, TổngXuất + Các Khoa + Tiền

            ws.Range(1, 1, 1, totalColumns).Merge(); ws.Range(2, 1, 2, totalColumns).Merge();

            int col = 1;
            ws.Cell(4, col++).Value = "STT";
            ws.Cell(4, col++).Value = "Tên Thiết bị / Linh kiện";
            ws.Cell(4, col++).Value = "ĐVT";
            ws.Cell(4, col++).Value = "Đơn giá";

            int colTongXuat = col++; ws.Cell(4, colTongXuat).Value = "Tổng SL Xuất";

            int deptStartCol = col;
            foreach (var dept in activeDepts) ws.Cell(4, col++).Value = dept;

            int totalAmountCol = col; ws.Cell(4, totalAmountCol).Value = "Thành tiền";

            var headerRange = ws.Range(4, 1, 4, totalAmountCol);
            headerRange.Style.Font.Bold = true; headerRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#4F81BD");
            headerRange.Style.Font.FontColor = XLColor.White; headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            headerRange.Style.Alignment.WrapText = true; ws.Row(4).Height = 35;

            var groupedData = data.GroupBy(x => new { x.AssetName, x.AssetTypeName })
                .Select(g => new {
                    AssetName = g.Key.AssetName,
                    IsMayMoc = new[] { "Máy tính", "Máy in", "Máy Scan", "Các loại khác" }.Contains(g.Key.AssetTypeName),
                    Price = g.Max(i => i.Price),
                    TotalXuat = g.Sum(i => i.Quantity),
                    Depts = g.GroupBy(d => d.DepartmentName).ToDictionary(d => d.Key, d => d.Sum(i => i.Quantity)),
                    TotalAmount = g.Sum(i => i.Quantity * i.Price)
                }).ToList();

            int currentRow = 5;
            void DrawSection(string title, IEnumerable<dynamic> items)
            {
                if (!items.Any()) return;
                ws.Cell(currentRow, 1).Value = title; ws.Range(currentRow, 1, currentRow, totalAmountCol).Merge();
                ws.Cell(currentRow, 1).Style.Font.Bold = true; ws.Cell(currentRow, 1).Style.Fill.BackgroundColor = XLColor.FromHtml("#F2F2F2");
                ws.Cell(currentRow, 1).Style.Alignment.Indent = 1; ws.Row(currentRow).Height = 28; currentRow++;

                int stt = 1;
                foreach (var item in items)
                {
                    ws.Row(currentRow).Height = 25;
                    ws.Cell(currentRow, 1).Value = stt++; ws.Cell(currentRow, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    ws.Cell(currentRow, 2).Value = item.AssetName; ws.Cell(currentRow, 2).Style.Alignment.Indent = 1;
                    ws.Cell(currentRow, 3).Value = item.IsMayMoc ? "Cái" : "Chiếc"; ws.Cell(currentRow, 3).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    ws.Cell(currentRow, 4).Value = item.Price; ws.Cell(currentRow, 4).Style.NumberFormat.Format = "#,##0"; ws.Cell(currentRow, 4).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;

                    ws.Cell(currentRow, colTongXuat).Value = item.TotalXuat; ws.Cell(currentRow, colTongXuat).Style.Font.Bold = true;
                    ws.Cell(currentRow, colTongXuat).Style.Font.FontColor = XLColor.FromHtml("#00B050"); ws.Cell(currentRow, colTongXuat).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;

                    int c = deptStartCol;
                    foreach (var dept in activeDepts)
                    {
                        if (item.Depts.ContainsKey(dept)) ws.Cell(currentRow, c).Value = item.Depts[dept];
                        ws.Cell(currentRow, c).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center; c++;
                    }

                    ws.Cell(currentRow, totalAmountCol).Value = item.TotalAmount; ws.Cell(currentRow, totalAmountCol).Style.Font.Bold = true;
                    ws.Cell(currentRow, totalAmountCol).Style.NumberFormat.Format = "#,##0"; ws.Cell(currentRow, totalAmountCol).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
                    currentRow++;
                }
            }

            DrawSection("I. MÁY MÓC (CCDC)", groupedData.Where(x => x.IsMayMoc).OrderBy(x => x.AssetName));
            DrawSection("II. LINH KIỆN VÀ VẬT TƯ THAY THẾ", groupedData.Where(x => !x.IsMayMoc).OrderBy(x => x.AssetName));

            ws.Row(currentRow).Height = 30;
            ws.Cell(currentRow, 1).Value = "TỔNG CỘNG XUẤT:"; ws.Range(currentRow, 1, currentRow, 4).Merge();
            ws.Cell(currentRow, 1).Style.Font.Bold = true; ws.Cell(currentRow, 1).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right;
            ws.Cell(currentRow, 1).Style.Fill.BackgroundColor = XLColor.FromHtml("#E7E6E6");

            for (int c = colTongXuat; c <= totalAmountCol; c++)
            {
                string colLetter = ws.Column(c).ColumnLetter(); ws.Cell(currentRow, c).FormulaA1 = $"SUM({colLetter}5:{colLetter}{currentRow - 1})";
                ws.Cell(currentRow, c).Style.Font.Bold = true; ws.Cell(currentRow, c).Style.Fill.BackgroundColor = XLColor.FromHtml("#E7E6E6"); ws.Cell(currentRow, c).Style.Font.FontColor = XLColor.FromHtml("#C00000");
                if (c == totalAmountCol) { ws.Cell(currentRow, c).Style.NumberFormat.Format = "#,##0"; ws.Cell(currentRow, c).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Right; }
                else ws.Cell(currentRow, c).Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            }

            var tableRange = ws.Range(4, 1, currentRow, totalAmountCol);
            tableRange.Style.Border.OutsideBorder = XLBorderStyleValues.Medium; tableRange.Style.Border.OutsideBorderColor = XLColor.FromHtml("#203764");
            tableRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin; tableRange.Style.Border.InsideBorderColor = XLColor.FromHtml("#BFBFBF");

            ws.Column(1).Width = 6; ws.Column(2).Width = 45; ws.Column(3).Width = 8; ws.Column(4).Width = 15; ws.Column(5).Width = 15;
            for (int c = deptStartCol; c < totalAmountCol; c++) ws.Column(c).Width = 11;
            ws.Column(totalAmountCol).Width = 18;
        }

        // =========================================================================
        // 3. HÀM GỘP NHẬP + XUẤT + SỬA CHỮA
        // =========================================================================
        private async Task<List<WarehouseHistoryResponse>> GetMergedDataSafe(DateTime? from, DateTime? to, string? types, int? deptId, string? transType = null)
        {
            var fromDate = from ?? DateTime.MinValue;
            var toDate = to ?? DateTime.MaxValue;
            int filterDeptId = deptId ?? 0;

            List<string> selectedTypes = new List<string>();
            if (!string.IsNullOrEmpty(types))
            {
                var decodedTypes = System.Net.WebUtility.UrlDecode(types);
                selectedTypes = decodedTypes.Split(',').Select(t => t.Trim()).Where(t => !string.IsNullOrEmpty(t)).ToList();
            }

            var result = new List<WarehouseHistoryResponse>();

            bool getNhap = string.IsNullOrEmpty(transType) || transType == "Nhập";
            bool getXuat = string.IsNullOrEmpty(transType) || transType == "Xuất";

            // A. LẤY DỮ LIỆU NHẬP KHO
            if (getNhap && filterDeptId == 0)
            {
                var queryN = _context.Assets.Include(a => a.AssetType)
                    .Where(a => a.ImportDate.Date >= fromDate.Date && a.ImportDate.Date <= toDate.Date);

                var imports = await queryN.ToListAsync();
                if (selectedTypes.Any())
                    imports = imports.Where(a => a.AssetType != null && selectedTypes.Contains(a.AssetType.TypeName)).ToList();

                foreach (var a in imports)
                {
                    result.Add(new WarehouseHistoryResponse
                    {
                        TransactionID = a.AssetID,
                        Date = a.ImportDate,
                        ReferenceNo = "NK-" + a.AssetID,
                        Type = "Nhập kho",
                        AssetName = a.AssetName,
                        AssetTypeName = a.AssetType?.TypeName ?? "",
                        DepartmentName = "Phòng CNTT",
                        UserName = "Admin",
                        Quantity = a.Quantity,
                        Price = a.Price,
                        TotalAmount = a.Quantity * a.Price,
                        Note = a.Config ?? ""
                    });
                }
            }

            // B. LẤY DỮ LIỆU XUẤT KHO
            if (getXuat)
            {
                var queryA = _context.AssetAllocations.Include(a => a.Asset).ThenInclude(t => t.AssetType).Include(a => a.Department).Include(a => a.User)
                    .Where(a => a.AllocatedDate.Date >= fromDate.Date && a.AllocatedDate.Date <= toDate.Date);

                if (filterDeptId > 0) queryA = queryA.Where(a => a.DepartmentID == filterDeptId);
                var allocations = await queryA.ToListAsync();

                if (selectedTypes.Any()) allocations = allocations.Where(a => a.Asset != null && a.Asset.AssetType != null && selectedTypes.Contains(a.Asset.AssetType.TypeName)).ToList();

                foreach (var a in allocations)
                {
                    decimal price = a.Asset?.Price ?? 0;
                    result.Add(new WarehouseHistoryResponse
                    {
                        TransactionID = a.AllocationID,
                        Date = a.AllocatedDate,
                        ReferenceNo = "CP-" + a.AllocationID,
                        Type = "Cấp phát",
                        AssetName = a.Asset != null ? a.Asset.AssetName : "N/A",
                        AssetTypeName = (a.Asset != null && a.Asset.AssetType != null) ? a.Asset.AssetType.TypeName : "",
                        DepartmentName = a.Department != null ? a.Department.DeptName : "N/A",
                        UserName = a.User != null ? a.User.Username : "N/A",
                        Quantity = a.Quantity,
                        Price = price,
                        TotalAmount = a.Quantity * price,
                        Note = a.Note ?? ""
                    });
                }

                var queryR = _context.RepairTicketDetails.Include(r => r.RepairTicket).ThenInclude(t => t.Department).Include(r => r.Asset).ThenInclude(t => t.AssetType)
                    .Where(r => r.RepairTicket.RepairDate.HasValue && r.RepairTicket.RepairDate.Value.Date >= fromDate.Date && r.RepairTicket.RepairDate.Value.Date <= toDate.Date);

                if (filterDeptId > 0) queryR = queryR.Where(r => r.RepairTicket.DepartmentID == filterDeptId);
                var repairs = await queryR.ToListAsync();

                if (selectedTypes.Any()) repairs = repairs.Where(r => r.Asset != null && r.Asset.AssetType != null && selectedTypes.Contains(r.Asset.AssetType.TypeName)).ToList();

                foreach (var r in repairs)
                {
                    decimal price = r.Asset?.Price ?? 0;
                    result.Add(new WarehouseHistoryResponse
                    {
                        TransactionID = r.DetailID,
                        Date = r.RepairTicket.RepairDate.Value,
                        ReferenceNo = "SC-" + r.TicketID,
                        Type = "Sửa chữa",
                        AssetName = r.Asset != null ? r.Asset.AssetName : "Linh kiện",
                        AssetTypeName = (r.Asset != null && r.Asset.AssetType != null) ? r.Asset.AssetType.TypeName : "Linh kiện",
                        DepartmentName = (r.RepairTicket != null && r.RepairTicket.Department != null) ? r.RepairTicket.Department.DeptName : "Khách lẻ",
                        UserName = "KTV",
                        Quantity = r.Quantity,
                        Price = price,
                        TotalAmount = r.Quantity * price,
                        Note = r.RepairTicket?.Asset != null ? $"Thay cho: {r.RepairTicket.Asset.AssetName}" : "Thay thế"
                    });
                }
            }

            return result.OrderByDescending(x => x.Date).ToList();
        }

        // =========================================================================
        // 4. XUẤT WORD
        // =========================================================================
        [HttpGet("export-word/{ticketId}")]
        public async Task<IActionResult> ExportWord(int ticketId)
        {
            try
            {
                var ticket = await _unitOfWork.GetRepository<RepairTicket>().GetByIdAsync(ticketId);
                if (ticket == null) return NotFound("Không tìm thấy phiếu.");
                string templatePath = Path.Combine(_env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot"), "templates", "Bien_Ban_Hien_Trang.docx");
                if (!System.IO.File.Exists(templatePath)) return NotFound("Không tìm thấy file mẫu.");

                using (var stream = new MemoryStream())
                {
                    using (var doc = DocX.Load(templatePath))
                    {
                        doc.ReplaceText("{{Ngay}}", ticket.CreatedDate.Day.ToString("00"));
                        doc.ReplaceText("{{Thang}}", ticket.CreatedDate.Month.ToString("00"));
                        doc.ReplaceText("{{Nam}}", ticket.CreatedDate.Year.ToString());
                        doc.SaveAs(stream);
                    }
                    return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.wordprocessingml.document", $"BienBan_SC_{ticketId}.docx");
                }
            }
            catch (Exception ex) { return StatusCode(500, $"Lỗi xử lý Word: {ex.Message}"); }
        }
    }
}