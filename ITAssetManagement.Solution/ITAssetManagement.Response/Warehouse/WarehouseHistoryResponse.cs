using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITAssetManagement.Response.Warehouse
{
    public class WarehouseHistoryResponse
    {
        public int TransactionID { get; set; }
        public string AssetName { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty; // IN, OUT
        public int Quantity { get; set; }
        public DateTime Date { get; set; }

        // Hiển thị tên thay vì ID
        public string DepartmentName { get; set; } = "N/A";
        public string UserName { get; set; } = "N/A"; // Người thực hiện/Người nhận

        public string Note { get; set; } = string.Empty;
        public string ReferenceNo { get; set; } = string.Empty; // Mã phiếu
    }
}
