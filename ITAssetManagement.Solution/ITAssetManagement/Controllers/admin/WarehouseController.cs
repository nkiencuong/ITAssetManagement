using Microsoft.AspNetCore.Mvc;
using ITAssetManagement.Service.Interfaces;
using System;
using System.Threading.Tasks;

namespace ITAssetManagement.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WarehouseController : ControllerBase
    {
        private readonly IWarehouseService _warehouseService;
        private readonly IAllocationService _allocationService;

        public WarehouseController(IWarehouseService warehouseService, IAllocationService allocationService)
        {
            _warehouseService = warehouseService;
            _allocationService = allocationService;
        }

        // 1. Lấy Lịch sử (Thêm Query params)
        // Gọi: GET api/warehouse/history?fromDate=...&toDate=...
        [HttpGet("history")]
        public async Task<IActionResult> GetWarehouseHistory([FromQuery] DateTime? fromDate, [FromQuery] DateTime? toDate)
        {
            var data = await _warehouseService.GetHistoryAsync(fromDate, toDate);
            return Ok(data);
        }

        // 2. Lấy Danh sách phiếu cấp phát
        [HttpGet("allocations")]
        public async Task<IActionResult> GetActiveAllocations()
        {
            var data = await _allocationService.GetAllocationHistoryAsync();
            return Ok(data);
        }
    }
}