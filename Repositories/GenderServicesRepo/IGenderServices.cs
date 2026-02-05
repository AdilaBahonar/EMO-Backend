using EMO.Models.DTOs.ResponseDTO;
using EMO.Models.DTOs.GenderDTOs;

namespace EMO.Repositories.GenderServicesRepo
{
    public interface IGenderServices
    {
        Task<ResponseModel<GenderResponseDTO>> AddGender(AddGenderDTO requestDto);
        Task<ResponseModel<GenderResponseDTO>> UpdateGender(UpdateGenderDTO requestDto);
        Task<ResponseModel<List<GenderResponseDTO>>> GetAllGenders();
        Task<ResponseModel<GenderResponseDTO>> GetGenderById(string id);
        Task<ResponseModel<string>> DeleteGender(string id);
    }
}
