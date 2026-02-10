using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITAssetManagement.Response.InventoryCheck
{
    public class InventoryCheckResponse
    {
        public int CheckID { get; set; }
        public DateTime CheckDate { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string AssetName { get; set; } = string.Empty;
        public string ActualStatus { get; set; } = string.Empty;
        public bool Discrepancy { get; set; }
        public string Note { get; set; } = string.Empty;
    }
}
