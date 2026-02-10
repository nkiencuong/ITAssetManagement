using AutoMapper;
using ITAssetManagement.Models.Entitis;
using ITAssetManagement.Repo.Interfaces;
using ITAssetManagement.Request.Assets;
using ITAssetManagement.Response.Assets;
using ITAssetManagement.Service.Interfaces;
using Microsoft.EntityFrameworkCore; // 👈 Quan trọng: Thêm dòng này để dùng .Include
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
                    UserID = 1,
                    DepartmentID = null,
                    Note = request.ImportNote ?? "Nhập mới tài sản lần đầu",
                    ReferenceNo = "PN-" + DateTime.Now.ToString("yyyyMMddHHmmss")
                };

                await _unitOfWork.GetRepository<WarehouseTransaction>().AddAsync(importTicket);
                await _unitOfWork.CompleteAsync();

                // Load lại đầy đủ thông tin để trả về (để hiển thị đúng tên Loại ngay sau khi thêm)
                var createdAsset = await GetAssetByIdAsync(asset.AssetID);
                return _mapper.Map<AssetResponse>(createdAsset);
            }
            catch (Exception ex)
            {
                throw new Exception("Lỗi khi nhập kho: " + ex.Message);
            }
        }

  
        // 2. LẤY DANH SÁCH (ĐÃ SỬA: Dùng .Include để lấy Tên Loại & NCC)
     
        public async Task<IEnumerable<AssetResponse>> GetAllAssetsAsync()
        {
            var repo = _unitOfWork.GetRepository<Asset>();

            // 👇 SỬA QUAN TRỌNG TẠI ĐÂY 👇
            var assets = await repo.GetAll()
                                   .Include(a => a.AssetType)   // Lấy bảng Loại
                                   .Include(a => a.Supplier)    // Lấy bảng NCC
                                   .Include(a => a.Department)  // Lấy bảng Phòng ban
                                   .OrderByDescending(a => a.CreatedDate)
                                   .ToListAsync();

            return _mapper.Map<IEnumerable<AssetResponse>>(assets);
        }

        // 3. LẤY CHI TIẾT

        public async Task<Asset> GetAssetByIdAsync(int id)
        {
            var repo = _unitOfWork.GetRepository<Asset>();
            // Cũng cần Include ở đây nếu muốn xem chi tiết đầy đủ
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

            asset.AssetName = request.AssetName;
            asset.Model = request.Model;
            asset.Unit = request.Unit;
            asset.Quantity = request.Quantity;
            asset.Price = request.Price;
            asset.Location = request.Location;
            asset.Config = request.Config;
            asset.AssetTypeID = request.AssetTypeID;
            asset.SupplierID = request.SupplierID;
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