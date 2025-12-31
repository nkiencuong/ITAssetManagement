using ITAssetManagement.Request.Allocations;
using ITAssetManagement.Response.Allocations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITAssetManagement.Service.Interfaces
{
    public interface IAllocationService
    {
        // Hàm phân bổ hàng loạt: Trả về true nếu thành công, false nếu thất bại
        Task<bool> AllocateAssetsAsync(AllocateAssetsRequest request);
        Task<IEnumerable<AllocationHistoryResponse>> GetAllocationHistoryAsync();
    }
}
