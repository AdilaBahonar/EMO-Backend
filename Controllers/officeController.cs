using EMO.Extensions;
using EMO.Extensions.MiddleWare;
using EMO.Models.DTOs.FloorDTOs;
using EMO.Models.DTOs.OfficeDTOs;
using EMO.Models.DTOs.ResponseDTO;
using EMO.Repositories.FloorServicesRepo;
using EMO.Repositories.OfficeServicesRepo;
using EMO.Repositories.SectionServicesRepo;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EMO.Controllers
{
    [ApiKey]
    [Route("api/[controller]")]
    [ApiController]
    public class OfficeController : ControllerBase
    {
        private readonly IOfficeServices OfficeServices;
        private readonly OtherServices otherServices;

        public OfficeController(IOfficeServices OfficeServices, OtherServices otherServices)
        {
            this.OfficeServices = OfficeServices;
            this.otherServices = otherServices;
        }

        [HttpPost]
        public async Task<ActionResult<ResponseModel<OfficeResponseDTO>>> Post(AddOfficeDTO model)
        {
            if (ModelState.IsValid)
            {
                var Response = OfficeServices.AddOffice(model);
                return Ok(await Response);
            }
            else
            {
                var Response = new ResponseModel<OfficeResponseDTO>()
                {
                    remarks = "Model Not Verified",
                    success = false
                };
                return BadRequest(Response);
            }
        }

        [HttpGet("GetBySectionId")]
        public async Task<ActionResult<ResponseModel<List<OfficeResponseDTO>>>> GetBySectionId(string id)
        {
            if (otherServices.Check(id))
            {
                var Response = OfficeServices.GetOfficeBySectionId(id);
                return Ok(await Response);
            }
            else
            {
                var Response = new ResponseModel<List<OfficeResponseDTO>>()
                {
                    remarks = "Invalid id.",
                    success = false
                };
                return BadRequest(Response);
            }
        }
        [HttpGet("GetAvailableOfficesBySectionId")]
        public async Task<ActionResult<ResponseModel<List<OfficeResponseDTO>>>> GetAvailableOfficesBySectionId(string id)
        {
            if (otherServices.Check(id))
            {
                var Response = OfficeServices.GetAvailableOfficesBySectionId(id);
                return Ok(await Response);
            }
            else
            {
                var Response = new ResponseModel<List<FloorResponseDTO>>()
                {
                    remarks = "Invalid id.",
                    success = false
                };
                return BadRequest(Response);
            }
        }

        [HttpGet("GetByBusinessId")]
        public async Task<ActionResult<ResponseModel<List<OfficeResponseDTO>>>> GetByBusinessId(string id)
        {
            if (otherServices.Check(id))
            {
                var Response = OfficeServices.GetOfficeByBusinessId(id);
                return Ok(await Response);
            }
            else
            {
                var Response = new ResponseModel<List<OfficeResponseDTO>>()
                {
                    remarks = "Invalid request",
                    success = false
                };
                return BadRequest(Response);
            }
        }

        [HttpPut]
        public async Task<ActionResult<ResponseModel<OfficeResponseDTO>>> Put(UpdateOfficeDTO model)
        {
            if (ModelState.IsValid)
            {
                var Response = OfficeServices.UpdateOffice(model);
                return Ok(await Response);
            }
            else
            {
                var Response = new ResponseModel<OfficeResponseDTO>()
                {
                    remarks = "Model Not Verified",
                    success = false
                };
                return BadRequest(Response);
            }
        }

        [HttpGet("GetById")]
        public async Task<ActionResult<ResponseModel<OfficeResponseDTO>>> GetById(string id)
        {
            if (otherServices.Check(id))
            {
                var Response = OfficeServices.GetOfficeById(id);
                return Ok(await Response);
            }
            else
            {
                var Response = new ResponseModel<OfficeResponseDTO>()
                {
                    remarks = "Office not found by ID",
                    success = false
                };
                return BadRequest(Response);
            }
        }

        [HttpGet]
        public async Task<ActionResult<ResponseModel<List<OfficeResponseDTO>>>> Get()
        {
            var Offices = await OfficeServices.GetAllOffices();
            if (Offices != null)
            {
                var Response = Offices;
                return Ok(Response);
            }
            else
            {
                var Response = new ResponseModel<OfficeResponseDTO>()
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
                var Response = OfficeServices.DeleteOfficeById(id);
                return Ok(await Response);
            }
            else
            {
                var Response = new ResponseModel<OfficeResponseDTO>()
                {
                    remarks = "Office not found",
                    success = false
                };
                return BadRequest(Response);
            }
        }
    }

}
