using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITAssetManagement.Models.Entitis
// Lưu ý: Chữ 'Entitis' này phải giống hệt tên thư mục của bác. Nếu bác dùng 'Entities' thì đổi lại nhé.
{
    [Table("Notification")]
    public class Notification
    {
        [Key]
        public int NotificationID { get; set; }

        public int UserID { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Message { get; set; } = string.Empty;

        public string? RelatedUrl { get; set; }

        public bool IsRead { get; set; }

        public DateTime CreatedAt { get; set; }

        // Khóa ngoại nối sang bảng User
        [ForeignKey("UserID")]
        public virtual User? User { get; set; }
    }
}