// File: ITAssetManagement.Client/Models/Asset.cs
namespace ITAssetManagement.Client.Models
{
    public class Asset
    {
        public int AssetID { get; set; }
        public string AssetName { get; set; } = string.Empty;
        public string Serial { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public string? Config { get; set; }
        public decimal Price { get; set; }
        public string? Currency { get; set; }
        public int Status { get; set; } // 0: Trong kho, 1: Đang dùng, 2: Hỏng
        public DateTime CreatedDate { get; set; }
        public string? Location { get; set; }

        // Nếu API Backend có trả về kèm tên Loại và Nhà cung cấp (Include)
        // thì mình khai báo thêm để hứng, còn không thì nó sẽ null (không lỗi gì cả)
        public AssetType? AssetType { get; set; }
        public Supplier? Supplier { get; set; }
    }

    public class AssetType
    {
        public int AssetTypeID { get; set; }
        public string TypeName { get; set; } = string.Empty;
    }

    public class Supplier
    {
        public int SupplierID { get; set; }
        public string SupplierName { get; set; } = string.Empty;
    }
}