using EMO.Models.DTOs.BuildingDTOs;
using EMO.Repositories.BuildingServicesRepo;
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
    public class BuildingController : ControllerBase
    {
        private readonly IBuildingServices BuildingServices;
        private readonly OtherServices otherServices;

        public BuildingController(IBuildingServices BuildingServices, OtherServices otherServices)
        {
            this.BuildingServices = BuildingServices;
            this.otherServices = otherServices;
        }

        [HttpPost]
        public async Task<ActionResult<ResponseModel<BuildingResponseDTO>>> Post(AddBuildingDTO model)
        {
            if (ModelState.IsValid)
            {
                var Response = BuildingServices.AddBuilding(model);
                return Ok(await Response);
            }
            else
            {
                var Response = new ResponseModel<BuildingResponseDTO>()
                {
                    remarks = "Model Not Verified",
                    success = false
                };
                return BadRequest(Response);
            }
        }

        [HttpPut]
        public async Task<ActionResult<ResponseModel<BuildingResponseDTO>>> Put(UpdateBuildingDTO model)
        {
            if (ModelState.IsValid)
            {
                var Response = BuildingServices.UpdateBuilding(model);
                return Ok(await Response);
            }
            else
            {
                var Response = new ResponseModel<BuildingResponseDTO>()
                {
                    remarks = "Model Not Verified",
                    success = false
                };
                return BadRequest(Response);
            }
        }

        [HttpGet("GetById")]
        public async Task<ActionResult<ResponseModel<BuildingResponseDTO>>> GetById(string id)
        {
            if (otherServices.Check(id))
            {
                var Response = BuildingServices.GetBuildingById(id);
                return Ok(await Response);
            }
            else
            {
                var Response = new ResponseModel<BuildingResponseDTO>()
                {
                    remarks = "Building not found by ID",
                    success = false
                };
                return BadRequest(Response);
            }
        }

        [HttpGet]
        public async Task<ActionResult<ResponseModel<List<BuildingResponseDTO>>>> Get()
        {
            var Buildings = await BuildingServices.GetAllBuildings();
            if (Buildings != null)
            {
                var Response = Buildings;
                return Ok(Response);
            }
            else
            {
                var Response = new ResponseModel<BuildingResponseDTO>()
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
                var Response = BuildingServices.DeleteBuildingById(id);
                return Ok(await Response);
            }
            else
            {
                var Response = new ResponseModel<BuildingResponseDTO>()
                {
                    remarks = "Building not found",
                    success = false
                };
                return BadRequest(Response);
            }
        }
    }

}
