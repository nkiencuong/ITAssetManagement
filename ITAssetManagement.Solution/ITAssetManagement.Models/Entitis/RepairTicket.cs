using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITAssetManagement.Models.Entitis
{
    public class RepairTicket
    {
        [Key]
        public int TicketID { get; set; }

        public int AssetID { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        public DateTime? RepairDate { get; set; }

        public int? ReplacedAssetID { get; set; }

        public decimal Cost { get; set; } = 0;

        public int Status { get; set; } = 0;  // 0: Ghi nhận, 1: Đang sửa, 2: Hoàn thành

        public int UserID { get; set; } // string nếu IdentityUser.Id là string

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        [StringLength(500)]
        public string? Note { get; set; }

        // Navigation
        public virtual Asset Asset { get; set; } = null!;
        public virtual Asset? ReplacedAsset { get; set; }
        public virtual User User { get; set; } = null!;
    }
}
