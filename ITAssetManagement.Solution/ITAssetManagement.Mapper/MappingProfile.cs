using AutoMapper;
using ITAssetManagement.Models.Entitis;
using ITAssetManagement.Request.Assets;
using ITAssetManagement.Response.Assets;
using System;

namespace ITAssetManagement.Mapper
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // 1. Map từ Request -> Entity
            CreateMap<CreateAssetRequest, Asset>()
                // Mặc định Status là 0 (Mới)
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => 0))

              
                // CreatedDate sẽ tự lấy DateTime.Now. ImportDate sẽ tự map sang ImportDate.
                .ForMember(dest => dest.CreatedDate, opt => opt.Ignore());

            // 2. Map từ Entity -> Response
            CreateMap<Asset, AssetResponse>()
                // Lấy tên Loại, nếu null thì báo "Chưa phân loại"
                .ForMember(dest => dest.AssetTypeName, opt => opt.MapFrom(src => src.AssetType != null ? src.AssetType.TypeName : "Chưa phân loại"))
                // Lấy tên NCC, nếu null thì báo "N/A"
                .ForMember(dest => dest.SupplierName, opt => opt.MapFrom(src => src.Supplier != null ? src.Supplier.SupplierName : "N/A"));
        }
    }
}