using EMO.Models.DTOs.BusinessDTOs;
using EMO.Models.DTOs.ControlTypeDTOs;
using EMO.Models.DTOs.ResponseDTO;

namespace EMO.Repositories.BusinessServicesRepo
{
    public interface IControlTypeServices
    {
         Task<ResponseModel<ControlTypeResponseDTO>> AddControlType(AddControlTypeDTO requestDto);
         Task<ResponseModel<ControlTypeResponseDTO>> UpdateControlType(UpdateControlTypeDTO requestDto);
         Task<ResponseModel<ControlTypeResponseDTO>> GetControlTypeById(string controltypeId);
         Task<ResponseModel<List<ControlTypeResponseDTO>>> GetAllControlTypes();
         Task<ResponseModel> DeleteControlTypeById(string controltypeId);
    }
}
