using APIProduct.Models.DTOs.OfficeDTOs;
using APIProduct.Repositories.OfficeServicesRepo;
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
