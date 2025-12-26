using APIProduct.Models.DTOs.DeviceDTOs;
using APIProduct.Models.DTOs.PocDTOs;
using APIProduct.Repositories.DeviceServicesRepo;
using APIProduct.Repositories.PocServicesRepo;
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
    public class PocController : ControllerBase
    {
        private readonly IPocServices PocServices;
        private readonly OtherServices otherServices;

        public PocController(IPocServices PocServices, OtherServices otherServices)
        {
            this.PocServices = PocServices;
            this.otherServices = otherServices;
        }

        [HttpPost]
        public async Task<ActionResult<ResponseModel<PocResponseDTO>>> Post(AddPocDTO model)
        {
            if (ModelState.IsValid)
            {
                var Response = PocServices.AddPoc(model);
                return Ok(await Response);
            }
            else
            {
                var Response = new ResponseModel<PocResponseDTO>()
                {
                    remarks = "Model Not Verified",
                    success = false
                };
                return BadRequest(Response);
            }
        }

        [HttpPut]
        public async Task<ActionResult<ResponseModel<PocResponseDTO>>> Put(UpdatePocDTO model)
        {
            if (ModelState.IsValid)
            {
                var Response = PocServices.UpdatePoc(model);
                return Ok(await Response);
            }
            else
            {
                var Response = new ResponseModel<PocResponseDTO>()
                {
                    remarks = "Model Not Verified",
                    success = false
                };
                return BadRequest(Response);
            }
        }

        [HttpGet("GetById")]
        public async Task<ActionResult<ResponseModel<PocResponseDTO>>> GetById(string id)
        {
            if (otherServices.Check(id))
            {
                var Response = PocServices.GetPocById(id);
                return Ok(await Response);
            }
            else
            {
                var Response = new ResponseModel<PocResponseDTO>()
                {
                    remarks = "Poc not found by ID",
                    success = false
                };
                return BadRequest(Response);
            }
        }

        [HttpGet]
        public async Task<ActionResult<ResponseModel<List<PocResponseDTO>>>> Get()
        {
            var Poces = await PocServices.GetAllPoces();
            if (Poces != null)
            {
                var Response = Poces;
                return Ok(Response);
            }
            else
            {
                var Response = new ResponseModel<PocResponseDTO>()
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
                var Response = PocServices.DeletePocById(id);
                return Ok(await Response);
            }
            else
            {
                var Response = new ResponseModel<PocResponseDTO>()
                {
                    remarks = "Poc not found",
                    success = false
                };
                return BadRequest(Response);
            }
        }
    }
}

    

