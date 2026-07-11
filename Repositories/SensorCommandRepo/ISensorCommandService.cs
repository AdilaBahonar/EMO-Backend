using EMO.Models.DTOs.ResponseDTO;
using EMO.Models.DTOs.SensorCommandDTOs;

namespace EMO.Repositories.SensorCommandRepo
{
    public interface ISensorCommandService
    {
        Task<ResponseModel<SensorRelayCommandResponseDTO>> SendRelayCommandAsync(SensorRelayCommandRequestDTO requestDto);
    }
}
