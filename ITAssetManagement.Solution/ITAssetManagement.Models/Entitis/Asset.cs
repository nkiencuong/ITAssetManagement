using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITAssetManagement.Models.Entitis
{
    public class Asset
    {
        [Key]
        public int AssetID { get; set; }

        [Required]
        [StringLength(200)]
        public string AssetName { get; set; } = string.Empty;

        [StringLength(100)]
        public string? Serial { get; set; }  // Unique in DB config

        [StringLength(100)]
        public string? Model { get; set; }

        public int AssetTypeID { get; set; }

        public int Status { get; set; } = 0;  // 0: Kho, 1: Cấp phát, 2: Sửa, 3: Hỏng, 4: Mất

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public DateTime? PurchaseDate { get; set; }

        public decimal Price { get; set; }

        public string Currency { get; set; } = "VND";

        public int? SupplierID { get; set; }

        [StringLength(200)]
        public string? Location { get; set; }

        [StringLength(500)]
        public string? Config { get; set; }

        [StringLength(100)]
        public string? QRCode { get; set; }

        public DateTime? WarrantyExpr { get; set; } // Thêm dòng này (Hạn bảo hành)

        // Navigation
        public virtual AssetType AssetType { get; set; } = null!;
        public virtual Supplier? Supplier { get; set; }
        public virtual ICollection<WarehouseTransaction> WarehouseTransactions { get; set; } = new List<WarehouseTransaction>();
        public virtual ICollection<AssetAllocation> AssetAllocations { get; set; } = new List<AssetAllocation>();
        public virtual ICollection<RepairTicket> RepairTickets { get; set; } = new List<RepairTicket>();
        public ICollection<InventoryCheck> InventoryChecks { get; set; } = new List<InventoryCheck>();
    }
}

