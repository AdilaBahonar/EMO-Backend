using EMO.Models.DTOs.FacilityDTOs;
using EMO.Repositories.FacilityServicesRepo;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using EMO.Extensions;
using EMO.Extensions.MiddleWare;
using EMO.Models.DTOs.ResponseDTO;

namespace EMO.Controllers
{
    [ApiKey]
    [Route("api/[controller]")]
    [ApiController]
    public class FacilityController : ControllerBase
    {
        private readonly IFacilityServices FacilityServices;
        private readonly OtherServices otherServices;

        public FacilityController(IFacilityServices FacilityServices, OtherServices otherServices)
        {
            this.FacilityServices = FacilityServices;
            this.otherServices = otherServices;
        }

        [HttpPost]
        public async Task<ActionResult<ResponseModel<FacilityResponseDTO>>> Post(AddFacilityDTO model)
        {
            if (ModelState.IsValid)
            {
                var Response = FacilityServices.AddFacility(model);
                return Ok(await Response);
            }
            else
            {
                var Response = new ResponseModel<FacilityResponseDTO>()
                {
                    remarks = "Model Not Verified",
                    success = false
                };
                return BadRequest(Response);
            }
        }

        [HttpPut]
        public async Task<ActionResult<ResponseModel<FacilityResponseDTO>>> Put(UpdateFacilityDTO model)
        {
            if (ModelState.IsValid)
            {
                var Response = FacilityServices.UpdateFacility(model);
                return Ok(await Response);
            }
            else
            {
                var Response = new ResponseModel<FacilityResponseDTO>()
                {
                    remarks = "Model Not Verified",
                    success = false
                };
                return BadRequest(Response);
            }
        }

        [HttpGet("GetById")]
        public async Task<ActionResult<ResponseModel<FacilityResponseDTO>>> GetById(string id)
        {
            if (otherServices.Check(id))
            {
                var Response = FacilityServices.GetFacilityById(id);
                return Ok(await Response);
            }
            else
            {
                var Response = new ResponseModel<FacilityResponseDTO>()
                {
                    remarks = "Facility not found by ID",
                    success = false
                };
                return BadRequest(Response);
            }
        }

        [HttpGet]
        public async Task<ActionResult<ResponseModel<List<FacilityResponseDTO>>>> Get()
        {
            var Facilities = await FacilityServices.GetAllFacilities();
            if (Facilities != null)
            {
                var Response = Facilities;
                return Ok(Response);
            }
            else
            {
                var Response = new ResponseModel<FacilityResponseDTO>()
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
                var Response = FacilityServices.DeleteFacilityById(id);
                return Ok(await Response);
            }
            else
            {
                var Response = new ResponseModel<FacilityResponseDTO>()
                {
                    remarks = "Facility not found",
                    success = false
                };
                return BadRequest(Response);
            }
        }
    }

}
