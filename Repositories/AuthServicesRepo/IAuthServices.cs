using EMO.Extensions;
using EMO.Models.DTOs.AuthDTOs;
using EMO.Models.DTOs.UserDTOs;
using EMO.Models.DTOs.ResponseDTO;
using EMO.Repositories.InnerServicesRepo;

namespace EMO.Repositories.AuthServicesRepo
{
    public interface IAuthServices
    {
        public Task<ResponseModel<UserLoginResponseDTO>> AuthUser(UserLoginDTO requestDto);

        //public  Task<UserResponseDTO> AuthUserForApp(UserAppLoginDTO requestDto);
        public Task<ResponseModel> ForgotPassword(ForgotPasswordDTO requestDto);
    }
}
