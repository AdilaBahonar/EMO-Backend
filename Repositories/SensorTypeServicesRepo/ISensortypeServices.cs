using EMO.Models.DTOs.SensorTypeDTOs;
using P3AHR.Models.DTOs.ResponseDTO;

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
