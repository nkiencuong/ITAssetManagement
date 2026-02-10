using System;

namespace ITAssetManagement.Response.Reports // Hoặc namespace: ITAssetManagement.Request.Reports nếu bạn để chung chỗ
{
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

        // Cột Thành tiền (Code sẽ tự tính và đổ vào đây)
        public decimal TotalAmount { get; set; }
    }
}