using EMO.Models.DTOs.DeviceDTOs;
using EMO.Models.DTOs.ResponseDTO;
using System.Threading.Tasks;

namespace EMO.Repositories.DeviceServicesRepo
{
    public interface IDeviceServices
    {
        Task<ResponseModel<DeviceResponseDTO>> AddDevice(AddDeviceDTO requestDto);
        Task<ResponseModel<DeviceResponseDTO>> UpdateDevice(UpdateDeviceDTO requestDto);
        Task<ResponseModel<DeviceResponseDTO>> GetDeviceById(string DeviceId);
        Task<ResponseModel<List<DeviceResponseDTO>>> GetAllDevices();
        Task<ResponseModel> DeleteDeviceById(string deviceId);
        public Task<ResponseModel<List<DeviceResponseDTO>>> GetDeviceByBusinessId(string deviceId);
    }
}
