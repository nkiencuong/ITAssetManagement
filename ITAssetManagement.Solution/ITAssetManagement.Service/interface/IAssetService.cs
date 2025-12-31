using ITAssetManagement.Request.Assets;
using ITAssetManagement.Response.Assets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITAssetManagement.Service.Interfaces
{
    public interface IAssetService
    {
        // Hàm tạo tài sản mới (xử lý cả nhập kho + tạo phiếu kiểm nhập)
        Task<AssetResponse> CreateAssetAsync(CreateAssetRequest request);

        // Hàm lấy danh sách tài sản (để hiện lên lưới)
        Task<IEnumerable<AssetResponse>> GetAllAssetsAsync();
    }
}