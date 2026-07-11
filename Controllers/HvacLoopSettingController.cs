using EMO.Extensions;
using EMO.Extensions.MiddleWare;
using EMO.Models.DTOs.HvacLoopSettingDTOs;
using EMO.Models.DTOs.ResponseDTO;
using EMO.Repositories.HvacLoopSettingServicesRepo;
using Microsoft.AspNetCore.Mvc;

namespace EMO.Controllers
{
    [ApiKey]
    [Route("api/[controller]")]
    [ApiController]
    public class HvacLoopSettingController : ControllerBase
    {
        private readonly IHvacLoopSettingServices hvacLoopSettingServices;
        private readonly OtherServices otherServices;

        public HvacLoopSettingController(
            IHvacLoopSettingServices hvacLoopSettingServices,
            OtherServices otherServices)
        {
            this.hvacLoopSettingServices = hvacLoopSettingServices;
            this.otherServices = otherServices;
        }

        [HttpPost]
        public async Task<ActionResult<ResponseModel<HvacLoopSettingResponseDTO>>> Post(AddHvacLoopSettingDTO model)
        {
            if (ModelState.IsValid)
            {
                var response = await hvacLoopSettingServices.AddHvacLoopSetting(model);
                return Ok(response);
            }

            return BadRequest(new ResponseModel<HvacLoopSettingResponseDTO>
            {
                remarks = "Model Not Verified",
                success = false
            });
        }

        [HttpPut]
        public async Task<ActionResult<ResponseModel<HvacLoopSettingResponseDTO>>> Put(UpdateHvacLoopSettingDTO model)
        {
            if (ModelState.IsValid)
            {
                var response = await hvacLoopSettingServices.UpdateHvacLoopSetting(model);
                return Ok(response);
            }

            return BadRequest(new ResponseModel<HvacLoopSettingResponseDTO>
            {
                remarks = "Model Not Verified",
                success = false
            });
        }

        [HttpPut("EnableLoop")]
        public async Task<ActionResult<ResponseModel<HvacLoopSettingResponseDTO>>> EnableLoop(HvacLoopSensorDTO model)
        {
            if (ModelState.IsValid)
            {
                var response = await hvacLoopSettingServices.EnableLoop(model);
                return Ok(response);
            }

            return BadRequest(new ResponseModel<HvacLoopSettingResponseDTO>
            {
                remarks = "Model Not Verified",
                success = false
            });
        }

        [HttpPut("DisableLoop")]
        public async Task<ActionResult<ResponseModel<HvacLoopSettingResponseDTO>>> DisableLoop(HvacLoopSensorDTO model)
        {
            if (ModelState.IsValid)
            {
                var response = await hvacLoopSettingServices.DisableLoop(model);
                return Ok(response);
            }

            return BadRequest(new ResponseModel<HvacLoopSettingResponseDTO>
            {
                remarks = "Model Not Verified",
                success = false
            });
        }

        [HttpGet]
        public async Task<ActionResult<ResponseModel<List<HvacLoopSettingResponseDTO>>>> Get()
        {
            var response = await hvacLoopSettingServices.GetAllHvacLoopSettings();
            return Ok(response);
        }

        [HttpGet("GetById")]
        public async Task<ActionResult<ResponseModel<HvacLoopSettingResponseDTO>>> GetById(string id)
        {
            if (otherServices.Check(id))
            {
                var response = await hvacLoopSettingServices.GetHvacLoopSettingById(id);
                return Ok(response);
            }

            return BadRequest(new ResponseModel<HvacLoopSettingResponseDTO>
            {
                remarks = "Invalid id.",
                success = false
            });
        }

        [HttpGet("GetBySensorId")]
        public async Task<ActionResult<ResponseModel<HvacLoopSettingResponseDTO>>> GetBySensorId(string id)
        {
            if (otherServices.Check(id))
            {
                var response = await hvacLoopSettingServices.GetHvacLoopSettingBySensorId(id);
                return Ok(response);
            }

            return BadRequest(new ResponseModel<HvacLoopSettingResponseDTO>
            {
                remarks = "Invalid sensor id.",
                success = false
            });
        }

        [HttpGet("GetByBusinessId")]
        public async Task<ActionResult<ResponseModel<List<HvacLoopSettingResponseDTO>>>> GetByBusinessId(string id)
        {
            if (otherServices.Check(id))
            {
                var response = await hvacLoopSettingServices.GetHvacLoopSettingsByBusinessId(id);
                return Ok(response);
            }

            return BadRequest(new ResponseModel<List<HvacLoopSettingResponseDTO>>
            {
                remarks = "Invalid business id.",
                success = false
            });
        }

        [HttpDelete]
        public async Task<ActionResult<ResponseModel>> Delete(string id)
        {
            if (otherServices.Check(id))
            {
                var response = await hvacLoopSettingServices.DeleteHvacLoopSettingById(id);
                return Ok(response);
            }

            return BadRequest(new ResponseModel
            {
                remarks = "Invalid id.",
                success = false
            });
        }
    }
}
