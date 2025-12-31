using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITAssetManagement.Models.Entitis
{
    public class Supplier
    {
        [Key]
        public int SupplierID { get; set; }

        [Required]
        [StringLength(200)]
        public string SupplierName { get; set; } = string.Empty;

        [StringLength(200)]
        public string? Contact { get; set; }

        [StringLength(20)]
        public string? Phone { get; set; }

        // Navigation
        public virtual ICollection<Asset> Assets { get; set; } = new List<Asset>();
    }
}
