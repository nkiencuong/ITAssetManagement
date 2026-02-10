namespace ITAssetManagement.Client.Models
{
    public class RepairTicketDetail
    {
        public int DetailID { get; set; }
        public int TicketID { get; set; }

        public int AssetID { get; set; }
        public Asset? Asset { get; set; } // Để hiển thị tên linh kiện

        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public string? Note { get; set; }
    }
}