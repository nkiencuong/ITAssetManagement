using System;
using System.Collections.Generic;

namespace ITAssetManagement.Client.Models
{
    public class Allocation
    {
        // --- PHẦN 1: Dùng để HIỂN THỊ (GET) ---
        public int AllocationID { get; set; }
        public string? AssetName { get; set; }
        public string? DepartmentName { get; set; }
        public DateTime AllocatedDate { get; set; }
        public string? Note { get; set; }

        // Mới thêm: Để hiện tên người nhận và số lượng/serial
        public string? ReceiverName { get; set; }
        public string? Serial { get; set; }

        // --- PHẦN 2: Dùng để TẠO MỚI (POST) ---
        public List<int> AssetIds { get; set; } = new List<int>();
        public int DepartmentID { get; set; }
        public int Status { get; set; } = 1; // 1: Đang cấp, 2: Đã trả
    }

    // --- PHẦN 3: Class phụ dùng cho chức năng THU HỒI ---
    public class ReturnRequest
    {
        public string Note { get; set; } = string.Empty;
    }
}