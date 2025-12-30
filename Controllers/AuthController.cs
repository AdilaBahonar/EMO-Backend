using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using EMO.Extensions.MiddleWare;
using EMO.Models.DBModels.DBTables;
using EMO.Models.DTOs.AuthDTOs;
using EMO.Models.DTOs.UserDTOs;
using EMO.Models.DTOs.ResponseDTO;
using EMO.Repositories.AuthServicesRepo;
using EMO.Repositories.JWTUtilsRepo;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using EMO.Models.DTOs.LoginDTOs;

namespace EMO.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthServices authService;
        public AuthController(IAuthServices authService) { this.authService = authService; }

        [HttpPost]
        public async Task<ActionResult<ResponseModel<UserResponseDTO>>> Post(UserLoginDTO model)
        {
            if (ModelState.IsValid)
            {
                var Response = authService.AuthUser(model);
                return Ok(await Response);
            }
            else
            {
                var Response = new ResponseModel<UserResponseDTO>()
                {
                    remarks = "Model Not Verified",
                    success = false
                };
                return BadRequest(Response);
            }
        }
        //[HttpPost("UserAuthenticationForApp")]
        //public async Task<ActionResult<ResponseModel<UserResponseDTO>>> UserAuthenticationForApp(UserAppLoginDTO model)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        var Response = authService.AuthUserForApp(model);
        //        return Ok(await Response);
        //    }
        //    else
        //    {
        //        var Response = new ResponseModel<UserResponseDTO>()
        //        {
        //            remarks = "Model Not Verified",
        //            success = false
        //        };
        //        return BadRequest(Response);
        //    }
        //}
        [HttpPost("ForgotPassword")]
        public async Task<ActionResult<ResponseModel>> ForgotPassword(ForgotPasswordDTO model)
        {
            if (ModelState.IsValid)
            {
                var Response = authService.ForgotPassword(model);
                return Ok(await Response);
            }
            else
            {
                var Response = new ResponseModel()
                {
                    remarks = "Model Not Verified",
                    success = false
                };
                return BadRequest(Response);
            }
        }
    }
}
