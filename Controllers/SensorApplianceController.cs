using EMO.Extensions;
using EMO.Extensions.MiddleWare;
using EMO.Models.DTOs.ResponseDTO;
using EMO.Models.DTOs.SensorApplianceDTOs;
using EMO.Repositories.SensorApplianceServicesRepo;
using Microsoft.AspNetCore.Mvc;

namespace EMO.Controllers
{
    [ApiKey]
    [Route("api/[controller]")]
    [ApiController]
    public class SensorApplianceController : ControllerBase
    {
        private readonly ISensorApplianceServices sensorApplianceServices;
        private readonly OtherServices otherServices;

        public SensorApplianceController(ISensorApplianceServices sensorApplianceServices, OtherServices otherServices)
        {
            this.sensorApplianceServices = sensorApplianceServices;
            this.otherServices = otherServices;
        }

        [HttpPost("Assign")]
        public async Task<ActionResult<ResponseModel<SensorApplianceResponseDTO>>> Assign(AssignSensorApplianceDTO model)
        {
            if (ModelState.IsValid)
            {
                var Response = sensorApplianceServices.AssignApplianceToSensor(model);
                return Ok(await Response);
            }
            else
            {
                var Response = new ResponseModel<SensorApplianceResponseDTO>()
                {
                    remarks = "Model Not Verified",
                    success = false
                };
                return BadRequest(Response);
            }
        }

        [HttpPut]
        public async Task<ActionResult<ResponseModel<SensorApplianceResponseDTO>>> Put(UpdateSensorApplianceDTO model)
        {
            if (ModelState.IsValid)
            {
                var Response = sensorApplianceServices.UpdateSensorAppliance(model);
                return Ok(await Response);
            }
            else
            {
                var Response = new ResponseModel<SensorApplianceResponseDTO>()
                {
                    remarks = "Model Not Verified",
                    success = false
                };
                return BadRequest(Response);
            }
        }

        [HttpGet]
        public async Task<ActionResult<ResponseModel<List<SensorApplianceResponseDTO>>>> Get()
        {
            var Response = sensorApplianceServices.GetAllSensorAppliances();
            return Ok(await Response);
        }

        [HttpGet("GetById")]
        public async Task<ActionResult<ResponseModel<SensorApplianceResponseDTO>>> GetById(string id)
        {
            if (otherServices.Check(id))
            {
                var Response = sensorApplianceServices.GetSensorApplianceById(id);
                return Ok(await Response);
            }
            else
            {
                var Response = new ResponseModel<SensorApplianceResponseDTO>()
                {
                    remarks = "Invalid id.",
                    success = false
                };
                return BadRequest(Response);
            }
        }

        [HttpGet("GetBySensorId")]
        public async Task<ActionResult<ResponseModel<SensorApplianceResponseDTO>>> GetBySensorId(string id)
        {
            if (otherServices.Check(id))
            {
                var Response = sensorApplianceServices.GetActiveApplianceBySensorId(id);
                return Ok(await Response);
            }
            else
            {
                var Response = new ResponseModel<SensorApplianceResponseDTO>()
                {
                    remarks = "Invalid sensor id.",
                    success = false
                };
                return BadRequest(Response);
            }
        }


        [HttpGet("GetAssignableBySensorId")]
        public async Task<ActionResult<ResponseModel<SensorAssignableAppliancesDTO>>> GetAssignableBySensorId(string id)
        {
            if (otherServices.Check(id))
            {
                var Response = sensorApplianceServices.GetAssignableBusinessAppliancesBySensorId(id);
                return Ok(await Response);
            }
            else
            {
                var Response = new ResponseModel<SensorAssignableAppliancesDTO>()
                {
                    remarks = "Invalid sensor id.",
                    success = false
                };
                return BadRequest(Response);
            }
        }

        [HttpGet("GetByApplianceId")]
        public async Task<ActionResult<ResponseModel<List<SensorApplianceResponseDTO>>>> GetByApplianceId(string id)
        {
            if (otherServices.Check(id))
            {
                var Response = sensorApplianceServices.GetSensorAppliancesByApplianceId(id);
                return Ok(await Response);
            }
            else
            {
                var Response = new ResponseModel<List<SensorApplianceResponseDTO>>()
                {
                    remarks = "Invalid appliance id.",
                    success = false
                };
                return BadRequest(Response);
            }
        }

        [HttpGet("GetStatusBySensorId")]
        public async Task<ActionResult<ResponseModel<ApplianceStatusDTO>>> GetStatusBySensorId(string id)
        {
            if (otherServices.Check(id))
            {
                var Response = sensorApplianceServices.GetSensorApplianceStatus(id);
                return Ok(await Response);
            }
            else
            {
                var Response = new ResponseModel<ApplianceStatusDTO>()
                {
                    remarks = "Invalid sensor id.",
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
                var Response = sensorApplianceServices.DeleteSensorApplianceById(id);
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
    }
}
