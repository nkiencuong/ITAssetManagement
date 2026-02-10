using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITAssetManagement.Request.RepairTickets
{
    public class CreateRepairTicketRequest
    {
        public int? AssetID { get; set; }
        public int? DepartmentID { get; set; }
        public string? Description { get; set; }
        public DateTime ReportDate { get; set; } = DateTime.Now;

        public string ReporterName { get; set; }
        public string ReporterPosition { get; set; }  // Chức vụ
        public string? DinhKemUrl { get; set; }
        public string? LoaiFile { get; set; }
    }
}