using System;

namespace ITAssetManagement.Response.Allocations
{
    public class AllocationHistoryResponse
    {
        public int AllocationID { get; set; }
        public string AssetName { get; set; } = string.Empty; // Tên máy

        // Sửa thành string? (cho phép null)
        // Trường này giờ sẽ hiển thị dòng chữ: "SL: 5 Cái" thay vì số Serial
        public string? Serial { get; set; }

        public string DepartmentName { get; set; } = string.Empty; // Khoa nhận
        public string ReceiverName { get; set; } = string.Empty;   // Người nhận (User)
        public DateTime AllocatedDate { get; set; } // Ngày cấp
        public string? Note { get; set; } // Cho phép null
        public int Status { get; set; } // <--- Phải có trường này
        public int Quantity { get; set; }
        public int DepartmentID { get; set; }
        public decimal Price { get; set; }
    }
}