//using EMO.Models.DTOs.BuildingDTOs;
//using EMO.Models.DTOs.ControlTypeDTOs;
//using EMO.Repositories.BuildingServicesRepo;
//using EMO.Repositories.BusinessServicesRepo;
//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Mvc;
//using EMO.Extensions;
//using EMO.Extensions.MiddleWare;
//using EMO.Models.DTOs.ResponseDTO;

//namespace EMO.Controllers
//{
//    [Route("api/[controller]")]
//    [ApiController]
//    public class ControlTypeController : ControllerBase
//    {
//        private readonly IControlTypeServices ControlTypeServices;
//        private readonly OtherServices otherServices;

//        public ControlTypeController(IControlTypeServices ControlTypeServices, OtherServices otherServices)
//        {
//            this.ControlTypeServices = ControlTypeServices;
//            this.otherServices = otherServices;
//        }

//        [HttpPost]
//        public async Task<ActionResult<ResponseModel<ControlTypeResponseDTO>>> Post(AddControlTypeDTO model)
//        {
//            if (ModelState.IsValid)
//            {
//                var Response = ControlTypeServices.AddControlType(model);
//                return Ok(await Response);
//            }
//            else
//            {
//                var Response = new ResponseModel<ControlTypeResponseDTO>()
//                {
//                    remarks = "Model Not Verified",
//                    success = false
//                };
//                return BadRequest(Response);
//            }
//        }

//        [HttpPut]
//        public async Task<ActionResult<ResponseModel<ControlTypeResponseDTO>>> Put(UpdateControlTypeDTO model)
//        {
//            if (ModelState.IsValid)
//            {
//                var Response = ControlTypeServices.UpdateControlType(model);
//                return Ok(await Response);
//            }
//            else
//            {
//                var Response = new ResponseModel<ControlTypeResponseDTO>()
//                {
//                    remarks = "Model Not Verified",
//                    success = false
//                };
//                return BadRequest(Response);
//            }
//        }

//        [HttpGet("GetById")]
//        public async Task<ActionResult<ResponseModel<ControlTypeResponseDTO>>> GetById(string id)
//        {
//            if (otherServices.Check(id))
//            {
//                var Response = ControlTypeServices.GetControlTypeById(id);
//                return Ok(await Response);
//            }
//            else
//            {
//                var Response = new ResponseModel<ControlTypeResponseDTO>()
//                {
//                    remarks = "Control Type not found by ID",
//                    success = false
//                };
//                return BadRequest(Response);
//            }
//        }

//        [HttpGet]
//        public async Task<ActionResult<ResponseModel<List<ControlTypeResponseDTO>>>> Get()
//        {
//            var ControlTypes = await ControlTypeServices.GetAllControlTypes();
//            if (ControlTypes != null)
//            {
//                var Response = ControlTypes;
//                return Ok(Response);
//            }
//            else
//            {
//                var Response = new ResponseModel<ControlTypeResponseDTO>()
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
//                var Response = ControlTypeServices.DeleteControlTypeById(id);
//                return Ok(await Response);
//            }
//            else
//            {
//                var Response = new ResponseModel<ControlTypeResponseDTO>()
//                {
//                    remarks = "Control Type not found",
//                    success = false
//                };
//                return BadRequest(Response);
//            }
//        }
//    }

//}
   

