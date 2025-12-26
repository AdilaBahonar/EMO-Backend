using APIProduct.Models.DTOs.BuildingDTOs;
using APIProduct.Models.DTOs.ControlTypeDTOs;
using APIProduct.Repositories.BuildingServicesRepo;
using APIProduct.Repositories.BusinessServicesRepo;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using P3AHR.Extensions;
using P3AHR.Extensions.MiddleWare;
using P3AHR.Models.DTOs.ResponseDTO;

namespace APIProduct.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ControlTypeController : ControllerBase
    {
        private readonly IControlTypeServices ControlTypeServices;
        private readonly OtherServices otherServices;

        public ControlTypeController(IControlTypeServices ControlTypeServices, OtherServices otherServices)
        {
            this.ControlTypeServices = ControlTypeServices;
            this.otherServices = otherServices;
        }

        [HttpPost]
        public async Task<ActionResult<ResponseModel<ControlTypeResponseDTO>>> Post(AddControlTypeDTO model)
        {
            if (ModelState.IsValid)
            {
                var Response = ControlTypeServices.AddControlType(model);
                return Ok(await Response);
            }
            else
            {
                var Response = new ResponseModel<ControlTypeResponseDTO>()
                {
                    remarks = "Model Not Verified",
                    success = false
                };
                return BadRequest(Response);
            }
        }

        [HttpPut]
        public async Task<ActionResult<ResponseModel<ControlTypeResponseDTO>>> Put(UpdateControlTypeDTO model)
        {
            if (ModelState.IsValid)
            {
                var Response = ControlTypeServices.UpdateControlType(model);
                return Ok(await Response);
            }
            else
            {
                var Response = new ResponseModel<ControlTypeResponseDTO>()
                {
                    remarks = "Model Not Verified",
                    success = false
                };
                return BadRequest(Response);
            }
        }

        [HttpGet("GetById")]
        public async Task<ActionResult<ResponseModel<ControlTypeResponseDTO>>> GetById(string id)
        {
            if (otherServices.Check(id))
            {
                var Response = ControlTypeServices.GetControlTypeById(id);
                return Ok(await Response);
            }
            else
            {
                var Response = new ResponseModel<ControlTypeResponseDTO>()
                {
                    remarks = "Control Type not found by ID",
                    success = false
                };
                return BadRequest(Response);
            }
        }

        [HttpGet]
        public async Task<ActionResult<ResponseModel<List<ControlTypeResponseDTO>>>> Get()
        {
            var ControlTypes = await ControlTypeServices.GetAllControlTypes();
            if (ControlTypes != null)
            {
                var Response = ControlTypes;
                return Ok(Response);
            }
            else
            {
                var Response = new ResponseModel<ControlTypeResponseDTO>()
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
                var Response = ControlTypeServices.DeleteControlTypeById(id);
                return Ok(await Response);
            }
            else
            {
                var Response = new ResponseModel<ControlTypeResponseDTO>()
                {
                    remarks = "Control Type not found",
                    success = false
                };
                return BadRequest(Response);
            }
        }
    }

}
   

