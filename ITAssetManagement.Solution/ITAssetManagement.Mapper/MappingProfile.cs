using AutoMapper;
using ITAssetManagement.Models.Entitis;
using ITAssetManagement.Request.Assets;
using ITAssetManagement.Response.Assets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ITAssetManagement.Mapper
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // 1. Map từ Request (Form nhập) sang Entity (Database)
            CreateMap<CreateAssetRequest, Asset>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => 0)) // Mặc định trạng thái là 0 (Trong kho)
                .ForMember(dest => dest.CreatedDate, opt => opt.MapFrom(src => DateTime.Now));

            // 2. Map từ Entity (Database) sang Response (Hiển thị)
            CreateMap<Asset, AssetResponse>()
                .ForMember(dest => dest.AssetTypeName, opt => opt.MapFrom(src => src.AssetType.TypeName)) // Lấy tên loại
                .ForMember(dest => dest.SupplierName, opt => opt.MapFrom(src => src.Supplier != null ? src.Supplier.SupplierName : "N/A")); // Lấy tên NCC
        }
    }
}