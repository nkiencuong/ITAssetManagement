using System;
using System.ComponentModel.DataAnnotations;

namespace ITAssetManagement.Request.Assets
{
    public class CreateAssetRequest
    {
        [Required(ErrorMessage = "Tên tài sản không được để trống")]
        public string AssetName { get; set; } = string.Empty;

        [Range(1, int.MaxValue, ErrorMessage = "Số lượng phải lớn hơn 0")]
        public int Quantity { get; set; } = 1;

        public string Unit { get; set; } = "Cái";

        public string? Model { get; set; }
        public string? ModelSeries { get; set; }
        // --- KHU VỰC ĐÃ BỔ SUNG THÊM ID (QUAN TRỌNG) ---

        // 1. Loại tài sản: Vừa hứng ID (Dropdown), vừa hứng Tên (Nhập tay)
        public int AssetTypeID { get; set; }
        public string AssetTypeName { get; set; } = "Thiết bị chung";

        // 2. Nhà cung cấp: Vừa hứng ID (Dropdown), vừa hứng Tên (Nhập tay)
        public int SupplierID { get; set; }
        public string SupplierName { get; set; } = "Kho Tổng";

        // 3. Phòng ban (Có thể null)
        public int? DepartmentID { get; set; }

        // ----------------------------------------------------

        public int UserID { get; set; }

        public decimal Price { get; set; }
        public string? Config { get; set; }
        public string? Location { get; set; }
        public string? ImportNote { get; set; }
        public DateTime? WarrantyExpr { get; set; }
        public DateTime ImportDate { get; set; } = DateTime.Now; // Mặc định là hôm nay
    }
}