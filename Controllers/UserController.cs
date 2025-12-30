using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using P3AHR.Extensions;
using P3AHR.Extensions.MiddleWare;
using P3AHR.Models.DTOs.UserDTOs;
using P3AHR.Models.DTOs.ResponseDTO;
using P3AHR.Repositories.UserServicesRepo;
using P3AHR.Repositories.InnerServicesRepo;
using Microsoft.AspNetCore.Authorization;

namespace P3AHR.Controllers
{
    [ApiKey]
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserServices userService;
        private readonly IInnerServices innerServices;
        private readonly OtherServices otherServices;
        public UserController(IUserServices userService, OtherServices otherServices, IInnerServices innerServices)
        {
            this.userService = userService;
            this.innerServices = innerServices;
            this.otherServices = otherServices;
        }
        [HttpPost]
        [AllowAnonymous]
        public async Task<ActionResult<ResponseModel<UserResponseDTO>>> Post(AddUserDTO model)
        {
            if (ModelState.IsValid)
            {
                var Response = userService.AddUser(model);
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
        [HttpPut]
        public async Task<ActionResult<ResponseModel<UserResponseDTO>>> Put(UpdateUserDTO model)
        {
            if (ModelState.IsValid)
            {
                var Response = userService.UpdateUser(model);
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
        [HttpGet("GetById")]
        public async Task<ActionResult<ResponseModel<UserResponseDTO>>> GetById(string id)
        {
            if (otherServices.Check(id))
            {
                var Response = userService.GetUserById(id);
                return Ok(await Response);
            }
            else
            {
                var Response = new ResponseModel<UserResponseDTO>()
                {
                    remarks = "User not found by ID",
                    success = false
                };
                return BadRequest(Response);
            }
        }
        [HttpGet]
        public async Task<ActionResult<ResponseModel<List<UserResponseDTO>>>> Get()
        {
            var users = await userService.GetAllUsers();
            if (users != null)
            {
                var Response = users;
                return Ok(Response);
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
        [HttpGet("GetByUserTypeId")]
        public async Task<ActionResult<ResponseModel<List<UserResponseDTO>>>> GetByUserTypeId(string userTypeId)
        {
            if (otherServices.Check(userTypeId))
            {
                var Response = userService.GetByUserTypeId(userTypeId);
                return Ok(await Response);
            }
            else
            {
                var Response = new ResponseModel<UserResponseDTO>()
                {
                    remarks = "User Not found by this User Type",
                    success = false
                };
                return BadRequest(Response);
            }
        }
        [HttpDelete]
        public async Task<ActionResult<ResponseModel>> Delete(string id)
        {
            if (otherServices.Check(id))
            {
                var Response = userService.DeleteUserById(id);
                return Ok(await Response);
            }
            else
            {
                var Response = new ResponseModel<UserResponseDTO>()
                {
                    remarks = "User not found",
                    success = false
                };
                return BadRequest(Response);
            }
        }
        [HttpGet("GetByOfficialEmail")]
        public async Task<ActionResult<ResponseModel<UserInnerResponseDTO>>> GetByOfficialEmail(string officialEmail)
        {
            if (otherServices.Check(officialEmail))
            {
                var Response = innerServices.GetUserByOfficialEmail(officialEmail);
                return Ok(await Response);
            }
            else
            {
                var Response = new ResponseModel<UserInnerResponseDTO>()
                {
                    remarks = "User not found by Id",
                    success = false
                };
                return BadRequest(Response);
            }
        }
    }
}
