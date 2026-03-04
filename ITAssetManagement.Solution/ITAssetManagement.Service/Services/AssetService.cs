using AutoMapper;
using ITAssetManagement.Models.Entitis;
using ITAssetManagement.Repo.Interfaces;
using ITAssetManagement.Request.Assets;
using ITAssetManagement.Response.Assets;
using ITAssetManagement.Service.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
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

        // 1. TẠO MỚI TÀI SẢN
        public async Task<AssetResponse> CreateAssetAsync(CreateAssetRequest request)
        {
            try
            {
                // --- BƯỚC 1: XỬ LÝ LOẠI TÀI SẢN ---
                int finalTypeId;
                if (request.AssetTypeID > 0)
                {
                    finalTypeId = request.AssetTypeID;
                }
                else
                {
                    var typeRepo = _unitOfWork.GetRepository<AssetType>();
                    var allTypes = await typeRepo.GetAllAsync();
                    string inputTypeName = string.IsNullOrWhiteSpace(request.AssetTypeName) ? "Thiết bị chung" : request.AssetTypeName.Trim();
                    var existingType = allTypes.FirstOrDefault(t => t.TypeName.Equals(inputTypeName, StringComparison.OrdinalIgnoreCase));

                    if (existingType != null)
                        finalTypeId = existingType.AssetTypeID;
                    else
                    {
                        var newType = new AssetType { TypeName = inputTypeName };
                        await typeRepo.AddAsync(newType);
                        await _unitOfWork.CompleteAsync();
                        finalTypeId = newType.AssetTypeID;
                    }
                }

                // --- BƯỚC 2: XỬ LÝ NHÀ CUNG CẤP ---
                int finalSupplierId;
                if (request.SupplierID > 0)
                {
                    finalSupplierId = request.SupplierID;
                }
                else
                {
                    var supplierRepo = _unitOfWork.GetRepository<Supplier>();
                    var allSuppliers = await supplierRepo.GetAllAsync();
                    string inputSupplierName = string.IsNullOrWhiteSpace(request.SupplierName) ? "Kho Tổng" : request.SupplierName.Trim();
                    var existingSupplier = allSuppliers.FirstOrDefault(s => s.SupplierName.Equals(inputSupplierName, StringComparison.OrdinalIgnoreCase));

                    if (existingSupplier != null)
                        finalSupplierId = existingSupplier.SupplierID;
                    else
                    {
                        var newSupplier = new Supplier { SupplierName = inputSupplierName };
                        await supplierRepo.AddAsync(newSupplier);
                        await _unitOfWork.CompleteAsync();
                        finalSupplierId = newSupplier.SupplierID;
                    }
                }

                // --- BƯỚC 3: MAP DỮ LIỆU ---
                var asset = _mapper.Map<Asset>(request);
                asset.AssetTypeID = finalTypeId;
                asset.SupplierID = finalSupplierId;
                asset.ImportDate = request.ImportDate;

                // 👇👇 QUAN TRỌNG: Gán ModelSeries (Model Cha) vào đây 👇👇
                asset.ModelSeries = request.ModelSeries;
                // 👆👆 ----------------------------------------------- 👆👆

                if (asset.Quantity <= 0) asset.Quantity = 1;
                if (string.IsNullOrEmpty(asset.Unit)) asset.Unit = "Cái";
                asset.Status = 0; // Mới nhập

                await _unitOfWork.GetRepository<Asset>().AddAsync(asset);

                // --- BƯỚC 4: TẠO PHIẾU NHẬP KHO ---
                var importTicket = new WarehouseTransaction
                {
                    Asset = asset,
                    Type = "IN",
                    Quantity = asset.Quantity,
                    Date = request.ImportDate,
                    UserID = request.UserID > 0 ? request.UserID : 1,
                    DepartmentID = null,
                    Note = request.ImportNote ?? "Nhập mới tài sản lần đầu",
                    ReferenceNo = "PN-" + DateTime.Now.ToString("yyyyMMddHHmmss")
                };

                await _unitOfWork.GetRepository<WarehouseTransaction>().AddAsync(importTicket);
                await _unitOfWork.CompleteAsync();

                // Load lại đầy đủ thông tin để trả về
                var createdAsset = await GetAssetByIdAsync(asset.AssetID);
                return _mapper.Map<AssetResponse>(createdAsset);
            }
            catch (Exception ex)
            {
                // In chi tiết lỗi Inner Exception nếu có để dễ debug
                var msg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                throw new Exception("Lỗi khi nhập kho: " + msg);
            }
        }

        // 2. LẤY DANH SÁCH
        public async Task<IEnumerable<AssetResponse>> GetAllAssetsAsync()
        {
            var repo = _unitOfWork.GetRepository<Asset>();

            var assets = await repo.GetAll()
                                   .Include(a => a.AssetType)
                                   .Include(a => a.Supplier)
                                   .Include(a => a.Department)
                                   .OrderByDescending(a => a.CreatedDate)
                                   .ToListAsync();

            return _mapper.Map<IEnumerable<AssetResponse>>(assets);
        }

        // 3. LẤY CHI TIẾT
        public async Task<Asset> GetAssetByIdAsync(int id)
        {
            var repo = _unitOfWork.GetRepository<Asset>();
            return await repo.GetAll()
                             .Include(a => a.AssetType)
                             .Include(a => a.Supplier)
                             .Include(a => a.Department)
                             .FirstOrDefaultAsync(a => a.AssetID == id);
        }

        // 4. CẬP NHẬT
        public async Task<bool> UpdateAssetAsync(int id, Asset request)
        {
            var repo = _unitOfWork.GetRepository<Asset>();
            var asset = await repo.GetByIdAsync(id);
            if (asset == null) throw new Exception("Không tìm thấy tài sản!");

            // 🚀🚀🚀 XỬ LÝ LƯU HÃNG / NHÀ CUNG CẤP TỪ TEXT NHẬP VÀO 🚀🚀🚀
            if (!string.IsNullOrWhiteSpace(request.SupplierName))
            {
                var supplierRepo = _unitOfWork.GetRepository<Supplier>();
                var allSuppliers = await supplierRepo.GetAllAsync();
                var inputName = request.SupplierName.Trim();

                // Tìm xem chữ "Canon" bác gõ đã có trong bảng Supplier chưa
                var existingSupplier = allSuppliers.FirstOrDefault(s => s.SupplierName.Equals(inputName, StringComparison.OrdinalIgnoreCase));

                if (existingSupplier != null)
                {
                    // Có rồi thì lấy ID của nó gán vào
                    asset.SupplierID = existingSupplier.SupplierID;
                }
                else
                {
                    // Chưa có thì tạo mới một Hãng/Công ty luôn
                    var newSupplier = new Supplier { SupplierName = inputName };
                    await supplierRepo.AddAsync(newSupplier);
                    await _unitOfWork.CompleteAsync(); // Lưu nháp để lấy ID mới
                    asset.SupplierID = newSupplier.SupplierID;
                }
            }
            else
            {
                asset.SupplierID = request.SupplierID; // Fallback giữ nguyên nếu không nhập gì
            }
            // 🚀🚀🚀 KẾT THÚC XỬ LÝ HÃNG 🚀🚀🚀

            asset.AssetName = request.AssetName;
            asset.Model = request.Model;

            // 👇👇 QUAN TRỌNG: Cập nhật ModelSeries khi sửa 👇👇
            asset.ModelSeries = request.ModelSeries;
            // 👆👆 ------------------------------------------ 👆👆

            asset.Unit = request.Unit;
            asset.Quantity = request.Quantity;
            asset.Price = request.Price;
            asset.Location = request.Location;
            asset.Config = request.Config;
            asset.AssetTypeID = request.AssetTypeID;

            // Đã ẩn dòng này vì mình vừa xử lý SupplierID xịn xò ở trên rồi
            // asset.SupplierID = request.SupplierID; 

            asset.DepartmentID = request.DepartmentID;
            asset.Status = request.Status;

            repo.Update(asset);
            await _unitOfWork.CompleteAsync();
            return true;
        }

        // 5. XÓA
        public async Task<bool> DeleteAssetAsync(int id)
        {
            var repo = _unitOfWork.GetRepository<Asset>();
            var asset = await repo.GetByIdAsync(id);
            if (asset == null) return false;

            repo.Delete(asset);
            await _unitOfWork.CompleteAsync();
            return true;
        }

        // 6. XỬ LÝ SỬA CHỮA
        public async Task<bool> ProcessMaintenanceAsync(int assetId, string reason, int userId)
        {
            var repo = _unitOfWork.GetRepository<Asset>();
            var asset = await repo.GetByIdAsync(assetId);
            if (asset == null) throw new Exception("Không tìm thấy tài sản!");

            asset.Status = 3;

            var transaction = new WarehouseTransaction
            {
                AssetID = assetId,
                Type = "MAINTENANCE",
                Quantity = 1,
                Date = DateTime.Now,
                UserID = userId,
                DepartmentID = asset.DepartmentID,
                Note = $"Chuyển đi sửa chữa. Lý do: {reason}",
                ReferenceNo = $"MAIN-{DateTime.Now:yyyyMMddHHmmss}"
            };

            await _unitOfWork.GetRepository<WarehouseTransaction>().AddAsync(transaction);
            repo.Update(asset);
            await _unitOfWork.CompleteAsync();
            return true;
        }
    }
}