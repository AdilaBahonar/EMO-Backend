using Microsoft.IdentityModel.Tokens;
using P3AHR.Models.DTOs.UserDTOs;
using P3AHR.Models.DTOs.ResponseDTO;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace P3AHR.Repositories.JWTUtilsRepo
{
    public interface IJWTUtils
    {
        public JwtSecurityToken GetToken(List<Claim> authClaims, bool isRememberMeActive);
        public Task<ResponseModel<UserResponseDTO>> ValidateToken(string userToken);
    }
}
