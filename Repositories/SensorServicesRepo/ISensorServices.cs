using EMO.Models.DTOs.SensorDTOs;
using P3AHR.Models.DTOs.ResponseDTO;

namespace EMO.Repositories.SensorServicesRepo
{
    public interface ISensorServices
    {
        Task<ResponseModel<SensorResponseDTO>> AddSensor(AddSensorDTO requestDto);
        Task<ResponseModel<SensorResponseDTO>> UpdateSensor(UpdateSensorDTO requestDto);
        Task<ResponseModel<SensorResponseDTO>> GetSensorById(string sensorId);
        Task<ResponseModel<List<SensorResponseDTO>>> GetAllSensors();
        Task<ResponseModel> DeleteSensorById(string sensorId);
    }
}
