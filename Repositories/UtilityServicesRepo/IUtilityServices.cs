using EMO.Models.DTOs.UtilityDTOs.EMO.Models.DTOs.UtilityDTOs;
using P3AHR.Models.DTOs.ResponseDTO;

namespace EMO.Repositories.UtilityServicesRepo
{
    public interface IUtilityServices
    {
        Task<ResponseModel<UtilityResponseDTO>> AddUtility(AddUtilityDTO requestDto);
        Task<ResponseModel<UtilityResponseDTO>> UpdateUtility(UpdateUtilityDTO requestDto);
        Task<ResponseModel<UtilityResponseDTO>> GetUtilityById(string utilityId);
        Task<ResponseModel<List<UtilityResponseDTO>>> GetAllUtilities();
        Task<ResponseModel> DeleteUtilityById(string utilityId);
    }
}
