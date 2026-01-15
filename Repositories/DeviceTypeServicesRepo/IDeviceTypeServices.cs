
using EMO.Models.DTOs.DeviceTypeDTOs;
using EMO.Models.DTOs.ResponseDTO;
using System.Threading.Tasks;

namespace EMO.Repositories.DeviceTypeServicesRepo
{
    public interface IDeviceTypeServices
    {
        Task<ResponseModel<DeviceTypeResponseDTO>> AddDeviceType(AddDeviceTypeDTO requestDto);
        Task<ResponseModel<DeviceTypeResponseDTO>> UpdateDeviceType(UpdateDeviceTypeDTO requestDto);
        Task<ResponseModel<DeviceTypeResponseDTO>> GetDeviceTypeById(string DeviceTypeId);
        Task<ResponseModel<List<DeviceTypeResponseDTO>>> GetAllDeviceTypes();
        Task<ResponseModel> DeleteDeviceTypeById(string DeviceTypeId);

    }
}
