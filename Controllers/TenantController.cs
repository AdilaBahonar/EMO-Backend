using EMO.Models.DTOs.TenantDTOs;
using EMO.Repositories.TenantServicesRepo;
using Microsoft.AspNetCore.Mvc;
using EMO.Extensions;
using EMO.Extensions.MiddleWare;
using EMO.Models.DTOs.ResponseDTO;
using EMO.Models.DTOs.TenantDTOs.EMO.Models.DTOs.TenantDTOs;

namespace EMO.Controllers
{
    [ApiKey]
    [Route("api/[controller]")]
    [ApiController]
    public class TenantController : ControllerBase
    {
        private readonly ITenantServices TenantServices;
        private readonly OtherServices otherServices;

        public TenantController(ITenantServices TenantServices, OtherServices otherServices)
        {
            this.TenantServices = TenantServices;
            this.otherServices = otherServices;
        }

        [HttpPost]
        public async Task<ActionResult<ResponseModel<TenantResponseDTO>>> Post(AddTenantDTO model)
        {
            if (ModelState.IsValid)
            {
                var Response = TenantServices.AddTenant(model);
                return Ok(await Response);
            }
            else
            {
                var Response = new ResponseModel<TenantResponseDTO>()
                {
                    remarks = "Model Not Verified",
                    success = false
                };
                return BadRequest(Response);
            }
        }

        [HttpPut]
        public async Task<ActionResult<ResponseModel<TenantResponseDTO>>> Put(UpdateTenantDTO model)
        {
            if (ModelState.IsValid)
            {
                var Response = TenantServices.UpdateTenant(model);
                return Ok(await Response);
            }
            else
            {
                var Response = new ResponseModel<TenantResponseDTO>()
                {
                    remarks = "Model Not Verified",
                    success = false
                };
                return BadRequest(Response);
            }
        }

        [HttpGet("GetById")]
        public async Task<ActionResult<ResponseModel<TenantResponseDTO>>> GetById(string id)
        {
            if (otherServices.Check(id))
            {
                var Response = TenantServices.GetTenantById(id);
                return Ok(await Response);
            }
            else
            {
                var Response = new ResponseModel<TenantResponseDTO>()
                {
                    remarks = "Tenant not found by ID",
                    success = false
                };
                return BadRequest(Response);
            }
        }

        [HttpGet]
        public async Task<ActionResult<ResponseModel<List<TenantResponseDTO>>>> Get()
        {
            var tenants = await TenantServices.GetAllTenants();
            if (tenants != null)
            {
                var Response = tenants;
                return Ok(Response);
            }
            else
            {
                var Response = new ResponseModel<TenantResponseDTO>()
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
                var Response = TenantServices.DeleteTenantById(id);
                return Ok(await Response);
            }
            else
            {
                var Response = new ResponseModel<TenantResponseDTO>()
                {
                    remarks = "Tenant not found",
                    success = false
                };
                return BadRequest(Response);
            }
        }
    }
}
