//using EMO.Models.DTOs.BuildingDTOs;
//using EMO.Models.DTOs.DeviceTypeDTOs;
//using EMO.Repositories.BuildingServicesRepo;
//using EMO.Repositories.DeviceTypeServicesRepo;
//using Microsoft.AspNetCore.Mvc;
//using EMO.Extensions;
//using EMO.Extensions.MiddleWare;
//using EMO.Models.DTOs.ResponseDTO;

//namespace EMO.Controllers
//{
    
//    [Route("api/[controller]")]
//    [ApiController]
//    public class DeviceTypeController : Controller
//    {
//        private readonly IDeviceTypeServices DeviceTypeServices;
//        private readonly OtherServices otherServices;

//        public DeviceTypeController(IDeviceTypeServices DeviceTypeServices, OtherServices otherServices)
//        {
//            this.DeviceTypeServices = DeviceTypeServices;
//            this.otherServices = otherServices;
//        }

//        [HttpPost]
//        public async Task<ActionResult<ResponseModel<DeviceTypeResponseDTO>>> Post(AddDeviceTypeDTO model)
//        {
//            if (ModelState.IsValid)
//            {
//                var Response = DeviceTypeServices.AddDeviceType(model);
//                return Ok(await Response);
//            }
//            else
//            {
//                var Response = new ResponseModel<DeviceTypeResponseDTO>()
//                {
//                    remarks = "Model Not Verified",
//                    success = false
//                };
//                return BadRequest(Response);
//            }
//        }

//        [HttpPut]
//        public async Task<ActionResult<ResponseModel<DeviceTypeResponseDTO>>> Put(UpdateDeviceTypeDTO model)
//        {
//            if (ModelState.IsValid)
//            {
//                var Response = DeviceTypeServices.UpdateDeviceType(model);
//                return Ok(await Response);
//            }
//            else
//            {
//                var Response = new ResponseModel<DeviceTypeResponseDTO>()
//                {
//                    remarks = "Model Not Verified",
//                    success = false
//                };
//                return BadRequest(Response);
//            }
//        }

//        [HttpGet("GetById")]
//        public async Task<ActionResult<ResponseModel<DeviceTypeResponseDTO>>> GetById(string id)
//        {
//            if (otherServices.Check(id))
//            {
//                var Response = DeviceTypeServices.GetDeviceTypeById(id);
//                return Ok(await Response);
//            }
//            else
//            {
//                var Response = new ResponseModel<DeviceTypeResponseDTO>()
//                {
//                    remarks = "Device Type not found by ID",
//                    success = false
//                };
//                return BadRequest(Response);
//            }
//        }

//        [HttpGet]
//        public async Task<ActionResult<ResponseModel<List<DeviceTypeResponseDTO>>>> Get()
//        {
//            var DeviceTypes = await DeviceTypeServices.GetAllDeviceTypes();
//            if (DeviceTypes != null)
//            {
//                var Response = DeviceTypes;
//                return Ok(Response);
//            }
//            else
//            {
//                var Response = new ResponseModel<DeviceTypeResponseDTO>()
//                {
//                    remarks = "Model Not Verified",
//                    success = false
//                };
//                return BadRequest(Response);
//            }
//        }

//        [HttpDelete]
//        public async Task<ActionResult<ResponseModel>> Delete(string id)
//        {
//            if (otherServices.Check(id))
//            {
//                var Response = DeviceTypeServices.DeleteDeviceTypeById(id);
//                return Ok(await Response);
//            }
//            else
//            {
//                var Response = new ResponseModel<DeviceTypeResponseDTO>()
//                {
//                    remarks = "Invalid Id.",
//                    success = false
//                };
//                return BadRequest(Response);
//            }
//        }
//    }

//}
