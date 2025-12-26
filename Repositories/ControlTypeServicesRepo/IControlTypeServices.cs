using APIProduct.Models.DTOs.BusinessDTOs;
using APIProduct.Models.DTOs.ControlTypeDTOs;
using P3AHR.Models.DTOs.ResponseDTO;

namespace APIProduct.Repositories.BusinessServicesRepo
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
