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

        public int Quantity { get; set; } = 0;

        [StringLength(50)]
        public string Unit { get; set; } = "Cái";

        [StringLength(100)]
        public string? Model { get; set; }

        // --- KHÓA NGOẠI (SỬA LẠI int? ĐỂ AN TOÀN TUYỆT ĐỐI) ---

        public int AssetTypeID { get; set; } // Loại thì nên bắt buộc

        public int? SupplierID { get; set; } // Cho phép Null (để tránh lỗi dữ liệu cũ)

        public int? DepartmentID { get; set; } // Cho phép Null

        public int Status { get; set; } = 0;

        // --- NGÀY THÁNG ---
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public DateTime ImportDate { get; set; } = DateTime.Now;  // Cột mới thêm
        public DateTime? PurchaseDate { get; set; }
        public DateTime? WarrantyExpr { get; set; }

        // --- TÀI CHÍNH ---
        [Column(TypeName = "decimal(18, 2)")]
        public decimal Price { get; set; }

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