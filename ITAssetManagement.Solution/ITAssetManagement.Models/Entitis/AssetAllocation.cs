using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITAssetManagement.Models.Entitis
{
    public class AssetAllocation
    {
        [Key]
        public int AllocationID { get; set; }

        public int AssetID { get; set; }

        public int DepartmentID { get; set; }

        public int? UserID { get; set; }

        public DateTime AllocatedDate { get; set; } = DateTime.Now;

        public DateTime? ReturnedDate { get; set; }

        public int Status { get; set; } = 1;  // 1: Cấp phát, 2: Thu hồi, 3: Điều chuyển

        [StringLength(500)]
        public string? Note { get; set; }

        // Navigation
        public virtual Asset? Asset { get; set; } = null!;
        public virtual Department? Department { get; set; } = null!;
        public virtual User? User { get; set; }
    }
}
