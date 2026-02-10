using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ITAssetManagement.Models.Entitis
{
    [Table("RepairTicketDetail")]
    public class RepairTicketDetail
    {
        [Key]
        public int DetailID { get; set; }

        // Liên kết với phiếu sửa chữa
        public int TicketID { get; set; }
        [ForeignKey("TicketID")]
        public virtual RepairTicket RepairTicket { get; set; }

        // Liên kết với kho tài sản (để lấy tên linh kiện)
        public int AssetID { get; set; }
        [ForeignKey("AssetID")]
        public virtual Asset Asset { get; set; }

        public int Quantity { get; set; }

        [Column(TypeName = "decimal(18, 2)")]
        public decimal Price { get; set; } // Giá lúc xuất kho

        public string Note { get; set; }
    }
}