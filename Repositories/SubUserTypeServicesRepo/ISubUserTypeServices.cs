using EMO.Models.DTOs.ResponseDTO;
using EMO.Models.DTOs.SubUserTypeDTOs;

namespace EMO.Repositories.SubUserTypeServicesRepo
{
    public interface ISubUserTypeServices
    {
        Task<ResponseModel<SubUserTypeResponseDTO>> AddSubUserType(AddSubUserTypeDTO requestDto);
        Task<ResponseModel<SubUserTypeResponseDTO>> UpdateSubUserType(UpdateSubUserTypeDTO requestDto);
        Task<ResponseModel<SubUserTypeResponseDTO>> GetSubUserTypeById(string subUserTypeId);
        Task<ResponseModel<List<SubUserTypeResponseDTO>>> GetAllSubUserTypes();
        Task<ResponseModel> DeleteSubUserTypeById(string subUserTypeId);
        Task<ResponseModel<List<SubUserTypeResponseDTO>>> GetSubUserTypesByUserId(string userId);
        public Task<ResponseModel<List<SubUserTypeResponseDTO>>> GetSubUserTypesOfBusiness(string userId);
        Task<ResponseModel> UpdateSubUserTypeHierarchy(List<SubUserTypeHierarchyDTO> requestDto);
        public Task<ResponseModel<List<SubUserTypeResponseDTO>>> GetActiveSubUserTypesByUserId(string userId);

    }
}
