using AutoMapper;
using ITAssetManagement.Models.Entitis;
using ITAssetManagement.Repo.Interfaces;
using ITAssetManagement.Request.Assets;
using ITAssetManagement.Response.Assets;
using ITAssetManagement.Service.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ITAssetManagement.Service.Services
{
    public class AssetService : IAssetService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public AssetService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<AssetResponse> CreateAssetAsync(CreateAssetRequest request)
        {
            try
            {
                // BƯỚC 1: Map dữ liệu từ Form nhập (Request) sang Entity (Asset)
                var asset = _mapper.Map<Asset>(request);

                // Xử lý logic: Nếu không có Serial (ví dụ nhập chuột/phím lô), tự sinh mã tạm
                if (string.IsNullOrEmpty(asset.Serial))
                {
                    asset.Serial = "GEN-" + Guid.NewGuid().ToString().Substring(0, 8).ToUpper();
                }

                // Thêm tài sản vào hàng chờ (chưa lưu xuống DB ngay)
                await _unitOfWork.GetRepository<Asset>().AddAsync(asset);

                // BƯỚC 2: Tự động tạo "Phiếu kiểm nhập" (WarehouseTransaction)
                // Theo đúng quy trình tờ giấy: Nhập -> Phiếu kiểm nhập
                var importTicket = new WarehouseTransaction
                {
                    Asset = asset, // EF Core tự hiểu liên kết asset này với phiếu nhập
                    Type = "IN",   // Loại phiếu: Nhập kho
                    Quantity = 1,  // Mặc định là 1 (với tài sản cố định)
                    Date = DateTime.Now,
                    UserID = 1, // Tạm thời để ID admin (sau này sẽ lấy ID người đăng nhập)
                    Note = request.ImportNote ?? "Nhập mới tài sản", // Ghi chú từ form nhập
                    ReferenceNo = "PN-" + DateTime.Now.ToString("yyyyMMddHHmmss") // Số phiếu tự sinh
                };

                // Thêm phiếu vào hàng chờ
                await _unitOfWork.GetRepository<WarehouseTransaction>().AddAsync(importTicket);

                // BƯỚC 3: Lưu tất cả xuống Database (Commit Transaction)
                // Nếu bước này lỗi, cả Tài sản và Phiếu nhập đều không được lưu -> Dữ liệu sạch
                await _unitOfWork.CompleteAsync();

                // Trả về kết quả để hiển thị
                return _mapper.Map<AssetResponse>(asset);
            }
            catch (Exception ex)
            {
                // Sau này sẽ ghi log lỗi ở đây
                throw new Exception("Lỗi khi nhập kho: " + ex.Message);
            }
        }

        public async Task<IEnumerable<AssetResponse>> GetAllAssetsAsync()
        {
            // Lấy tất cả tài sản từ DB
            var assets = await _unitOfWork.GetRepository<Asset>().GetAllAsync();

            // Map sang dạng hiển thị (Response)
            return _mapper.Map<IEnumerable<AssetResponse>>(assets);
        }
    }
}