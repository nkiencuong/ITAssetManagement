using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITAssetManagement.Models.Entitis
{
    public class InventoryCheck
    {
        [Key]
        public int CheckID { get; set; }

        public DateTime CheckDate { get; set; } = DateTime.Now;

        public int UserID { get; set; } 

        public int AssetID { get; set; }

        public int? ActualStatus { get; set; }

        [StringLength(200)]
        public string? Discrepancy { get; set; }

        [StringLength(500)]
        public string? Note { get; set; }

        // Navigation
        public virtual User User { get; set; } = null!;
        public virtual Asset Asset { get; set; } = null!;
    }
}
