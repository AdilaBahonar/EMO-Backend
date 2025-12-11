using APIProduct.Models.DTOs.SectionDTOs;
using APIProduct.Repositories.SectionServicesRepo;
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
    public class SectionController : ControllerBase
    {
        private readonly ISectionServices SectionServices;
        private readonly OtherServices otherServices;

        public SectionController(ISectionServices SectionServices, OtherServices otherServices)
        {
            this.SectionServices = SectionServices;
            this.otherServices = otherServices;
        }

        [HttpPost]
        public async Task<ActionResult<ResponseModel<SectionResponseDTO>>> Post(AddSectionDTO model)
        {
            if (ModelState.IsValid)
            {
                var Response = SectionServices.AddSection(model);
                return Ok(await Response);
            }
            else
            {
                var Response = new ResponseModel<SectionResponseDTO>()
                {
                    remarks = "Model Not Verified",
                    success = false
                };
                return BadRequest(Response);
            }
        }

        [HttpPut]
        public async Task<ActionResult<ResponseModel<SectionResponseDTO>>> Put(UpdateSectionDTO model)
        {
            if (ModelState.IsValid)
            {
                var Response = SectionServices.UpdateSection(model);
                return Ok(await Response);
            }
            else
            {
                var Response = new ResponseModel<SectionResponseDTO>()
                {
                    remarks = "Model Not Verified",
                    success = false
                };
                return BadRequest(Response);
            }
        }

        [HttpGet("GetById")]
        public async Task<ActionResult<ResponseModel<SectionResponseDTO>>> GetById(string id)
        {
            if (otherServices.Check(id))
            {
                var Response = SectionServices.GetSectionById(id);
                return Ok(await Response);
            }
            else
            {
                var Response = new ResponseModel<SectionResponseDTO>()
                {
                    remarks = "Section not found by ID",
                    success = false
                };
                return BadRequest(Response);
            }
        }

        [HttpGet]
        public async Task<ActionResult<ResponseModel<List<SectionResponseDTO>>>> Get()
        {
            var Sections = await SectionServices.GetAllSections();
            if (Sections != null)
            {
                var Response = Sections;
                return Ok(Response);
            }
            else
            {
                var Response = new ResponseModel<SectionResponseDTO>()
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
                var Response = SectionServices.DeleteSectionById(id);
                return Ok(await Response);
            }
            else
            {
                var Response = new ResponseModel<SectionResponseDTO>()
                {
                    remarks = "Section not found",
                    success = false
                };
                return BadRequest(Response);
            }
        }
    }

}
