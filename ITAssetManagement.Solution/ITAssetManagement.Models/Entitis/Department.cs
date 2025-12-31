using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITAssetManagement.Models.Entitis
{
    public class Department
    {
        [Key]
        public int DepartmentID { get; set; }

        [Required]
        [StringLength(200)]
        public string DeptName { get; set; } = string.Empty;

        [StringLength(50)]
        public string? Code { get; set; }

        public int? ManagerID { get; set; }  // FK to User

        // Navigation
        public virtual User? Manager { get; set; }  // Nếu dùng Identity custom User
        public virtual ICollection<AssetAllocation> AssetAllocations { get; set; } = new List<AssetAllocation>();
        public virtual ICollection<WarehouseTransaction> WarehouseTransactions { get; set; } = new List<WarehouseTransaction>();
    }
}
