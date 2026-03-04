namespace ITAssetManagement.Client.Models
{
    public class AssetType
    {
        public int AssetTypeID { get; set; }
        public string TypeName { get; set; } = "";
        public string? Description { get; set; }
        public int GroupType { get; set; }
    }
}