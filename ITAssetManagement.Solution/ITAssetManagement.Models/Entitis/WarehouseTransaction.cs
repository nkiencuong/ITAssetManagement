using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public int UserID { get; set; }   // string nếu Identity

        [StringLength(500)]
        public string? Note { get; set; }

        [StringLength(100)]
        public string? ReferenceNo { get; set; }

        // Navigation
        public virtual Asset Asset { get; set; } = null!;
        public virtual Department? Department { get; set; }
        public virtual User User { get; set; } = null!;
    }
}
