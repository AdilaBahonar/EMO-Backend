using EMO.Models.DTOs.TenantDTOs.EMO.Models.DTOs.TenantDTOs;
using P3AHR.Models.DTOs.ResponseDTO;

namespace EMO.Repositories.TenantServicesRepo
{
    public interface ITenantServices
    {
        Task<ResponseModel<TenantResponseDTO>> AddTenant(AddTenantDTO requestDto);
        Task<ResponseModel<TenantResponseDTO>> UpdateTenant(UpdateTenantDTO requestDto);
        Task<ResponseModel<TenantResponseDTO>> GetTenantById(string tenantId);
        Task<ResponseModel<List<TenantResponseDTO>>> GetAllTenants();
        Task<ResponseModel> DeleteTenantById(string tenantId);
    }
}
