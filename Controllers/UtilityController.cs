using EMO.Models.DTOs.UtilityDTOs;
using EMO.Repositories.UtilityServicesRepo;
using Microsoft.AspNetCore.Mvc;
using EMO.Extensions;
using EMO.Extensions.MiddleWare;
using EMO.Models.DTOs.ResponseDTO;
using EMO.Models.DTOs.UtilityDTOs.EMO.Models.DTOs.UtilityDTOs;
using P3AHR.Extensions.MiddleWare;
using P3AHR.Models.DTOs.ResponseDTO;

namespace EMO.Controllers
{
    [ApiKey]
    [Route("api/[controller]")]
    [ApiController]
    public class UtilityController : ControllerBase
    {
        private readonly IUtilityServices UtilityServices;
        private readonly OtherServices otherServices;

        public UtilityController(IUtilityServices UtilityServices, OtherServices otherServices)
        {
            this.UtilityServices = UtilityServices;
            this.otherServices = otherServices;
        }

        [HttpPost]
        public async Task<ActionResult<ResponseModel<UtilityResponseDTO>>> Post(AddUtilityDTO model)
        {
            if (ModelState.IsValid)
            {
                var Response = UtilityServices.AddUtility(model);
                return Ok(await Response);
            }
            else
            {
                var Response = new ResponseModel<UtilityResponseDTO>()
                {
                    remarks = "Model Not Verified",
                    success = false
                };
                return BadRequest(Response);
            }
        }

        [HttpPut]
        public async Task<ActionResult<ResponseModel<UtilityResponseDTO>>> Put(UpdateUtilityDTO model)
        {
            if (ModelState.IsValid)
            {
                var Response = UtilityServices.UpdateUtility(model);
                return Ok(await Response);
            }
            else
            {
                var Response = new ResponseModel<UtilityResponseDTO>()
                {
                    remarks = "Model Not Verified",
                    success = false
                };
                return BadRequest(Response);
            }
        }

        [HttpGet("GetById")]
        public async Task<ActionResult<ResponseModel<UtilityResponseDTO>>> GetById(string id)
        {
            if (otherServices.Check(id))
            {
                var Response = UtilityServices.GetUtilityById(id);
                return Ok(await Response);
            }
            else
            {
                var Response = new ResponseModel<UtilityResponseDTO>()
                {
                    remarks = "Utility not found by ID",
                    success = false
                };
                return BadRequest(Response);
            }
        }

        [HttpGet]
        public async Task<ActionResult<ResponseModel<List<UtilityResponseDTO>>>> Get()
        {
            var utilities = await UtilityServices.GetAllUtilities();
            if (utilities != null)
            {
                var Response = utilities;
                return Ok(Response);
            }
            else
            {
                var Response = new ResponseModel<UtilityResponseDTO>()
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
                var Response = UtilityServices.DeleteUtilityById(id);
                return Ok(await Response);
            }
            else
            {
                var Response = new ResponseModel<UtilityResponseDTO>()
                {
                    remarks = "Utility not found",
                    success = false
                };
                return BadRequest(Response);
            }
        }
    }
}
