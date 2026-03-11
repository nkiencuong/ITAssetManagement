using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITAssetManagement.Response.Assets
{
    public class AssetResponse
    {
        public int AssetID { get; set; }
        public string AssetName { get; set; }
        public int Quantity { get; set; }
        public string Unit { get; set; } = string.Empty;
        public string Model { get; set; }

        // Thông tin loại và nhà cung cấp (để hiển thị lên lưới)
        public int AssetTypeID { get; set; }
        public string AssetTypeName { get; set; } // Tên loại (Laptop)

        public int? SupplierID { get; set; }
        public string SupplierName { get; set; } // Tên NCC (FPT)

        public decimal Price { get; set; }
        public int Status { get; set; } // 0: Kho, 1: Dùng...
        public string Location { get; set; }
        public string Config { get; set; }
        public int AllocatedQuantity { get; set; } // Số lượng đã xuất (cộng dồn)
        public DateTime ImportDate { get; set; }
        public DateTime CreatedDate { get; set; }
        public DateTime? WarrantyExpr { get; set; }
    }
}