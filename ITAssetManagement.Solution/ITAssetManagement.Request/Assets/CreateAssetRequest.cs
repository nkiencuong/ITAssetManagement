using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITAssetManagement.Request.Assets
{
    public class CreateAssetRequest
    {
        [Required(ErrorMessage = "Tên tài sản không được để trống")]
        public string AssetName { get; set; }

        // Serial có thể null nếu là linh kiện số lượng nhiều (Chuột, Phím...)
        public string? Serial { get; set; }

        public string? Model { get; set; }

        [Required(ErrorMessage = "Phải chọn loại tài sản")]
        public int AssetTypeID { get; set; }

        public int? SupplierID { get; set; } // Có thể null nếu là hàng được tặng/điều chuyển

        public decimal Price { get; set; }

        public string? Config { get; set; } // Cấu hình (Core i5, Ram 8GB...)

        public string? Location { get; set; } // Vị trí lưu kho ban đầu

        // --- Các trường phục vụ "Phiếu kiểm nhập" (WarehouseTransaction) ---

        // Ghi chú nhập kho (VD: Nhập mới theo dự án A, Nhập linh kiện lẻ...)
        public string? ImportNote { get; set; }

        public DateTime? WarrantyExpr { get; set; }
    }
}