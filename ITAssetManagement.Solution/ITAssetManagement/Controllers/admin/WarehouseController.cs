using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ITAssetManagement.Models; // Chứa ApplicationDbContext
using ITAssetManagement.Models.Entitis; // Chứa AssetAllocation, RepairTicketDetail
using ITAssetManagement.Service.Interfaces;
using ITAssetManagement.Response.Warehouse; // Chứa WarehouseHistoryResponse
using ClosedXML.Excel;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using System.Collections.Generic;

namespace ITAssetManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WarehouseController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IAllocationService _allocationService;

        public WarehouseController(ApplicationDbContext context, IAllocationService allocationService)
        {
            _context = context;
            _allocationService = allocationService;
        }

        // ============================================================
        // 1. API LẤY LỊCH SỬ KHO (Dùng cho trang "Lịch sử Nhập/Xuất")
        // Logic: Lấy từ bảng WarehouseTransaction để hiện cả NHẬP và XUẤT
        // ============================================================
        [HttpGet("history")]
        public async Task<IActionResult> GetWarehouseHistory([FromQuery] DateTime? from, [FromQuery] DateTime? to)
        {
            try
            {
                var query = _context.WarehouseTransactions
                    .Include(t => t.Asset).ThenInclude(a => a.AssetType)
                    .Include(t => t.Department)
                    // 🚀 1. THÊM DÒNG NÀY ĐỂ KÉO BẢNG USER LÊN
                    .Include(t => t.User)
                    .AsQueryable();

                if (from.HasValue) query = query.Where(t => t.Date.Date >= from.Value.Date);
                if (to.HasValue) query = query.Where(t => t.Date.Date <= to.Value.Date);

                var data = await query.OrderByDescending(t => t.Date)
                    .Select(t => new WarehouseHistoryResponse
                    {
                        TransactionID = t.TransactionID,
                        Date = t.Date,
                        ReferenceNo = t.ReferenceNo ?? "",
                        Type = t.Type == "IN" ? "Nhập kho" : (t.Type == "OUT" ? "Xuất kho" : t.Type),
                        AssetName = t.Asset != null ? t.Asset.AssetName : "Không xác định",
                        AssetTypeName = (t.Asset != null && t.Asset.AssetType != null) ? t.Asset.AssetType.TypeName : "",
                        DepartmentName = t.Department != null ? t.Department.DeptName : "-",
                        Quantity = t.Quantity,
                        Note = t.Note ?? "",
                        // 🚀 2. GÁN TÊN NGƯỜI DÙNG Ở ĐÂY NÀY BÁC
                        UserName = t.User != null ? t.User.FullName : "Hệ thống"
                    })
                    .ToListAsync();

                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest("Lỗi lấy lịch sử kho: " + ex.Message);
            }
        }

        // ============================================================
        // 2. API LẤY DỮ LIỆU BÁO CÁO XUẤT KHO (Cấp phát + Sửa chữa)
        // Logic: Dùng cho trang Báo cáo và Xuất Excel
        // ============================================================
        [HttpGet("report-data")]
        public async Task<IActionResult> GetReportData([FromQuery] DateTime? from, [FromQuery] DateTime? to)
        {
            try
            {
                var data = await GetMergedReportData(from, to);
                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest("Lỗi lấy dữ liệu báo cáo: " + ex.Message);
            }
        }

        // ============================================================
        // 3. API XUẤT EXCEL (.XLSX)
        // ============================================================
        [HttpGet("export-excel")]
        public async Task<IActionResult> ExportExcel([FromQuery] DateTime? from, [FromQuery] DateTime? to)
        {
            try
            {
                // Gọi hàm gộp dữ liệu
                var data = await GetMergedReportData(from, to);

                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("BaoCaoXuatKho");

                    // Header
                    var headers = new[] { "Ngày xuất", "Mã phiếu", "Loại GD", "Loại TS", "Tên Tài sản / Linh kiện", "Đơn vị nhận / Người nhận", "SL", "Ghi chú" };

                    for (int i = 0; i < headers.Length; i++)
                    {
                        var cell = worksheet.Cell(1, i + 1);
                        cell.Value = headers[i];
                        cell.Style.Font.Bold = true;
                        cell.Style.Fill.BackgroundColor = XLColor.FromHtml("#007bff");
                        cell.Style.Font.FontColor = XLColor.White;
                        cell.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    }

                    // Body
                    int row = 2;
                    foreach (var item in data)
                    {
                        worksheet.Cell(row, 1).Value = item.Date;
                        worksheet.Cell(row, 2).Value = item.ReferenceNo;
                        worksheet.Cell(row, 3).Value = item.Type;
                        worksheet.Cell(row, 4).Value = item.AssetTypeName;
                        worksheet.Cell(row, 5).Value = item.AssetName;

                        // Xử lý hiển thị người nhận
                        string receiver = string.IsNullOrEmpty(item.UserName) || item.UserName == "N/A"
                                          ? item.DepartmentName
                                          : $"{item.UserName} ({item.DepartmentName})";
                        worksheet.Cell(row, 6).Value = receiver;

                        worksheet.Cell(row, 7).Value = item.Quantity;
                        worksheet.Cell(row, 8).Value = item.Note;

                        if (item.Type == "Sửa chữa")
                        {
                            worksheet.Range(row, 1, row, 8).Style.Fill.BackgroundColor = XLColor.LightYellow;
                        }

                        row++;
                    }
                    worksheet.Columns().AdjustToContents();

                    using (var stream = new MemoryStream())
                    {
                        workbook.SaveAs(stream);
                        return File(stream.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", $"XuatKho_{DateTime.Now:ddMMyy}.xlsx");
                    }
                }
            }
            catch (Exception ex)
            {
                return BadRequest("Lỗi xuất Excel: " + ex.Message);
            }
        }

        // ============================================================
        // 4. API CẤP PHÁT CŨ (Giữ nguyên để không lỗi trang khác)
        // ============================================================
        [HttpGet("allocations")]
        public async Task<IActionResult> GetActiveAllocations()
        {
            try
            {
                var data = await _allocationService.GetAllocationHistoryAsync();
                return Ok(data);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // ============================================================
        // HÀM PRIVATE: LOGIC GỘP DỮ LIỆU (Đã sửa tên cột chính xác)
        // ============================================================
        private async Task<List<WarehouseHistoryResponse>> GetMergedReportData(DateTime? from, DateTime? to)
        {
            var fromDate = from ?? DateTime.MinValue;
            var toDate = to ?? DateTime.MaxValue;

            // --- 1. NGUỒN CẤP PHÁT (AssetAllocations) ---
            var queryAllocations = _context.AssetAllocations // ✅ Tên đúng
                .Include(a => a.Asset).ThenInclude(t => t.AssetType)
                .Include(a => a.Department)
                .Include(a => a.User) // Include User để lấy tên
                .Where(a => a.AllocatedDate.Date >= fromDate.Date && a.AllocatedDate.Date <= toDate.Date)
                .Select(a => new WarehouseHistoryResponse
                {
                    TransactionID = a.AllocationID, // ✅ Đã sửa AllocationID
                    Date = a.AllocatedDate,
                    ReferenceNo = "CP-" + a.AllocationID,
                    Type = "Cấp phát",
                    AssetName = a.Asset != null ? a.Asset.AssetName : "Không xác định",
                    AssetTypeName = (a.Asset != null && a.Asset.AssetType != null) ? a.Asset.AssetType.TypeName : "",
                    DepartmentName = a.Department != null ? a.Department.DeptName : "N/A",
                    // ✅ Fix lỗi Null User (kiểm tra null trước khi lấy Username)
                    UserName = a.User != null ? a.User.Username : "N/A",
                    Quantity = a.Quantity,
                    Note = a.Note ?? ""
                });

            // --- 2. NGUỒN LINH KIỆN SỬA CHỮA (RepairTicketDetails) ---
            var queryRepairs = _context.RepairTicketDetails // ✅ Tên đúng
                .Include(r => r.RepairTicket).ThenInclude(t => t.Department)
                .Include(r => r.Asset).ThenInclude(t => t.AssetType)
                .Where(r => r.RepairTicket.RepairDate.HasValue
                            && r.RepairTicket.RepairDate.Value.Date >= fromDate.Date
                            && r.RepairTicket.RepairDate.Value.Date <= toDate.Date)
                .Select(r => new WarehouseHistoryResponse
                {
                    TransactionID = r.DetailID, // ✅ Đã sửa DetailID
                    Date = r.RepairTicket.RepairDate.Value,
                    ReferenceNo = "SC-" + r.TicketID, // ✅ Đã sửa TicketID
                    Type = "Sửa chữa",
                    AssetName = r.Asset != null ? r.Asset.AssetName : "Linh kiện",
                    AssetTypeName = (r.Asset != null && r.Asset.AssetType != null) ? r.Asset.AssetType.TypeName : "",
                    // ✅ Fix lỗi Null Department của RepairTicket
                    DepartmentName = (r.RepairTicket != null && r.RepairTicket.Department != null) ? r.RepairTicket.Department.DeptName : "Khách lẻ",
                    UserName = "KTV Sửa chữa",
                    Quantity = r.Quantity,
                    Note = r.RepairTicket != null && r.RepairTicket.Asset != null
                           ? $"Thay thế cho máy: {r.RepairTicket.Asset.AssetName}"
                           : "Thay thế linh kiện"
                });

            // GỘP VÀ TRẢ VỀ
            return await queryAllocations
                .Union(queryRepairs)
                .OrderByDescending(x => x.Date)
                .ToListAsync();
        }
    }
}