using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations; // Thêm dòng này để dùng [Required] nếu cần

namespace ITAssetManagement.Request.Allocations
{
    public class AllocateAssetsRequest
    {
        // Danh sách ID tài sản
        public List<int> AssetIds { get; set; } = new List<int>();

        // Phải viết hoa chữ 'ID' để khớp với Service
        public int DepartmentID { get; set; }

        public int? UserID { get; set; }

        public DateTime AllocatedDate { get; set; } = DateTime.Now;

        public string? Note { get; set; }
    }
}