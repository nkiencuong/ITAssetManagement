using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITAssetManagement.Request.Reports
{
    public class DashboardStatsResponse
    {
        public int TotalReceived { get; set; }  // Tổng đơn nhận
        public int Processing { get; set; }     // Đang sửa (bao gồm Mới báo + Đang xử lý)
        public int Completed { get; set; }      // Đã hoàn thành
        public double CompletionRate { get; set; } // Tỷ lệ hoàn thành (%)
    }
}