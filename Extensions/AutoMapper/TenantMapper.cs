using EMO.Models.DBModels.DBTables;
using EMO.Models.DTOs.TenantDTOs;
using AutoMapper;
using EMO.Extensions;

namespace EMO.Extensions.AutoMapper
{
    public class TenantMapper : Profile
    {
        private readonly OtherServices otherServices = new();

        public TenantMapper()
        {
            CreateMap<AddTenantDTO, tbl_tenant>()
                .ForMember(d => d.tenant_name, opt => opt.MapFrom(src => src.tenantName))
                .ForMember(d => d.tenant_ntn, opt => opt.MapFrom(src => src.tenantNtn))
                .ForMember(d => d.tenant_address, opt => opt.MapFrom(src => src.tenantAddress))
                .ForMember(d => d.tenant_coin, opt => opt.MapFrom(src => src.tenantCoin));

            CreateMap<UpdateTenantDTO, tbl_tenant>()
                .ForMember(d => d.tenant_name, opt => opt.MapFrom((src, dest) => otherServices.Check(src.tenantName) ? src.tenantName : dest.tenant_name))
                .ForMember(d => d.tenant_ntn, opt => opt.MapFrom((src, dest) => otherServices.Check(src.tenantNtn) ? src.tenantNtn : dest.tenant_ntn))
                .ForMember(d => d.tenant_address, opt => opt.MapFrom((src, dest) => otherServices.Check(src.tenantAddress) ? src.tenantAddress : dest.tenant_address))
                .ForMember(d => d.tenant_coin, opt => opt.MapFrom((src, dest) => otherServices.Check(src.tenantCoin) ? src.tenantCoin : dest.tenant_coin))
                .ForMember(d => d.is_active, opt => opt.MapFrom((src, dest) => src.isActive))
                .ForMember(d => d.updated_at, opt => opt.MapFrom((src, dest) => DateTime.Now));

            CreateMap<tbl_tenant, TenantResponseDTO>()
                .ForMember(d => d.tenantId, opt => opt.MapFrom(src => src.tenant_id.ToString()))
                .ForMember(d => d.tenantName, opt => opt.MapFrom(src => src.tenant_name))
                .ForMember(d => d.tenantNtn, opt => opt.MapFrom(src => src.tenant_ntn))
                .ForMember(d => d.tenantAddress, opt => opt.MapFrom(src => src.tenant_address))
                .ForMember(d => d.tenantCoin, opt => opt.MapFrom(src => src.tenant_coin))
                .ForMember(d => d.createdAt, opt => opt.MapFrom(src => src.created_at.ToString()))
                .ForMember(d => d.updatedAt, opt => opt.MapFrom(src => src.updated_at != null ? src.updated_at.ToString() : string.Empty))
                .ForMember(d => d.isActive, opt => opt.MapFrom(src => src.is_active));
        }
    }
}
