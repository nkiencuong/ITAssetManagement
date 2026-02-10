using ITAssetManagement.Request.Allocations;
using ITAssetManagement.Response.Allocations;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ITAssetManagement.Service.Interfaces
{
    public interface IAllocationService
    {
        // 1. Cấp phát (Phải có tham số int actionUserId)
        Task<bool> AllocateAssetsAsync(AllocateAssetsRequest request, int actionUserId);

        // 2. Lấy danh sách lịch sử
        Task<IEnumerable<AllocationHistoryResponse>> GetAllocationHistoryAsync();

        // 3. Thu hồi (Phải có tham số DateTime returnDate)
        Task<bool> ReturnAssetAsync(int allocationId, string returnNote, DateTime returnDate);
    }
}