using APIProduct.Models.DTOs.UserTypeDTOs;
using P3AHR.Models.DTOs.ResponseDTO;

namespace APIProduct.Repositories.UserTypeServicesRepo
{
    public interface IUserTypeService
    {
        public Task<ResponseModel<UserTypeResponseDTO>> AddUserType(AddUserTypeDTO requestDto);
        public Task<ResponseModel<UserTypeResponseDTO>> UpdateUserType(UpdateUserTypeDTO requestDto);
        public Task<ResponseModel<UserTypeResponseDTO>> GetUserTypeById(string UserTypeId);
        public Task<ResponseModel<List<UserTypeResponseDTO>>> GetAllUserTypes();
        public Task<ResponseModel> DeleteUserTypeById(string UserTypeId);
    }
}
