using EMO.Models.DTOs.ApplianceDTOs;
using EMO.Models.DTOs.ResponseDTO;

namespace EMO.Repositories.ApplianceServicesRepo
{
    public interface IApplianceServices
    {
        Task<ResponseModel<ApplianceResponseDTO>> AddAppliance(AddApplianceDTO requestDto);
        Task<ResponseModel<ApplianceResponseDTO>> UpdateAppliance(UpdateApplianceDTO requestDto);
        Task<ResponseModel<ApplianceResponseDTO>> GetApplianceById(string applianceId);
        Task<ResponseModel<List<ApplianceResponseDTO>>> GetAllAppliances();
        Task<ResponseModel<List<ApplianceResponseDTO>>> GetAppliancesByUtilityId(string utilityId);
        Task<ResponseModel> DeleteApplianceById(string applianceId);
        Task<ResponseModel<List<ApplianceResponseDTO>>> SeedDefaultAppliances();
        Task<ResponseModel<List<ApplianceResponseDTO>>> SeedBusinessDefaultAppliances(string businessId);
        Task<ResponseModel<ApplianceResponseDTO>> AddBusinessAppliance(AddApplianceDTO requestDto);
        Task<ResponseModel<ApplianceResponseDTO>> UpdateBusinessAppliance(UpdateApplianceDTO requestDto);
        Task<ResponseModel<ApplianceResponseDTO>> GetBusinessApplianceById(string businessApplianceId);
        Task<ResponseModel<List<ApplianceResponseDTO>>> GetBusinessAppliances(string businessId);
        Task<ResponseModel<List<ApplianceResponseDTO>>> GetBusinessAppliancesByUtilityId(string businessId, string utilityId);
        Task<ResponseModel> DeleteBusinessApplianceById(string businessApplianceId);
    }
}
