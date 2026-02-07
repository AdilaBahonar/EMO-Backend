using EMO.Extensions;
using EMO.Extensions.MiddleWare;
using EMO.Models.DTOs.BuildingDTOs;
using EMO.Models.DTOs.DeviceDTOs;
using EMO.Models.DTOs.FacilityDTOs;
using EMO.Models.DTOs.ResponseDTO;
using EMO.Repositories.BuildingServicesRepo;
using EMO.Repositories.DeviceServicesRepo;
using EMO.Repositories.FacilityServicesRepo;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EMO.Controllers
{
    [ApiKey]
    [Route("api/[controller]")]
    [ApiController]
    public class DeviceController : ControllerBase
    {
        private readonly  IDeviceServices DeviceServices;
        private readonly OtherServices otherServices;

        public DeviceController(IDeviceServices DeviceServices, OtherServices otherServices)
        {
            this.DeviceServices = DeviceServices;
            this.otherServices = otherServices;
        }

        [HttpPost]
        public async Task<ActionResult<ResponseModel<DeviceResponseDTO>>> Post(AddDeviceDTO model)
        {
            if (ModelState.IsValid)
            {
                var Response = DeviceServices.AddDevice(model);
                return Ok(await Response);
            }
            else
            {
                var Response = new ResponseModel<DeviceResponseDTO>()
                {
                    remarks = "Model Not Verified",
                    success = false
                };
                return BadRequest(Response);
            }
        }


        [HttpGet("GetByBusinessId")]
        public async Task<ActionResult<ResponseModel<List<DeviceResponseDTO>>>> GetByBusinessId(string id)
        {
            if (otherServices.Check(id))
            {
                var Response = DeviceServices.GetDeviceByBusinessId(id);
                return Ok(await Response);
            }
            else
            {
                var Response = new ResponseModel<List<FacilityResponseDTO>>()
                {
                    remarks = "Invalid Request.",
                    success = false
                };
                return BadRequest(Response);
            }
        }

        [HttpPut]
        public async Task<ActionResult<ResponseModel<DeviceResponseDTO>>> Put(UpdateDeviceDTO model)
        {
            if (ModelState.IsValid)
            {
                var Response = DeviceServices.UpdateDevice(model);
                return Ok(await Response);
            }
            else
            {
                var Response = new ResponseModel<DeviceResponseDTO>()
                {
                    remarks = "Model Not Verified",
                    success = false
                };
                return BadRequest(Response);
            }
        }

        [HttpGet("GetById")]
        public async Task<ActionResult<ResponseModel<DeviceResponseDTO>>> GetById(string id)
        {
            if (otherServices.Check(id))
            {
                var Response = DeviceServices.GetDeviceById(id);
                return Ok(await Response);
            }
            else
            {
                var Response = new ResponseModel<DeviceResponseDTO>()
                {
                    remarks = "Device not found by ID",
                    success = false
                };
                return BadRequest(Response);
            }
        }

        [HttpGet]
        public async Task<ActionResult<ResponseModel<List<DeviceResponseDTO>>>> Get()
        {
            var Devices = await DeviceServices.GetAllDevices();
            if (Devices != null)
            {
                var Response = Devices;
                return Ok(Response);
            }
            else
            {
                var Response = new ResponseModel<DeviceResponseDTO>()
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
                var Response = DeviceServices.DeleteDeviceById(id);
                return Ok(await Response);
            }
            else
            {
                var Response = new ResponseModel<DeviceResponseDTO>()
                {
                    remarks = "Device not found",
                    success = false
                };
                return BadRequest(Response);
            }
        }
    }
}
