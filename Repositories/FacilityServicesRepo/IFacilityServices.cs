using EMO.Models.DTOs.FacilityDTOs;
using 
    .Models.DTOs.ResponseDTO;

namespace EMO.Repositories.FacilityServicesRepo
{
    public interface IFacilityServices
    {
        Task<ResponseModel<FacilityResponseDTO>> AddFacility(AddFacilityDTO requestDto);
        Task<ResponseModel<FacilityResponseDTO>> UpdateFacility(UpdateFacilityDTO requestDto);
        Task<ResponseModel<FacilityResponseDTO>> GetFacilityById(string facilityId);
        Task<ResponseModel<List<FacilityResponseDTO>>> GetAllFacilities();
        Task<ResponseModel> DeleteFacilityById(string facilityId);
    }
}
