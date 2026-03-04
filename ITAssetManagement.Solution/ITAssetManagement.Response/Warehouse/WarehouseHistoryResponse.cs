using System;

namespace ITAssetManagement.Response.Warehouse
{
    public class WarehouseHistoryResponse
    {
        public int TransactionID { get; set; }
        public string AssetName { get; set; } = string.Empty;
        public string AssetTypeName { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public DateTime Date { get; set; }
        public string DepartmentName { get; set; } = "N/A";
        public string UserName { get; set; } = "N/A";
        public string Note { get; set; } = string.Empty;
        public string ReferenceNo { get; set; } = string.Empty;

        // 👇👇 THÊM 2 DÒNG NÀY ĐỂ HỨNG TIỀN 👇👇
        public decimal Price { get; set; } = 0; // Đơn giá
        public decimal TotalAmount { get; set; } = 0; // Thành tiền
    }
}