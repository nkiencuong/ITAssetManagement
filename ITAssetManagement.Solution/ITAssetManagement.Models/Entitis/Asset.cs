using ITAssetManagement.Models.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ITAssetManagement.Models.Entitis
{
    [Table("Asset")]
    public class Asset
    {
        [Key]
        public int AssetID { get; set; }

        [Required]
        [StringLength(200)]
        public string AssetName { get; set; } = string.Empty;

        // 👇 TRẢ VỀ int VÀ decimal (KHÔNG CÓ ?) ĐỂ CÁC FILE KHÁC KHÔNG BỊ LỖI TÍNH TOÁN
        // (Vì mình đã chạy SQL update dữ liệu về 0 ở Bước 1 rồi nên không sợ Crash nữa)
        public int Quantity { get; set; } = 0;

        [Column(TypeName = "decimal(18, 2)")]
        public decimal Price { get; set; } = 0;

        public int Status { get; set; } = 0;

        [StringLength(50)]
        public string Unit { get; set; } = "Cái";

        [StringLength(100)]
        public string? Model { get; set; }

        // Cột này mới thêm, cứ để ? cho an toàn, không ảnh hưởng logic cũ
        public string? ModelSeries { get; set; }

        // --- KHÓA NGOẠI ---
        public int AssetTypeID { get; set; }
        public int? SupplierID { get; set; }
        [NotMapped]
        public string? SupplierName { get; set; }
        public int? DepartmentID { get; set; }

        // --- NGÀY THÁNG ---
        // 👇 MẤY CÁI NÀY BẮT BUỘC PHẢI CÓ ? VÌ DATABASE CỦA BÁC ĐANG NULL (Xem ảnh image_99fcd4)
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime ImportDate { get; set; } = DateTime.Now;
        public DateTime? PurchaseDate { get; set; }
        public DateTime? WarrantyExpr { get; set; }

        // --- CÁC TRƯỜNG KHÁC ---
        public string Currency { get; set; } = "VND";
        [StringLength(200)]
        public string? Location { get; set; }
        [StringLength(500)]
        public string? Config { get; set; }
        [StringLength(100)]
        public string? QRCode { get; set; }

        // --- LIÊN KẾT ---
        [ForeignKey("AssetTypeID")]
        public virtual AssetType? AssetType { get; set; }
        [ForeignKey("SupplierID")]
        public virtual Supplier? Supplier { get; set; }
        [ForeignKey("DepartmentID")]
        public virtual Department? Department { get; set; }

        // --- DANH SÁCH CON ---
        public virtual ICollection<WarehouseTransaction> WarehouseTransactions { get; set; } = new List<WarehouseTransaction>();
        public virtual ICollection<AssetAllocation> AssetAllocations { get; set; } = new List<AssetAllocation>();
        public virtual ICollection<RepairTicket> RepairTickets { get; set; } = new List<RepairTicket>();
        public virtual ICollection<InventoryCheck> InventoryChecks { get; set; } = new List<InventoryCheck>();
    }
}