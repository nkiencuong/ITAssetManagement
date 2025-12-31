// File: Client/Models/Allocation.cs
using System;
using System.Collections.Generic; // Để dùng List

namespace ITAssetManagement.Client.Models
{
    public class Allocation
    {
        // Các trường để HIỂN THỊ danh sách (GET)
        public int AllocationID { get; set; }
        public string? AssetName { get; set; }     // Tên để hiện lên bảng
        public string? DepartmentName { get; set; } // Tên phòng ban để hiện
        public DateTime AllocatedDate { get; set; }
        public string? Note { get; set; }

        // Các trường để GỬI ĐI tạo mới (POST) - Khớp với cái lỗi AssetIds nãy bạn thấy
        public List<int> AssetIds { get; set; } = new List<int>(); // Cái này quan trọng nè!
        public int DepartmentID { get; set; }
        public int Status { get; set; } = 1;
    }
}