using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITAssetManagement.Models.Entitis
{
    public class AuditLog
    {
        [Key]
        public int LogID { get; set; }

        public int UserID { get; set; }

        [Required]
        [StringLength(100)]
        public string Action { get; set; } = string.Empty;

        [StringLength(50)]
        public string? TableName { get; set; }

        public int? RecordID { get; set; }

        public string? OldValue { get; set; }

        public string? NewValue { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.Now;
        public DateTime ActionDate { get; set; } = DateTime.Now;
        public string? Details { get; set; }

        // Navigation
        public virtual User? User { get; set; } = null!;
    }
}
