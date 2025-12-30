using APIProduct.Models.DTOs.DeviceTypeDTOs;
using P3AHR.Models.DTOs.ResponseDTO;
using System.Threading.Tasks;

namespace APIProduct.Repositories.DeviceTypeServicesRepo
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
