using System;

namespace ITAssetManagement.Client.Models
{
    public class RepairTicket
    {
        public int TicketID { get; set; }
        public int? AssetID { get; set; }
        public string? Description { get; set; }
        public DateTime? RepairDate { get; set; }
        public int? ReplacedAssetID { get; set; }
        public int? DepartmentID { get; set; }
        public decimal Cost { get; set; }
        public int Status { get; set; }
        public DateTime CreatedDate { get; set; }
        public string? Note { get; set; }
        public string ReporterName { get; set; } = "";
        public string ReporterPosition { get; set; } = "";
        public string? DinhKemUrl { get; set; }
        public string? LoaiFile { get; set; }
        public string? Solution { get; set; }
        public int? AssignedToUserID { get; set; }
        public string? AssignedToUserName { get; set; } // Để hiển thị tên anh IT lên bảng
        //Các biến object để hiển thị tên(Mapping từ API)
        public Asset? Asset { get; set; }
        public Asset? ReplacedAsset { get; set; }
        public Department? Department { get; set; }
        public string? DamageStatus { get; set; }
        public List<RepairTicketDetail>? RepairDetails { get; set; }
    }
}