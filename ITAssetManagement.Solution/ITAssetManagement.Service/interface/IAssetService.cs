using ITAssetManagement.Models.Entitis;
using ITAssetManagement.Request.Assets;
using ITAssetManagement.Response.Assets;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ITAssetManagement.Service.Interfaces
{
    public interface IAssetService
    {
        // 1. Tạo mới
        Task<AssetResponse> CreateAssetAsync(CreateAssetRequest request);

        // 2. Lấy danh sách
        Task<IEnumerable<AssetResponse>> GetAllAssetsAsync(DateTime? fromDate = null, DateTime? toDate = null);

        // --- CÁC HÀM MỚI THÊM ---

        // 3. Lấy chi tiết theo ID (để hiển thị form sửa)
        Task<Asset> GetAssetByIdAsync(int id);

        // 4. Cập nhật tài sản
        Task<bool> UpdateAssetAsync(int id, Asset request);

        // 5. Xóa tài sản
        Task<bool> DeleteAssetAsync(int id);
        //7. Lấy tai sản theo phòng ban 
        Task<IEnumerable<AssetResponse>> GetAssetsByDepartmentAsync(int departmentId);
    }
}