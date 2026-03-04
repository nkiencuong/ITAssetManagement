using System;

namespace ITAssetManagement.Client.Models
{
    public class Asset
    {
        public int AssetID { get; set; }
        public string AssetName { get; set; } = string.Empty;

        public int Quantity { get; set; } = 0;
        public string Unit { get; set; } = "Cái";

        // 👇 QUAN TRỌNG: Server gửi tên loại vào biến này, không phải vào object AssetType
        public string? AssetTypeName { get; set; }
        public string? SupplierName { get; set; }

        public string Model { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public string? Currency { get; set; }
        public int Status { get; set; }
        public DateTime CreatedDate { get; set; }
        public string? Location { get; set; }
        public string? Config { get; set; }

        public int AssetTypeID { get; set; }
        public int? SupplierID { get; set; }
        public string ModelSeries { get; set; } = "";

        // Object này thường bị NULL do API trả về dạng phẳng (flat), nên không dùng để lọc được
        public AssetType? AssetType { get; set; }
    }
}