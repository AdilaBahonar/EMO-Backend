using EMO.Models.DTOs.HvacLoopSettingDTOs;
using EMO.Models.DTOs.ResponseDTO;

namespace EMO.Repositories.HvacLoopSettingServicesRepo
{
    public interface IHvacLoopSettingServices
    {
        Task<ResponseModel<HvacLoopSettingResponseDTO>> AddHvacLoopSetting(AddHvacLoopSettingDTO requestDto);
        Task<ResponseModel<HvacLoopSettingResponseDTO>> UpdateHvacLoopSetting(UpdateHvacLoopSettingDTO requestDto);
        Task<ResponseModel<List<HvacLoopSettingResponseDTO>>> GetAllHvacLoopSettings();
        Task<ResponseModel<HvacLoopSettingResponseDTO>> GetHvacLoopSettingById(string id);
        Task<ResponseModel<HvacLoopSettingResponseDTO>> GetHvacLoopSettingBySensorId(string sensorId);
        Task<ResponseModel<List<HvacLoopSettingResponseDTO>>> GetHvacLoopSettingsByBusinessId(string businessId);
        Task<ResponseModel<HvacLoopSettingResponseDTO>> EnableLoop(HvacLoopSensorDTO requestDto);
        Task<ResponseModel<HvacLoopSettingResponseDTO>> DisableLoop(HvacLoopSensorDTO requestDto);
        Task<ResponseModel> DeleteHvacLoopSettingById(string id);
    }
}
