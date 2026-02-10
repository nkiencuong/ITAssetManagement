using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Threading.Tasks;

namespace ITAssetManagement.Controllers // Chú ý namespace cho đúng với project của bác
{
    [Route("api/[controller]")]
    [ApiController]
    public class UploadController : ControllerBase
    {
        // API này nhận file, lưu vào ổ cứng, trả về đường dẫn
        [HttpPost]
        public async Task<IActionResult> Upload(IFormFile file)
        {
            // 1. Kiểm tra xem có file gửi lên không
            if (file == null || file.Length == 0)
                return BadRequest("Vui lòng chọn file!");

            try
            {
                // 2. Xác định thư mục lưu trữ (Thư mục wwwroot/uploads)
                // Directory.GetCurrentDirectory() là thư mục gốc của server
                var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");

                // Nếu thư mục chưa có thì tạo mới luôn
                if (!Directory.Exists(uploadPath))
                {
                    Directory.CreateDirectory(uploadPath);
                }

                // 3. Đặt tên file mới (Dùng Guid để đảm bảo không bao giờ trùng tên)
                // Ví dụ: file gốc là "anh.jpg" -> thành "d9203-12930-12301.jpg"
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);

                // Đường dẫn tuyệt đối trên ổ cứng (để lệnh copy nó biết đường copy vào)
                var filePath = Path.Combine(uploadPath, fileName);

                // 4. Thực hiện copy file từ luồng mạng vào ổ cứng
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // 5. Trả về đường dẫn tương đối (để Client lưu vào Database)
                // Ví dụ: /uploads/d9203-12930-12301.jpg
                var url = $"/uploads/{fileName}";

                return Ok(new { Url = url });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Lỗi server: {ex.Message}");
            }
        }
    }
}