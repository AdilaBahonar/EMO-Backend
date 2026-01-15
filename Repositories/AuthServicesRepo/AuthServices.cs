using AutoMapper;
using Microsoft.EntityFrameworkCore;
using EMO.Extensions;
using EMO.Models.DBModels;
using EMO.Models.DBModels.DBTables;
using EMO.Models.DTOs.AuthDTOs;
using EMO.Models.DTOs.UserDTOs;
using EMO.Models.DTOs.ResponseDTO;
using EMO.Repositories.UserServicesRepo;
using EMO.Repositories.InnerServicesRepo;
using EMO.Repositories.JWTUtilsRepo;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace EMO.Repositories.AuthServicesRepo
{
    public class AuthServices: IAuthServices
    {
        private readonly DBUserManagementContext db;
        private readonly IMapper mapper;
        private readonly OtherServices otherServices;
        private readonly IJWTUtils jwtUtils;
        private readonly IInnerServices innerServices;
        public AuthServices(DBUserManagementContext db, IMapper mapper, OtherServices otherServices, IJWTUtils jwtUtils, IInnerServices innerServices)
        {
            this.db = db;
            this.mapper = mapper;
            this.otherServices = otherServices;
            this.jwtUtils= jwtUtils;
            this.innerServices = innerServices;
        }
        // correct this logic
        public async Task<ResponseModel<UserLoginResponseDTO>> AuthUser(UserLoginDTO requestDto)
        {
            try
            {
                var existingUser = await innerServices.GetUserByOfficialEmail(requestDto.userName) ;
                if (existingUser.success )
                {
                    if (existingUser.data.userPassword == otherServices.encodePassword(requestDto.password))
                    {
                        var response = await Login(existingUser.data, requestDto.isRememberMe);
                        return new ResponseModel<UserLoginResponseDTO>
                        {
                            data = response.data,
                            remarks = "Success",
                            success = true,
                        };
                    }
                    else
                    {

                        return new ResponseModel<UserLoginResponseDTO>
                        {
                            remarks = "Password is wrong",
                            success = false,
                        };

                    }
                }
                else
                {
                    // Authentication failed
                    return new ResponseModel<UserLoginResponseDTO>
                    {
                        remarks = "User not found",
                        success = false,
                    };
                }
            }
            catch (Exception ex)
            {
                // Log the exception
                return new ResponseModel<UserLoginResponseDTO>
                {
                    remarks = $"There was a fatal error {ex.ToString()}",
                    success = false,
                };
            }
        }
        //public async Task<UserResponseDTO> AuthUserForApp(UserAppLoginDTO requestDto)
        //{
        //    try
        //    {
        //        var existingUser = await innerServices.GetUserByPhoneNo(requestDto.userPhoneNo);
        //        if (existingUser.success)
        //        {
        //            if (existingUser.data.userPassword == otherServices.encodePassword(requestDto.password))
        //            {
        //                var response = await Login(existingUser.data, false);
        //                // Authentication successful
        //                return new UserResponseDTO
        //                {
        //                    userEmail= response.data.userOfficialEmail,
        //                    userName= response.data.userName,
        //                    userPhone= requestDto.userPhoneNo,
        //                    userId=response.data.userId,
        //                };
        //            }
        //            else
        //            {
        //                // Authentication failed

        //                return new UserResponseDTO
        //                {
                           
        //                };

        //            }
        //        }
        //        else
        //        {
        //            // Authentication failed
        //            return new UserResponseDTO
        //            {
        //                remarks = "User not found",
        //                resultCode = 1200,
        //            };
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        // Log the exception
        //        return  new UserResponseDTO
        //        {
        //            remarks = $"There was a fatal error {ex.ToString()}",
        //          resultCode=1000
        //        };
        //    }
        //}
        public async Task<ResponseModel> ForgotPassword(ForgotPasswordDTO requestDto)
        {
            try
            {
                var existingUser = await db.tbl_user.Where(u => u.user_name.ToLower() == requestDto.userName.ToLower()).FirstOrDefaultAsync();
                if (existingUser != null)
                {
                  //  Generate a password reset token and send a reset email
                    // ... (code to generate token and send email)

                    return (new ResponseModel
                    {
                        success = true,
                        remarks = "Password reset email sent"
                    });
                }
                else
                {
              //      User not found
                    return (new ResponseModel
                    {
                        success = false,
                        remarks = "Email is wrong"
                    });
                }
            }
            catch (Exception ex)
            {
                return new ResponseModel
                {
                    remarks = $"There was a fatal error {ex.ToString()}",
                    success = false,
                };
            }
        }
        private async Task<ResponseModel<UserLoginResponseDTO>> Login(UserInnerResponseDTO user, bool isRememberMe)
        {
            var authClaims = new List<Claim>
                        {
                            new Claim(ClaimTypes.Email, user.userName),
                            new Claim(JwtRegisteredClaimNames.Jti,Guid.NewGuid().ToString())
                        };
            var token = jwtUtils.GetToken(authClaims, isRememberMe);
            user.userToken = new JwtSecurityTokenHandler().WriteToken(token);
            user.userPassword = "";
            await innerServices.UpdateUser(mapper.Map<UpdateInnerUserDTO>(user));
            return new ResponseModel<UserLoginResponseDTO>()
            {
                data = mapper.Map<UserLoginResponseDTO>(user),
                success = true,
                remarks = $"Success"
            };
        }
    }
}
