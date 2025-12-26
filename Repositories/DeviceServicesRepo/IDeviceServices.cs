using APIProduct.Models.DTOs.DeviceDTOs;
using P3AHR.Models.DTOs.ResponseDTO;
using System.Threading.Tasks;

namespace APIProduct.Repositories.DeviceServicesRepo
{
    public interface IDeviceServices
    {
        Task<ResponseModel<DeviceResponseDTO>> AddDevice(AddDeviceDTO requestDto);
        Task<ResponseModel<DeviceResponseDTO>> UpdateDevice(UpdateDeviceDTO requestDto);
        Task<ResponseModel<DeviceResponseDTO>> GetDeviceById(string DeviceId);
        Task<ResponseModel<List<DeviceResponseDTO>>> GetAllDevices();
        Task<ResponseModel> DeleteDeviceById(string deviceId);
    }
}
