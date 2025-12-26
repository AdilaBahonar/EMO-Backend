using APIProduct.Models.DTOs.BuildingDTOs;
using APIProduct.Models.DTOs.DeviceDTOs;
using APIProduct.Repositories.BuildingServicesRepo;
using APIProduct.Repositories.DeviceServicesRepo;
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
