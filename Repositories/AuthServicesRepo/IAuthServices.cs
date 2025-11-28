using P3AHR.Extensions;
using P3AHR.Models.DTOs.AuthDTOs;
using P3AHR.Models.DTOs.UserDTOs;
using P3AHR.Models.DTOs.ResponseDTO;
using P3AHR.Repositories.InnerServicesRepo;

namespace P3AHR.Repositories.AuthServicesRepo
{
    public interface IAuthServices
    {
        public Task<ResponseModel<UserLoginResponseDTO>> AuthUser(UserLoginDTO requestDto);

        //public  Task<UserResponseDTO> AuthUserForApp(UserAppLoginDTO requestDto);
        public Task<ResponseModel> ForgotPassword(ForgotPasswordDTO requestDto);
    }
}
