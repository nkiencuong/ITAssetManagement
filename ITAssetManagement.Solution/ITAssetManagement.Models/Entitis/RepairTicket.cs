using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ITAssetManagement.Models.Entitis
{
    [Table("RepairTicket")]
    public class RepairTicket
    {
        [Key]
        public int TicketID { get; set; }

        public int? AssetID { get; set; } // Máy bị hỏng

        [StringLength(500)]
        public string? Description { get; set; } // Mô tả lỗi

        public DateTime? RepairDate { get; set; }

        // Linh kiện thay thế (Nếu có) - Lưu ý: logic này chỉ thay được 1 món
        public int? ReplacedAssetID { get; set; }

        public int? DepartmentID { get; set; } // MỚI: Thêm cái này để biết khoa nào báo hỏng

        [Column(TypeName = "decimal(18, 2)")]
        public decimal Cost { get; set; } = 0;

        public int Status { get; set; } = 0;  // 0: Mới báo, 1: Đang sửa, 2: Hoàn thành, 3: Hủy

        public int? UserID { get; set; } // Người tạo phiếu (Admin/IT)

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public string? ReporterName { get; set; } // Người báo hỏng
        public string? ReporterPosition { get; set; } // Chức vụ
        public string? Solution { get; set; } //Biện pháp khắc phục
        public string? DinhKemUrl { get; set; } // Lưu đường dẫn file (VD: /uploads/img_123.jpg)
        public string? LoaiFile { get; set; }    // Lưu loại: "Image" hoặc "Video"

        [StringLength(500)]
        public string? Note { get; set; }

        // --- Navigation Properties ---
        [ForeignKey("AssetID")]
        public virtual Asset? Asset { get; set; }

        [ForeignKey("ReplacedAssetID")]
        public virtual Asset? ReplacedAsset { get; set; } // Link tới linh kiện trong kho

        [ForeignKey("UserID")]
        public virtual User? User { get; set; }

        [ForeignKey("DepartmentID")]
        public virtual Department? Department { get; set; }
        public virtual ICollection<RepairTicketDetail> RepairDetails { get; set; }
    }
}
