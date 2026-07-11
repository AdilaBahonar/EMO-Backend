using EMO.Models.DTOs.ResponseDTO;
using EMO.Models.DTOs.SensorApplianceDTOs;

namespace EMO.Repositories.SensorApplianceServicesRepo
{
    public interface ISensorApplianceServices
    {
        Task<ResponseModel<SensorApplianceResponseDTO>> AssignApplianceToSensor(AssignSensorApplianceDTO requestDto);
        Task<ResponseModel<SensorApplianceResponseDTO>> UpdateSensorAppliance(UpdateSensorApplianceDTO requestDto);
        Task<ResponseModel<SensorApplianceResponseDTO>> GetSensorApplianceById(string sensorApplianceId);
        Task<ResponseModel<SensorApplianceResponseDTO>> GetActiveApplianceBySensorId(string sensorId);
        Task<ResponseModel<SensorAssignableAppliancesDTO>> GetAssignableBusinessAppliancesBySensorId(string sensorId);
        Task<ResponseModel<List<SensorApplianceResponseDTO>>> GetAllSensorAppliances();
        Task<ResponseModel<List<SensorApplianceResponseDTO>>> GetSensorAppliancesByApplianceId(string applianceId);
        Task<ResponseModel<ApplianceStatusDTO>> GetSensorApplianceStatus(string sensorId);
        Task<ResponseModel> DeleteSensorApplianceById(string sensorApplianceId);
    }
}
