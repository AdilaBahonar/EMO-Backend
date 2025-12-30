using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using EMO.Models.DBModels;
using EMO.Models.DBModels.DBTables;
using EMO.Models.DTOs.UserDTOs;
using EMO.Models.DTOs.ResponseDTO;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace EMO.Repositories.JWTUtilsRepo
{
    public class JWTUtils : IJWTUtils
    {
        private readonly IConfiguration _configuration;
        private readonly DBUserManagementContext db;
        private readonly IMapper _mapper;
        public JWTUtils(IConfiguration configuration, DBUserManagementContext db, IMapper mapper)
        {
            _configuration = configuration;
            this.db = db;
            _mapper = mapper;
        }
        public JwtSecurityToken GetToken(List<Claim> authClaims, bool isRememberMeActive)
        {
            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Key"]));

            var token = new JwtSecurityToken(
                issuer: _configuration["JWT:ValidIssuer"],
                expires: isRememberMeActive ? DateTime.UtcNow.AddHours(29) : DateTime.UtcNow.AddHours(8),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
                );

            return token;
        }

        public async Task<ResponseModel<UserResponseDTO>> ValidateToken(string userToken)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Key"]));

                tokenHandler.ValidateToken(userToken, new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = key,
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                var jwtToken = (JwtSecurityToken)validatedToken;
                var email = jwtToken.Claims.First(x => x.Type == ClaimTypes.Email).Value;

                var employee = await db.tbl_user
                    .Where(x => x.user_official_email == email && x.user_token == userToken)
                    .FirstOrDefaultAsync();

                if (employee != null)
                {
                    return new ResponseModel<UserResponseDTO>
                    {
                        data = _mapper.Map<UserResponseDTO>(employee),
                        success = true
                    };
                }

                return new ResponseModel<UserResponseDTO> { success = false, remarks = "UserNotFound" };
            }
            catch (SecurityTokenExpiredException)
            {
                return new ResponseModel<UserResponseDTO> { success = false, remarks = "TokenExpired" };
            }
            catch (Exception)
            {
                return new ResponseModel<UserResponseDTO> { success = false, remarks = "TokenInvalid" };
            }
        }
        /* public async Task<ResponseModel<UserResponseDTO>> ValidateToken(string userToken)
         {
             var tokenHandler = new JwtSecurityTokenHandler();
             var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Key"]));
             tokenHandler.ValidateToken(userToken, new TokenValidationParameters
             {
                 ValidateIssuerSigningKey = true,
                 IssuerSigningKey = key,
                 ValidateIssuer = false,
                 ValidateAudience = false,
                 //set clockskew to zero so tokens expire exactly at token expiration time(instead of 5 minutes later)
                 ClockSkew = TimeSpan.Zero
             }, out SecurityToken validatedToken);
             var jwtToken = (JwtSecurityToken)validatedToken;
             var email = jwtToken.Claims.First(x => x.Type == ClaimTypes.Email).Value;
             if (email != null)
             {
                 var employee = await db.tbl_user.Where(x => x.user_official_email == email && x.user_token == userToken).FirstOrDefaultAsync();
                 if (employee != null)
                 {

                     return new ResponseModel<UserResponseDTO>()
                     {
                         data = _mapper.Map<UserResponseDTO>(employee),
                         success = true
                     };
                 }
                 else
                 {
                     return new ResponseModel<UserResponseDTO>()
                     {
                         success = false
                     };
                 }
             }
             else
             {
                 return new ResponseModel<UserResponseDTO>() { success = false };
             }
         }*/
    }
}
