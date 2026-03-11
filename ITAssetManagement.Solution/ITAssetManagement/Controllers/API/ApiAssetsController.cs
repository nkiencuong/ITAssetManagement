using ITAssetManagement.Models.Entitis; // Hoặc namespace chứa Asset model của bạn
using ITAssetManagement.Request.Assets;
using ITAssetManagement.Service.Interfaces;
using Microsoft.AspNetCore.Mvc;
using ITAssetManagement.Repo.Interfaces; // 🚀 MỚI THÊM 1: Để gọi Database
using Microsoft.EntityFrameworkCore; // 🚀 MỚI THÊM 2: Để dùng lệnh tìm kiếm FirstOrDefaultAsync

namespace ITAssetManagement.Controllers.Api
{
    [Route("api/assets")] // Đường dẫn API chuẩn: domain/api/assets
    [ApiController]
    public class ApiAssetsController : ControllerBase
    {
        private readonly IAssetService _assetService;
        private readonly IUnitOfWork _unitOfWork; // 🚀 MỚI THÊM 3: Khai báo UnitOfWork

        // 🚀 MỚI THÊM 4: Bơm (Inject) UnitOfWork vào hàm khởi tạo
        public ApiAssetsController(IAssetService assetService, IUnitOfWork unitOfWork)
        {
            _assetService = assetService;
            _unitOfWork = unitOfWork;
        }

        // POST: api/assets
        // Chức năng: Nhập tài sản mới
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateAssetRequest request)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            try
            {
                // 🚀🚀🚀 BƯỚC 2: MỚI BỔ SUNG LẤY ID NGƯỜI ĐĂNG NHẬP 🚀🚀🚀
                var userIdString = User.FindFirst("UserID")?.Value
                                ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

                if (int.TryParse(userIdString, out int loggedInUserId))
                {
                    request.UserID = loggedInUserId; // Bắt được ID thì nhét vào giỏ
                }
                else
                {
                    request.UserID = 1; // Fallback an toàn (tránh lỗi nếu lỡ chưa đăng nhập)
                }
                // 🚀🚀🚀 KẾT THÚC LẤY ID 🚀🚀🚀


                // 🚀🚀🚀 BẮT ĐẦU ĐOẠN CODE CỘNG DỒN MỚI THÊM 🚀🚀🚀
                if (!string.IsNullOrEmpty(request.AssetName))
                {
                    var assetRepo = _unitOfWork.GetRepository<Asset>();
                    var reqName = request.AssetName.Trim().ToLower();

                    // 🛠️ ĐÃ FIX LỖI Ở ĐÂY: Dùng GetAllAsync() có sẵn của bác
                    var allAssets = await assetRepo.GetAllAsync();
                    //  var existingAsset = allAssets.FirstOrDefault(a => a.AssetName.Trim().ToLower() == reqName);
                    // 🚀 ĐIỀU KIỆN CỘNG DỒN NGHIÊM NGẶT HƠN (Cùng Tên + Cùng Giá + Cùng Nhà Cung Cấp + Cùng Năm Nhập)
                    var existingAsset = allAssets.FirstOrDefault(a =>
                        a.AssetName.Trim().ToLower() == reqName &&
                        a.Price == request.Price &&
                        a.SupplierID == request.SupplierID &&
                        a.ImportDate.Year == request.ImportDate.Year // Thêm cái này để soi chuẩn lô theo năm
                    );
                    if (existingAsset != null)
                    {
                        // 🎯 TÌNH HUỐNG 1: TÊN ĐÃ TỒN TẠI -> CHỈ CỘNG DỒN SỐ LƯỢNG (Không tạo mới)
                        existingAsset.Quantity += request.Quantity;

                        assetRepo.Update(existingAsset);

                        // 🚀 LƯU Ý: NẾU CỘNG DỒN, MÌNH CŨNG PHẢI GHI LỊCH SỬ KHO (DÙNG ID VỪA LẤY)
                        var addTicket = new WarehouseTransaction
                        {
                            AssetID = existingAsset.AssetID,
                            Type = "IN",
                            Quantity = request.Quantity,
                            Date = request.ImportDate,
                            UserID = request.UserID > 0 ? request.UserID : 1, // Dùng ID người đăng nhập
                            DepartmentID = null,
                            Note = "Nhập thêm số lượng (Cộng dồn)",
                            ReferenceNo = "PN-ADD-" + DateTime.Now.ToString("yyyyMMddHHmmss")
                        };
                        await _unitOfWork.GetRepository<WarehouseTransaction>().AddAsync(addTicket);

                        await _unitOfWork.CommitAsync();

                        // Trả về luôn, kết thúc chu trình
                        return Ok(new { message = $"Đã cộng dồn thêm {request.Quantity} vào '{existingAsset.AssetName}' có sẵn!" });
                    }
                }
                // 🚀🚀🚀 KẾT THÚC ĐOẠN CODE CỘNG DỒN 🚀🚀🚀

                // 🎯 TÌNH HUỐNG 2: TÊN CHƯA CÓ TRONG KHO -> GIỮ NGUYÊN CODE CŨ CỦA BÁC
                var result = await _assetService.CreateAssetAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }
        // --- MỚI THÊM: SỬA (UPDATE) ---
        // PUT: api/assets/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] Asset request)
        {
            try
            {
                // Gọi Service xử lý cập nhật (Bạn cần đảm bảo Service có hàm UpdateAssetAsync)
                // Lưu ý: request ở đây có thể là Asset model hoặc UpdateAssetRequest tùy bạn định nghĩa
                var result = await _assetService.UpdateAssetAsync(id, request);

                if (result)
                    return Ok(new { message = "Cập nhật thành công" });
                else
                    return BadRequest(new { message = "Cập nhật thất bại" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // --- MỚI THÊM: XÓA (DELETE) ---
        // DELETE: api/assets/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                // Gọi Service xử lý xóa
                var result = await _assetService.DeleteAssetAsync(id);

                if (result)
                    return Ok(new { message = "Xóa thành công" });
                else
                    return NotFound(new { message = "Không tìm thấy tài sản để xóa" });
            }
            catch (Exception ex)
            {
                // Quan trọng: Bắt lỗi Foreign Key (Ràng buộc dữ liệu)
                // Nếu tài sản đã từng được cấp phát hoặc nhập kho, SQL sẽ không cho xóa cứng.
                // Trả về lỗi 400 để Frontend hiển thị thông báo.
                return BadRequest(new { message = "Không thể xóa tài sản này vì nó đang được sử dụng hoặc có lịch sử giao dịch!" });
            }
        }
    }
}