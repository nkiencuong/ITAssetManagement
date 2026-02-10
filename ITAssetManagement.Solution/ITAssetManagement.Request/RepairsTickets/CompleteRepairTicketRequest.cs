using System;
using System.Collections.Generic;

namespace ITAssetManagement.Request.RepairTickets
{
    // Class con: Đại diện cho 1 dòng linh kiện (Ví dụ: Hộp mực - SL: 1)
    public class RepairItemDto
    {
        public int AssetId { get; set; }  // ID linh kiện trong kho
        public int Quantity { get; set; } // Số lượng dùng
    }

    public class CompleteRepairTicketRequest
    {
        // Cách khắc phục (Mục III)
        public string Solution { get; set; } = "";

        // Danh sách linh kiện thay thế (Mục IV) - QUAN TRỌNG
        public List<RepairItemDto> Parts { get; set; } = new List<RepairItemDto>();

        public DateTime CompletedDate { get; set; } = DateTime.Now;
    }
}