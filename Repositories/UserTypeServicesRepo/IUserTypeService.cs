using EMO.Models.DTOs.UserTypeDTOs;
using EMO.Models.DTOs.ResponseDTO;

namespace EMO.Repositories.UserTypeServicesRepo
{
    public interface IUserTypeService
    {
        public Task<ResponseModel<UserTypeResponseDTO>> AddUserType(AddUserTypeDTO requestDto);
        public Task<ResponseModel<UserTypeResponseDTO>> UpdateUserType(UpdateUserTypeDTO requestDto);
        public Task<ResponseModel<UserTypeResponseDTO>> GetUserTypeById(string UserTypeId);
        public Task<ResponseModel<List<UserTypeResponseDTO>>> GetAllUserTypes();
        public Task<ResponseModel> DeleteUserTypeById(string UserTypeId);
        public Task<ResponseModel<List<UserTypeResponseDTO>>> GetUserTypeByUserId(string UserId);
        public Task<ResponseModel> UpdateUserTypeHierarchy(List<UserTypeHierarchyDTO> requestDto);
        public Task<ResponseModel<List<UserTypeResponseDTO>>> GetActiveUserTypes();
    }
}
