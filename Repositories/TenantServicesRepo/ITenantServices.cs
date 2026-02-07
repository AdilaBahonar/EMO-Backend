using EMO.Models.DTOs.ResponseDTO;
using EMO.Models.DTOs.TenantDTOs;

namespace EMO.Repositories.TenantServicesRepo
{
    public interface ITenantServices
    {
        Task<ResponseModel<TenantResponseDTO>> AddTenant(AddTenantDTO requestDto);
        Task<ResponseModel<TenantResponseDTO>> UpdateTenant(UpdateTenantDTO requestDto);
        Task<ResponseModel<TenantResponseDTO>> GetTenantById(string tenantId);
        Task<ResponseModel<List<TenantResponseDTO>>> GetAllTenants();
        Task<ResponseModel> DeleteTenantById(string tenantId);
        public Task<ResponseModel> AssignTenant(AssignTenantDTO requestDto);
        public Task<ResponseModel<List<tenantResponseDTO>>> GetTenantByBusinessId(string BusinessId);
    }
}
