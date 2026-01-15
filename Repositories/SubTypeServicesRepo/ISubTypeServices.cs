using EMO.Models.DTOs.SubTypeDTOs.EMO.Models.DTOs.SubTypeDTOs;
using P3AHR.Models.DTOs.ResponseDTO;

namespace EMO.Repositories.SubTypeServicesRepo
{
    public interface ISubTypeServices
    {
        Task<ResponseModel<SubTypeResponseDTO>> AddSubType(AddSubTypeDTO requestDto);
        Task<ResponseModel<SubTypeResponseDTO>> UpdateSubType(UpdateSubTypeDTO requestDto);
        Task<ResponseModel<SubTypeResponseDTO>> GetSubTypeById(string subTypeId);
        Task<ResponseModel<List<SubTypeResponseDTO>>> GetAllSubTypes();
        Task<ResponseModel> DeleteSubTypeById(string subTypeId);
    }
}
