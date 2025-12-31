using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITAssetManagement.Models.Entitis
{
    public class AssetType
    {
        [Key]
        public int AssetTypeID { get; set; }

        [Required]
        [StringLength(100)]
        public string TypeName { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        // Navigation
        public virtual ICollection<Asset> Assets { get; set; } = new List<Asset>();
    }
}
