using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using P3AHR.Extensions.MiddleWare;
using P3AHR.Extensions;
using P3AHR.Models.DTOs.ResponseDTO;
using P3AHR.Repositories.InnerServicesRepo;
using APIProduct.Repositories.UserTypeServicesRepo;
using APIProduct.Models.DTOs.UserTypeDTOs;

namespace APIProduct.Controllers
{
    [ApiKey]
    [Route("api/[controller]")]
    [ApiController]
    public class UserTypeController : ControllerBase
    {
        private readonly IUserTypeService UserTypeService;
        private readonly OtherServices otherServices;
        public UserTypeController(IUserTypeService UserTypeService, OtherServices otherServices)
        {
            this.UserTypeService = UserTypeService;
            this.otherServices = otherServices;
        }
        [HttpPost]
        public async Task<ActionResult<ResponseModel<UserTypeResponseDTO>>> Post(AddUserTypeDTO model)
        {
            if (ModelState.IsValid)
            {
                var Response = UserTypeService.AddUserType(model);
                return Ok(await Response);
            }
            else
            {
                var Response = new ResponseModel<UserTypeResponseDTO>()
                {
                    remarks = "Model Not Verified",
                    success = false
                };
                return BadRequest(Response);
            }
        }
        [HttpPut]
        public async Task<ActionResult<ResponseModel<UserTypeResponseDTO>>> Put(UpdateUserTypeDTO model)
        {
            if (ModelState.IsValid)
            {
                var Response = UserTypeService.UpdateUserType(model);
                return Ok(await Response);
            }
            else
            {
                var Response = new ResponseModel<UserTypeResponseDTO>()
                {
                    remarks = "Model Not Verified",
                    success = false
                };
                return BadRequest(Response);
            }
        }
        [HttpGet("GetById")]
        public async Task<ActionResult<ResponseModel<UserTypeResponseDTO>>> GetById(string id)
        {
            if (otherServices.Check(id))
            {
                var Response = UserTypeService.GetUserTypeById(id);
                return Ok(await Response);
            }
            else
            {
                var Response = new ResponseModel<UserTypeResponseDTO>()
                {
                    remarks = "UserType not found by ID",
                    success = false
                };
                return BadRequest(Response);
            }
        }
        [HttpGet]
        public async Task<ActionResult<ResponseModel<List<UserTypeResponseDTO>>>> Get()
        {
            var UserTypes = await UserTypeService.GetAllUserTypes();
            if (UserTypes != null)
            {
                var Response = UserTypes;
                return Ok(Response);
            }
            else
            {
                var Response = new ResponseModel<UserTypeResponseDTO>()
                {
                    remarks = "Model Not Verified",
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
                var Response = UserTypeService.DeleteUserTypeById(id);
                return Ok(await Response);
            }
            else
            {
                var Response = new ResponseModel<UserTypeResponseDTO>()
                {
                    remarks = "UserType not found",
                    success = false
                };
                return BadRequest(Response);
            }
        }
    }
}
