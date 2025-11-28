using P3AHR.Models.DTOs.UserDTOs;
using P3AHR.Models.DTOs.ResponseDTO;

namespace P3AHR.Repositories.InnerServicesRepo
{
    public interface IInnerServices
    {
        public Task<ResponseModel<UserInnerResponseDTO>> GetUserByOfficialEmail(string userId);
        public Task<ResponseModel<UserInnerResponseDTO>> GetUserByPhoneNo(string phoneNo);
        public Task<ResponseModel<UserResponseDTO>> UpdateUser(UpdateInnerUserDTO requestDto);
    }
}
