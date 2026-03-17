using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ITAssetManagement.Request.Allocations
{
    public class AllocateAssetsRequest
    {
        // Danh sách ID tài sản
        public List<int> AssetIds { get; set; } = new List<int>();

        // ID phòng ban là bắt buộc
        public int DepartmentID { get; set; }

        // Người nhận có thể để trống
        public int? UserID { get; set; }

        // Ngày cấp phát (Hứng từ giao diện)
        public DateTime AllocatedDate { get; set; } = DateTime.Now;

        public string? Note { get; set; }
        public int Quantity { get; set; } = 1;
    }
    public class EditAllocationRequest
    {
        public int DepartmentID { get; set; }
        public int Quantity { get; set; }
        public DateTime AllocatedDate { get; set; }
        public string ReceiverName { get; set; } = "";
        public decimal Price { get; set; }
    }
}