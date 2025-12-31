using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITAssetManagement.Models.Entitis
{
    public class User
    {
        [Key]
        public int UserID { get; set; }  // int tự tăng, hoặc string nếu muốn GUID

        [Required]
        [StringLength(100)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(200)]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;  // Hash bằng BCrypt sau

        [StringLength(50)]
        public string Role { get; set; } = "Viewer";  // 'Admin', 'Tech', 'Viewer'

        [StringLength(200)]
        public string? FullName { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // Navigation properties (lịch sử thao tác, phiếu, cấp phát...)
        public virtual ICollection<WarehouseTransaction> WarehouseTransactions { get; set; } = new List<WarehouseTransaction>();
        public virtual ICollection<AssetAllocation> AssetAllocations { get; set; } = new List<AssetAllocation>();
        public virtual ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
        public virtual ICollection<InventoryCheck> InventoryChecks { get; set; } = new List<InventoryCheck>();
        public virtual ICollection<RepairTicket> RepairTickets { get; set; } = new List<RepairTicket>();
        public virtual Department? ManagedDepartment { get; set; }  // Nếu là manager phòng ban
    }
}

