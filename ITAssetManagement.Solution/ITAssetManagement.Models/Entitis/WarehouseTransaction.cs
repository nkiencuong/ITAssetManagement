using System;
using System.ComponentModel.DataAnnotations;

namespace ITAssetManagement.Models.Entitis
{
    public class WarehouseTransaction
    {
        [Key]
        public int TransactionID { get; set; }

        [Required]
        [StringLength(10)]
        public string Type { get; set; } = string.Empty;  // 'IN', 'OUT', 'ADJUST', 'REPAIR'

        public int AssetID { get; set; }

        public int Quantity { get; set; } = 1;

        public DateTime Date { get; set; } = DateTime.Now;

        public int? DepartmentID { get; set; }

        // --- ĐÃ SỬA: Thêm dấu ? để cho phép Null ---
        public int? UserID { get; set; }

        [StringLength(500)]
        public string? Note { get; set; }

        [StringLength(100)]
        public string? ReferenceNo { get; set; }

        // Navigation
        public virtual Asset Asset { get; set; } = null!;
        public virtual Department? Department { get; set; }

        // --- ĐÃ SỬA: Thêm dấu ? để Navigation cũng cho phép Null ---
        public virtual User? User { get; set; }
    }
}