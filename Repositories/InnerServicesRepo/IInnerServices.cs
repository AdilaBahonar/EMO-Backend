using EMO.Models.DTOs.UserDTOs;
using EMO.Models.DTOs.ResponseDTO;

namespace EMO.Repositories.InnerServicesRepo
{
    public interface IInnerServices
    {
        public Task<ResponseModel<UserInnerResponseDTO>> GetUserByOfficialEmail(string userId);
        public Task<ResponseModel<UserInnerResponseDTO>> GetUserByPhoneNo(string phoneNo);
        public Task<ResponseModel<UserResponseDTO>> UpdateUser(UpdateInnerUserDTO requestDto);
    }
}
