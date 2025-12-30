using Microsoft.IdentityModel.Tokens;
using EMO.Models.DTOs.UserDTOs;
using EMO.Models.DTOs.ResponseDTO;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace EMO.Repositories.JWTUtilsRepo
{
    public interface IJWTUtils
    {
        public JwtSecurityToken GetToken(List<Claim> authClaims, bool isRememberMeActive);
        public Task<ResponseModel<UserResponseDTO>> ValidateToken(string userToken);
    }
}
