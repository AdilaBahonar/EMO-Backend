using EMO.Models.DTOs.SensorDTOs;
using EMO.Repositories.SensorServicesRepo;
using Microsoft.AspNetCore.Mvc;
using EMO.Extensions;
using EMO.Extensions.MiddleWare;
using EMO.Models.DTOs.ResponseDTO;

namespace EMO.Controllers
{
    [ApiKey]
    [Route("api/[controller]")]
    [ApiController]
    public class SensorController : ControllerBase
    {
        private readonly ISensorServices SensorServices;
        private readonly OtherServices otherServices;

        public SensorController(ISensorServices SensorServices, OtherServices otherServices)
        {
            this.SensorServices = SensorServices;
            this.otherServices = otherServices;
        }

        [HttpPost]
        public async Task<ActionResult<ResponseModel<SensorResponseDTO>>> Post(AddSensorDTO model)
        {
            if (ModelState.IsValid)
            {
                var Response = SensorServices.AddSensor(model);
                return Ok(await Response);
            }
            else
            {
                var Response = new ResponseModel<SensorResponseDTO>()
                {
                    remarks = "Model Not Verified",
                    success = false
                };
                return BadRequest(Response);
            }
        }

        [HttpPut]
        public async Task<ActionResult<ResponseModel<SensorResponseDTO>>> Put(UpdateSensorDTO model)
        {
            if (ModelState.IsValid)
            {
                var Response = SensorServices.UpdateSensor(model);
                return Ok(await Response);
            }
            else
            {
                var Response = new ResponseModel<SensorResponseDTO>()
                {
                    remarks = "Model Not Verified",
                    success = false
                };
                return BadRequest(Response);
            }
        }

        [HttpGet("GetById")]
        public async Task<ActionResult<ResponseModel<SensorResponseDTO>>> GetById(string id)
        {
            if (otherServices.Check(id))
            {
                var Response = SensorServices.GetSensorById(id);
                return Ok(await Response);
            }
            else
            {
                var Response = new ResponseModel<SensorResponseDTO>()
                {
                    remarks = "Sensor not found by ID",
                    success = false
                };
                return BadRequest(Response);
            }
        }

        [HttpGet]
        public async Task<ActionResult<ResponseModel<List<SensorResponseDTO>>>> Get()
        {
            var sensors = await SensorServices.GetAllSensors();
            if (sensors != null)
            {
                var Response = sensors;
                return Ok(Response);
            }
            else
            {
                var Response = new ResponseModel<SensorResponseDTO>()
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
                var Response = SensorServices.DeleteSensorById(id);
                return Ok(await Response);
            }
            else
            {
                var Response = new ResponseModel<SensorResponseDTO>()
                {
                    remarks = "Sensor not found",
                    success = false
                };
                return BadRequest(Response);
            }
        }
    }
}
