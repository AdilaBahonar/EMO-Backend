using EMO.Extensions;
using EMO.Models.DTOs.AgreementDTOs;
using EMO.Models.DTOs.ResponseDTO;
using EMO.Repositories.AgreementServicesRepo;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EMO.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AgreementController : ControllerBase
    {
        private readonly IAgreementServices AgreementServices;
        private readonly OtherServices otherServices;

        public AgreementController(IAgreementServices AgreementServices, OtherServices otherServices)
        {
            this.AgreementServices = AgreementServices;
            this.otherServices = otherServices;
        }

        [HttpPost]
        public async Task<ActionResult<ResponseModel<AgreementResponseDTO>>> Post(AddAgreementDTO model)
        {
            if (ModelState.IsValid)
            {
                var Response = AgreementServices.AddAgreement(model);
                return Ok(await Response);
            }
            else
            {
                var Response = new ResponseModel<AgreementResponseDTO>()
                {
                    remarks = "Model Not Verified",
                    success = false
                };
                return BadRequest(Response);
            }
        }

        [HttpPut]
        public async Task<ActionResult<ResponseModel<AgreementResponseDTO>>> Put(UpdateAgreementDTO model)
        {
            if (ModelState.IsValid)
            {
                var Response = AgreementServices.UpdateAgreement(model);
                return Ok(await Response);
            }
            else
            {
                var Response = new ResponseModel<AgreementResponseDTO>()
                {
                    remarks = "Model Not Verified",
                    success = false
                };
                return BadRequest(Response);
            }
        }

        [HttpGet("GetById")]
        public async Task<ActionResult<ResponseModel<AgreementResponseDTO>>> GetById(string id)
        {
            if (otherServices.Check(id))
            {
                var Response = AgreementServices.GetAgreementById(id);
                return Ok(await Response);
            }
            else
            {
                var Response = new ResponseModel<AgreementResponseDTO>()
                {
                    remarks = "Agreement not found by ID",
                    success = false
                };
                return BadRequest(Response);
            }
        }

        [HttpGet]
        public async Task<ActionResult<ResponseModel<List<AgreementResponseDTO>>>> Get()
        {
            var Agreements = await AgreementServices.GetAllAgreements();
            if (Agreements != null)
            {
                var Response = Agreements;
                return Ok(Response);
            }
            else
            {
                var Response = new ResponseModel<AgreementResponseDTO>()
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
                var Response = AgreementServices.DeleteAgreementById(id);
                return Ok(await Response);
            }
            else
            {
                var Response = new ResponseModel<AgreementResponseDTO>()
                {
                    remarks = "Agreement not found",
                    success = false
                };
                return BadRequest(Response);
            }
        }
    }
}
