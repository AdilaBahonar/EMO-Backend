using EMO.Models.DTOs.SubTypeDTOs;
using EMO.Repositories.SubTypeServicesRepo;
using Microsoft.AspNetCore.Mvc;
using EMO.Extensions;
using EMO.Extensions.MiddleWare;
using EMO.Models.DTOs.ResponseDTO;
using EMO.Models.DTOs.SubTypeDTOs.EMO.Models.DTOs.SubTypeDTOs;
using P3AHR.Extensions.MiddleWare;
using P3AHR.Models.DTOs.ResponseDTO;

namespace EMO.Controllers
{
    [ApiKey]
    [Route("api/[controller]")]
    [ApiController]
    public class SubTypeController : ControllerBase
    {
        private readonly ISubTypeServices SubTypeServices;
        private readonly OtherServices otherServices;

        public SubTypeController(ISubTypeServices SubTypeServices, OtherServices otherServices)
        {
            this.SubTypeServices = SubTypeServices;
            this.otherServices = otherServices;
        }

        [HttpPost]
        public async Task<ActionResult<ResponseModel<SubTypeResponseDTO>>> Post(AddSubTypeDTO model)
        {
            if (ModelState.IsValid)
            {
                var Response = SubTypeServices.AddSubType(model);
                return Ok(await Response);
            }
            else
            {
                var Response = new ResponseModel<SubTypeResponseDTO>()
                {
                    remarks = "Model Not Verified",
                    success = false
                };
                return BadRequest(Response);
            }
        }

        [HttpPut]
        public async Task<ActionResult<ResponseModel<SubTypeResponseDTO>>> Put(UpdateSubTypeDTO model)
        {
            if (ModelState.IsValid)
            {
                var Response = SubTypeServices.UpdateSubType(model);
                return Ok(await Response);
            }
            else
            {
                var Response = new ResponseModel<SubTypeResponseDTO>()
                {
                    remarks = "Model Not Verified",
                    success = false
                };
                return BadRequest(Response);
            }
        }

        [HttpGet("GetById")]
        public async Task<ActionResult<ResponseModel<SubTypeResponseDTO>>> GetById(string id)
        {
            if (otherServices.Check(id))
            {
                var Response = SubTypeServices.GetSubTypeById(id);
                return Ok(await Response);
            }
            else
            {
                var Response = new ResponseModel<SubTypeResponseDTO>()
                {
                    remarks = "Sub Type not found by ID",
                    success = false
                };
                return BadRequest(Response);
            }
        }

        [HttpGet]
        public async Task<ActionResult<ResponseModel<List<SubTypeResponseDTO>>>> Get()
        {
            var subTypes = await SubTypeServices.GetAllSubTypes();
            if (subTypes != null)
            {
                var Response = subTypes;
                return Ok(Response);
            }
            else
            {
                var Response = new ResponseModel<SubTypeResponseDTO>()
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
                var Response = SubTypeServices.DeleteSubTypeById(id);
                return Ok(await Response);
            }
            else
            {
                var Response = new ResponseModel<SubTypeResponseDTO>()
                {
                    remarks = "Sub Type not found",
                    success = false
                };
                return BadRequest(Response);
            }
        }
    }
}
