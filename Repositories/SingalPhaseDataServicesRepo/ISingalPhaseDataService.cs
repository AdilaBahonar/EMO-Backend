using EMO.Models.DTOs.ResponseDTO;
using EMO.Models.DTOs.SingalPhaseDTOs;

namespace EMO.Repositories.SingalPhaseDataServicesRepo
{
    public interface ISingalPhaseDataService
    {
        Task<ResponseModel<SingalPhaseDataResponseDTO>> AddSingalPhaseData(AddSingalPhaseDataDTO requestDto);
        Task<ResponseModel<SingalPhaseDataResponseDTO>> UpdateSingalPhaseData(UpdateSingalPhaseDataDTO requestDto);
        Task<ResponseModel<SingalPhaseDataResponseDTO>> GetSingalPhaseDataById(Guid singalPhaseDataId);
        Task<ResponseModel<List<SingalPhaseDataResponseDTO>>> GetAllSingalPhaseData();
        Task<ResponseModel> DeleteSingalPhaseDataById(Guid singalPhaseDataId);
    }
}
