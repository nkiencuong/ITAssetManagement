using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ITAssetManagement.Models.Entitis
{
    [Table("Department")] // QUAN TRỌNG: Map đúng tên bảng trong SQL
    public class Department
    {
        [Key]
        public int DepartmentID { get; set; }

        [Required]
        [StringLength(200)]
        public string DeptName { get; set; } = string.Empty; // Khớp với cột SQL của bạn

        [StringLength(50)]
        public string? Code { get; set; }

        public int? ManagerID { get; set; }

        // Navigation (Giữ nguyên để sau này dùng nếu cần)
        [ForeignKey("ManagerID")]
        public virtual User? Manager { get; set; }
    }
}