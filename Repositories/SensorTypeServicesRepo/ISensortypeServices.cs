using EMO.Models.DTOs.ResponseDTO;
using EMO.Models.DTOs.SensorTypeDTOs;

namespace EMO.Repositories.SensorTypeServicesRepo
{
    public interface ISensortypeServices
    {
        Task<ResponseModel<SensorTypeResponseDTO>> AddSensorType(AddSensorTypeDTO requestDto);
        Task<ResponseModel<SensorTypeResponseDTO>> UpdateSensorType(UpdateSensorTypeDTO requestDto);
        Task<ResponseModel<SensorTypeResponseDTO>> GetSensorTypeById(string sensorTypeId);
        Task<ResponseModel<List<SensorTypeResponseDTO>>> GetAllSensorTypes();
        Task<ResponseModel> DeleteSensorTypeById(string sensorTypeId);
    }
}
