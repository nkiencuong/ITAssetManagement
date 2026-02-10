using System;

namespace ITAssetManagement.Response.RepairTickets
{
    public class RepairTicketResponse
    {
        public int TicketID { get; set; }
        public int AssetID { get; set; }
        public string? AssetName { get; set; } // Trả về tên máy luôn cho tiện
        public string? Description { get; set; }
        public int? DepartmentID { get; set; }
        public string? DepartmentName { get; set; } // Trả về tên khoa
        public decimal Cost { get; set; }
        public int Status { get; set; }
        public DateTime CreatedDate { get; set; }
        public string? Note { get; set; }

        // Thông tin linh kiện thay thế (nếu có)
        public int? ReplacedAssetID { get; set; }
        public string? ReplacedAssetName { get; set; }
    }
}