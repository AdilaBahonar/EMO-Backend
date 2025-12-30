using APIProduct.Models.DTOs.FloorDTOs;
using APIProduct.Repositories.FloorServicesRepo;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using P3AHR.Extensions;
using P3AHR.Extensions.MiddleWare;
using P3AHR.Models.DTOs.ResponseDTO;

namespace APIProduct.Controllers
{
    [ApiKey]
    [Route("api/[controller]")]
    [ApiController]
    public class FloorController : ControllerBase
    {
        private readonly IFloorServices FloorServices;
        private readonly OtherServices otherServices;

        public FloorController(IFloorServices FloorServices, OtherServices otherServices)
        {
            this.FloorServices = FloorServices;
            this.otherServices = otherServices;
        }

        [HttpPost("AddFloor")]

        public async Task<ActionResult<ResponseModel<FloorResponseDTO>>> Post(AddFloorDTO model)
        {
            if (ModelState.IsValid)
            {
                var Response = FloorServices.AddFloor(model);
                return Ok(await Response);
            }
            else
            {
                var Response = new ResponseModel<FloorResponseDTO>()
                {
                    remarks = "Model Not Verified",
                    success = false
                };
                return BadRequest(Response);
            }
        }

        [HttpPut]
        public async Task<ActionResult<ResponseModel<FloorResponseDTO>>> Put(UpdateFloorDTO model)
        {
            if (ModelState.IsValid)
            {
                var Response = FloorServices.UpdateFloor(model);
                return Ok(await Response);
            }
            else
            {
                var Response = new ResponseModel<FloorResponseDTO>()
                {
                    remarks = "Model Not Verified",
                    success = false
                };
                return BadRequest(Response);
            }
        }

        [HttpGet("GetById")]
        public async Task<ActionResult<ResponseModel<FloorResponseDTO>>> GetById(string id)
        {
            if (otherServices.Check(id))
            {
                var Response = FloorServices.GetFloorById(id);
                return Ok(await Response);
            }
            else
            {
                var Response = new ResponseModel<FloorResponseDTO>()
                {
                    remarks = "Floor not found by ID",
                    success = false
                };
                return BadRequest(Response);
            }
        }

        [HttpGet]
        public async Task<ActionResult<ResponseModel<List<FloorResponseDTO>>>> Get()
        {
            var Floors = await FloorServices.GetAllFloors();
            if (Floors != null)
            {
                var Response = Floors;
                return Ok(Response);
            }
            else
            {
                var Response = new ResponseModel<FloorResponseDTO>()
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
                var Response = FloorServices.DeleteFloorById(id);
                return Ok(await Response);
            }
            else
            {
                var Response = new ResponseModel<FloorResponseDTO>()
                {
                    remarks = "Floor not found",
                    success = false
                };
                return BadRequest(Response);
            }
        }
    }

}
