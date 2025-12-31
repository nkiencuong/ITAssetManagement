using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITAssetManagement.Response.Allocations
{
    public class AllocationHistoryResponse
    {
        public int AllocationID { get; set; }
        public string AssetName { get; set; }      // Tên máy
        public string Serial { get; set; }         // Số serial
        public string DepartmentName { get; set; } // Khoa nhận
        public string ReceiverName { get; set; }   // Người nhận (User)
        public DateTime AllocatedDate { get; set; } // Ngày cấp
        public string Note { get; set; }
    }
}