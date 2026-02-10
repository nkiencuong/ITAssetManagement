using ITAssetManagement.Models.Entitis;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema; // Cần thêm thư viện này để dùng [ForeignKey] và [Table]
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITAssetManagement.Models.Entities // Đã sửa lỗi chính tả: Entitis -> Entities
{
    [Table("InventoryCheck")] // Đảm bảo map đúng tên bảng trong SQL Server
    public class InventoryCheck
    {
        [Key]
        public int CheckID { get; set; }

        public DateTime CheckDate { get; set; } = DateTime.Now;

        public int UserID { get; set; } // Người đi kiểm kê

        public int AssetID { get; set; } // Tài sản được kiểm kê

        [StringLength(50)]
        public string ActualStatus { get; set; } = string.Empty; // Tốt, Hỏng, Mất...

        public bool Discrepancy { get; set; } // Có sai lệch so với hồ sơ không? (True/False)

        public string? Note { get; set; }

        // Navigation properties
        public virtual User? User { get; set; }
        public virtual Asset? Asset { get; set; }
    }
}