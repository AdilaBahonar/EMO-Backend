using EMO.Models.DTOs.BusinessDTOs;
using EMO.Repositories.BusinessServicesRepo;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using EMO.Extensions;
using EMO.Extensions.MiddleWare;
using EMO.Models.DTOs.ResponseDTO;

namespace EMO.Controllers
{
    [ApiKey]
    [Route("api/[controller]")]
    [ApiController]
    public class BusinessController : ControllerBase
    {
        private readonly IBusinessServices BusinessServices;
        private readonly OtherServices otherServices;

        public BusinessController(IBusinessServices BusinessServices, OtherServices otherServices)
        {
            this.BusinessServices = BusinessServices;
            this.otherServices = otherServices;
        }

        [HttpPost]
        public async Task<ActionResult<ResponseModel<BusinessResponseDTO>>> Post(AddBusinessDTO model)
        {
            if (ModelState.IsValid)
            {
                var Response = BusinessServices.AddBusiness(model);
                return Ok(await Response);
            }
            else
            {
                var Response = new ResponseModel<BusinessResponseDTO>()
                {
                    remarks = "Model Not Verified",
                    success = false
                };
                return BadRequest(Response);
            }
        }

        [HttpPut]
        public async Task<ActionResult<ResponseModel<BusinessResponseDTO>>> Put(UpdateBusinessDTO model)
        {
            if (ModelState.IsValid)
            {
                var Response = BusinessServices.UpdateBusiness(model);
                return Ok(await Response);
            }
            else
            {
                var Response = new ResponseModel<BusinessResponseDTO>()
                {
                    remarks = "Model Not Verified",
                    success = false
                };
                return BadRequest(Response);
            }
        }

        [HttpGet("GetById")]
        public async Task<ActionResult<ResponseModel<BusinessResponseDTO>>> GetById(string id)
        {
            if (otherServices.Check(id))
            {
                var Response = BusinessServices.GetBusinessById(id);
                return Ok(await Response);
            }
            else
            {
                var Response = new ResponseModel<BusinessResponseDTO>()
                {
                    remarks = "Business not found by ID",
                    success = false
                };
                return BadRequest(Response);
            }
        }

        [HttpGet]
        public async Task<ActionResult<ResponseModel<List<BusinessResponseDTO>>>> Get()
        {
            var Businesses = await BusinessServices.GetAllBusinesses();
            if (Businesses != null)
            {
                var Response = Businesses;
                return Ok(Response);
            }
            else
            {
                var Response = new ResponseModel<BusinessResponseDTO>()
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
                var Response = BusinessServices.DeleteBusinessById(id);
                return Ok(await Response);
            }
            else
            {
                var Response = new ResponseModel<BusinessResponseDTO>()
                {
                    remarks = "Business not found",
                    success = false
                };
                return BadRequest(Response);
            }
        }
    }

}
