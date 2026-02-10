using ClosedXML.Excel;
using ITAssetManagement.Models.Entitis;
using ITAssetManagement.Repo.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ITAssetManagement.Server.Controllers
{
    [Route("api/report")]
    [ApiController]
    public class ApiReportController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public ApiReportController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // 1. API CŨ: XUẤT FILE EXCEL
        [HttpPost("export-excel")]
        public async Task<IActionResult> ExportMaterialReport([FromBody] ReportRequest request)
        {
            try
            {
                var data = await GetQueryData(request); // 🔥 Gom code query vào hàm chung cho gọn

                if (data.Count == 0) return BadRequest("Không có dữ liệu trong khoảng thời gian này.");

                using (var workbook = new XLWorkbook())
                {
                    var worksheet = workbook.Worksheets.Add("BaoCaoXuatKho");

                    // Header
                    worksheet.Cell(1, 1).Value = "STT";
                    worksheet.Cell(1, 2).Value = "Ngày xuất";
                    worksheet.Cell(1, 3).Value = "Mã VT (ID)";
                    worksheet.Cell(1, 4).Value = "Tên Vật Tư";
                    worksheet.Cell(1, 5).Value = "Model";
                    worksheet.Cell(1, 6).Value = "Phòng Ban Nhận";
                    worksheet.Cell(1, 7).Value = "Số lượng";
                    worksheet.Cell(1, 8).Value = "Người thực hiện";
                    worksheet.Cell(1, 9).Value = "Đơn giá";
                    worksheet.Cell(1, 10).Value = "Thành tiền"; // Thêm cột thành tiền vào Excel luôn cho đẹp

                    var headerRange = worksheet.Range("A1:J1");
                    headerRange.Style.Font.Bold = true;
                    headerRange.Style.Fill.BackgroundColor = XLColor.LightBlue;
                    headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                    headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;

                    // Đổ dữ liệu
                    int row = 2;
                    for (int i = 0; i < data.Count; i++)
                    {
                        var item = data[i];
                        worksheet.Cell(row, 1).Value = i + 1;
                        worksheet.Cell(row, 2).Value = item.Date.ToString("dd/MM/yyyy");
                        worksheet.Cell(row, 3).Value = item.Asset?.AssetID;
                        worksheet.Cell(row, 4).Value = item.Asset?.AssetName;
                        worksheet.Cell(row, 5).Value = item.Asset?.Model;

                        string phongBanNhan = GetDepartmentName(item); // Hàm xử lý tên phòng
                        worksheet.Cell(row, 6).Value = phongBanNhan;

                        worksheet.Cell(row, 7).Value = item.Quantity;
                        worksheet.Cell(row, 8).Value = item.User?.FullName;

                        decimal donGia = item.Asset?.Price ?? 0;
                        worksheet.Cell(row, 9).Value = donGia;
                        worksheet.Cell(row, 9).Style.NumberFormat.Format = "#,##0";

                        // Thành tiền
                        worksheet.Cell(row, 10).Value = donGia * item.Quantity;
                        worksheet.Cell(row, 10).Style.NumberFormat.Format = "#,##0";

                        row++;
                    }

                    worksheet.Columns().AdjustToContents();

                    using (var stream = new MemoryStream())
                    {
                        workbook.SaveAs(stream);
                        var content = stream.ToArray();
                        string fileName = $"BaoCao_{DateTime.Now:ddMMyyyy}.xlsx";
                        return File(content, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
                    }
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi server: {ex.Message}");
            }
        }

        // 2. API MỚI: XEM TRƯỚC
        [HttpPost("get-data")]
        public async Task<IActionResult> GetReportData([FromBody] ReportRequest request)
        {
            try
            {
                var rawData = await GetQueryData(request); // 🔥 Dùng chung hàm Query

                var resultList = new List<ReportResponse>();
                int stt = 1;

                foreach (var item in rawData)
                {
                    string phongBanNhan = GetDepartmentName(item);
                    decimal price = item.Asset?.Price ?? 0;

                    resultList.Add(new ReportResponse
                    {
                        STT = stt++,
                        Date = item.Date,
                        AssetId = item.AssetID,
                        AssetName = item.Asset?.AssetName ?? "",
                        Model = item.Asset?.Model ?? "",
                        DepartmentName = phongBanNhan,
                        Quantity = item.Quantity,
                        UserFullName = item.User?.FullName ?? "",
                        Price = price,
                        TotalAmount = price * item.Quantity
                    });
                }

                return Ok(resultList);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Lỗi: " + ex.Message);
            }
        }

        // --- 👇 CÁC HÀM HỖ TRỢ (PRIVATE) ĐỂ CODE GỌN HƠN 👇 ---

        // Hàm 1: Lọc dữ liệu chung cho cả Excel và Xem trước
        private async Task<List<WarehouseTransaction>> GetQueryData(ReportRequest request)
        {
            var query = _unitOfWork.GetRepository<WarehouseTransaction>().GetAll()
                                   .Include(t => t.Asset).ThenInclude(a => a.Department)
                                   .Include(t => t.Asset).ThenInclude(a => a.AssetType)
                                   .Include(t => t.User)
                                   .Include(t => t.Department)
                                   .Where(t => t.Type == "OUT" || t.Type == "OUT_REPAIR" || t.Type == "REPAIR");

            query = query.Where(t => t.Date.Date >= request.FromDate.Date
                                 && t.Date.Date <= request.ToDate.Date);

            if (request.AssetId.HasValue)
            {
                query = query.Where(t => t.AssetID == request.AssetId.Value);
            }

            // 🔥 LOGIC LỌC THEO LOẠI TÀI SẢN (MỚI THÊM)
            if (!string.IsNullOrEmpty(request.AssetType))
            {
                if (request.AssetType == "PART")
                {
                    // Lọc Linh kiện & Vật tư
                    query = query.Where(t => t.Asset.AssetType.TypeName.Contains("Linh kiện")
                                          || t.Asset.AssetType.TypeName.Contains("Vật tư"));
                }
                else
                {
                    // Lọc theo tên loại (Máy in, Máy tính...)
                    query = query.Where(t => t.Asset.AssetType.TypeName.Contains(request.AssetType));
                }
            }

            return await query.OrderBy(t => t.Date).ToListAsync();
        }

        // Hàm 2: Xử lý logic tên phòng ban (đỡ phải copy paste nhiều lần)
        private string GetDepartmentName(WarehouseTransaction item)
        {
            if (item.Department != null)
                return item.Department.DeptName;

            if (item.Type == "OUT_REPAIR" || item.Type == "REPAIR")
                return item.Asset?.Department?.DeptName ?? "Sửa chữa (Chưa rõ phòng)";

            if (item.Asset?.AssetType?.TypeName.Contains("Linh kiện") == true || item.Asset?.AssetType?.TypeName.Contains("Vật tư") == true)
                return "Xuất dùng / Thay thế";

            return "Khác / Xuất ngoài";
        }
    }

    public class ReportRequest
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public int? AssetId { get; set; }

        // 👇 Thêm trường này để nhận dữ liệu từ Dropdown
        public string? AssetType { get; set; }
    }

    public class ReportResponse
    {
        public int STT { get; set; }
        public DateTime Date { get; set; }
        public int AssetId { get; set; }
        public string AssetName { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public string DepartmentName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public string UserFullName { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public decimal TotalAmount { get; set; }
    }
}