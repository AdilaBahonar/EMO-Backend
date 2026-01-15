// ========================= Controller =========================
using EMO.Models.DTOs.SensorTypeDTOs;
using EMO.Repositories.SensorTypeServicesRepo;
using Microsoft.AspNetCore.Mvc;
using EMO.Extensions;
using EMO.Extensions.MiddleWare;
using EMO.Models.DTOs.ResponseDTO;

namespace EMO.Controllers
{
    [ApiKey]
    [Route("api/[controller]")]
    [ApiController]
    public class SensorTypeController : ControllerBase
    {
        private readonly ISensortypeServices SensorTypeServices;
        private readonly OtherServices otherServices;

        public SensorTypeController(ISensortypeServices SensorTypeServices, OtherServices otherServices)
        {
            this.SensorTypeServices = SensorTypeServices;
            this.otherServices = otherServices;
        }

        [HttpPost]
        public async Task<ActionResult<ResponseModel<SensorTypeResponseDTO>>> Post(AddSensorTypeDTO model)
        {
            if (ModelState.IsValid)
            {
                var Response = SensorTypeServices.AddSensorType(model);
                return Ok(await Response);
            }
            else
            {
                var Response = new ResponseModel<SensorTypeResponseDTO>()
                {
                    remarks = "Model Not Verified",
                    success = false
                };
                return BadRequest(Response);
            }
        }

        [HttpPut]
        public async Task<ActionResult<ResponseModel<SensorTypeResponseDTO>>> Put(UpdateSensorTypeDTO model)
        {
            if (ModelState.IsValid)
            {
                var Response = SensorTypeServices.UpdateSensorType(model);
                return Ok(await Response);
            }
            else
            {
                var Response = new ResponseModel<SensorTypeResponseDTO>()
                {
                    remarks = "Model Not Verified",
                    success = false
                };
                return BadRequest(Response);
            }
        }

        [HttpGet("GetById")]
        public async Task<ActionResult<ResponseModel<SensorTypeResponseDTO>>> GetById(string id)
        {
            if (otherServices.Check(id))
            {
                var Response = SensorTypeServices.GetSensorTypeById(id);
                return Ok(await Response);
            }
            else
            {
                var Response = new ResponseModel<SensorTypeResponseDTO>()
                {
                    remarks = "Sensor Type not found by ID",
                    success = false
                };
                return BadRequest(Response);
            }
        }

        [HttpGet]
        public async Task<ActionResult<ResponseModel<List<SensorTypeResponseDTO>>>> Get()
        {
            var items = await SensorTypeServices.GetAllSensorTypes();
            if (items != null)
            {
                return Ok(items);
            }
            else
            {
                var Response = new ResponseModel<SensorTypeResponseDTO>()
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
                var Response = SensorTypeServices.DeleteSensorTypeById(id);
                return Ok(await Response);
            }
            else
            {
                var Response = new ResponseModel<SensorTypeResponseDTO>()
                {
                    remarks = "Sensor Type not found",
                    success = false
                };
                return BadRequest(Response);
            }
        }
    }
}
