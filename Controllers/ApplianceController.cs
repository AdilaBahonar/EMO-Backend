using EMO.Extensions;
using EMO.Extensions.MiddleWare;
using EMO.Models.DTOs.ApplianceDTOs;
using EMO.Models.DTOs.ResponseDTO;
using EMO.Repositories.ApplianceServicesRepo;
using Microsoft.AspNetCore.Mvc;

namespace EMO.Controllers
{
    [ApiKey]
    [Route("api/[controller]")]
    [ApiController]
    public class ApplianceController : ControllerBase
    {
        private readonly IApplianceServices applianceServices;
        private readonly OtherServices otherServices;

        public ApplianceController(IApplianceServices applianceServices, OtherServices otherServices)
        {
            this.applianceServices = applianceServices;
            this.otherServices = otherServices;
        }

        [HttpPost]
        public async Task<ActionResult<ResponseModel<ApplianceResponseDTO>>> Post(AddApplianceDTO model)
        {
            if (ModelState.IsValid)
            {
                var Response = applianceServices.AddAppliance(model);
                return Ok(await Response);
            }
            else
            {
                var Response = new ResponseModel<ApplianceResponseDTO>()
                {
                    remarks = "Model Not Verified",
                    success = false
                };
                return BadRequest(Response);
            }
        }

        [HttpPut]
        public async Task<ActionResult<ResponseModel<ApplianceResponseDTO>>> Put(UpdateApplianceDTO model)
        {
            if (ModelState.IsValid)
            {
                var Response = applianceServices.UpdateAppliance(model);
                return Ok(await Response);
            }
            else
            {
                var Response = new ResponseModel<ApplianceResponseDTO>()
                {
                    remarks = "Model Not Verified",
                    success = false
                };
                return BadRequest(Response);
            }
        }

        [HttpPost("business")]
        public async Task<ActionResult<ResponseModel<ApplianceResponseDTO>>> PostBusinessAppliance(AddApplianceDTO model)
        {
            if (!otherServices.Check(model.fkBusiness))
            {
                var Response = new ResponseModel<ApplianceResponseDTO>()
                {
                    remarks = "Invalid business id.",
                    success = false
                };
                return BadRequest(Response);
            }

            if (ModelState.IsValid)
            {
                var Response = applianceServices.AddBusinessAppliance(model);
                return Ok(await Response);
            }
            else
            {
                var Response = new ResponseModel<ApplianceResponseDTO>()
                {
                    remarks = "Model Not Verified",
                    success = false
                };
                return BadRequest(Response);
            }
        }

        [HttpPut("business")]
        public async Task<ActionResult<ResponseModel<ApplianceResponseDTO>>> PutBusinessAppliance(UpdateApplianceDTO model)
        {
            if (ModelState.IsValid)
            {
                var Response = applianceServices.UpdateBusinessAppliance(model);
                return Ok(await Response);
            }
            else
            {
                var Response = new ResponseModel<ApplianceResponseDTO>()
                {
                    remarks = "Model Not Verified",
                    success = false
                };
                return BadRequest(Response);
            }
        }

        [HttpGet]
        public async Task<ActionResult<ResponseModel<List<ApplianceResponseDTO>>>> Get()
        {
            var Response = applianceServices.GetAllAppliances();
            return Ok(await Response);
        }

        [HttpGet("GetById")]
        public async Task<ActionResult<ResponseModel<ApplianceResponseDTO>>> GetById(string id)
        {
            if (otherServices.Check(id))
            {
                var Response = applianceServices.GetApplianceById(id);
                return Ok(await Response);
            }
            else
            {
                var Response = new ResponseModel<ApplianceResponseDTO>()
                {
                    remarks = "Invalid id.",
                    success = false
                };
                return BadRequest(Response);
            }
        }

        [HttpGet("GetByUtilityId")]
        public async Task<ActionResult<ResponseModel<List<ApplianceResponseDTO>>>> GetByUtilityId(string id)
        {
            if (otherServices.Check(id))
            {
                var Response = applianceServices.GetAppliancesByUtilityId(id);
                return Ok(await Response);
            }
            else
            {
                var Response = new ResponseModel<List<ApplianceResponseDTO>>()
                {
                    remarks = "Invalid utility id.",
                    success = false
                };
                return BadRequest(Response);
            }
        }

        [HttpGet("business")]
        public async Task<ActionResult<ResponseModel<List<ApplianceResponseDTO>>>> GetBusinessAppliances(string businessId)
        {
            if (otherServices.Check(businessId))
            {
                var Response = applianceServices.GetBusinessAppliances(businessId);
                return Ok(await Response);
            }
            else
            {
                var Response = new ResponseModel<List<ApplianceResponseDTO>>()
                {
                    remarks = "Invalid business id.",
                    success = false
                };
                return BadRequest(Response);
            }
        }

        [HttpGet("business/GetById")]
        public async Task<ActionResult<ResponseModel<ApplianceResponseDTO>>> GetBusinessApplianceById(string id)
        {
            if (otherServices.Check(id))
            {
                var Response = applianceServices.GetBusinessApplianceById(id);
                return Ok(await Response);
            }
            else
            {
                var Response = new ResponseModel<ApplianceResponseDTO>()
                {
                    remarks = "Invalid business appliance id.",
                    success = false
                };
                return BadRequest(Response);
            }
        }

        [HttpGet("business/GetByUtilityId")]
        public async Task<ActionResult<ResponseModel<List<ApplianceResponseDTO>>>> GetBusinessAppliancesByUtilityId(string businessId, string utilityId)
        {
            if (otherServices.Check(businessId) && otherServices.Check(utilityId))
            {
                var Response = applianceServices.GetBusinessAppliancesByUtilityId(businessId, utilityId);
                return Ok(await Response);
            }
            else
            {
                var Response = new ResponseModel<List<ApplianceResponseDTO>>()
                {
                    remarks = "Invalid business or utility id.",
                    success = false
                };
                return BadRequest(Response);
            }
        }

        [HttpGet("SeedDefault")]
        public async Task<ActionResult<ResponseModel<List<ApplianceResponseDTO>>>> SeedDefault()
        {
            var Response = applianceServices.SeedDefaultAppliances();
            return Ok(await Response);
        }

        [HttpGet("business/SeedDefault")]
        public async Task<ActionResult<ResponseModel<List<ApplianceResponseDTO>>>> SeedBusinessDefault(string businessId)
        {
            if (otherServices.Check(businessId))
            {
                var Response = applianceServices.SeedBusinessDefaultAppliances(businessId);
                return Ok(await Response);
            }
            else
            {
                var Response = new ResponseModel<List<ApplianceResponseDTO>>()
                {
                    remarks = "Invalid business id.",
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
                var Response = applianceServices.DeleteApplianceById(id);
                return Ok(await Response);
            }
            else
            {
                var Response = new ResponseModel()
                {
                    remarks = "Invalid id.",
                    success = false
                };
                return BadRequest(Response);
            }
        }

        [HttpDelete("business")]
        public async Task<ActionResult<ResponseModel>> DeleteBusinessAppliance(string id)
        {
            if (otherServices.Check(id))
            {
                var Response = applianceServices.DeleteBusinessApplianceById(id);
                return Ok(await Response);
            }
            else
            {
                var Response = new ResponseModel()
                {
                    remarks = "Invalid business appliance id.",
                    success = false
                };
                return BadRequest(Response);
            }
        }
    }
}
