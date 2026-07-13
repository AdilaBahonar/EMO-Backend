using EMO.Extensions;
using EMO.Models.DTOs.AgreementDTOs;
using EMO.Models.DTOs.OfficeDTOs;
using EMO.Models.DTOs.ResponseDTO;
using EMO.Models.DTOs.SensorDTOs;
using EMO.Repositories.AgreementServicesRepo;
using EMO.Repositories.SensorServicesRepo;
using EMO.Repositories.UserAccessRepo;
using EMO.Models.DTOs.UserDTOs;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace EMO.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AgreementController : ControllerBase
    {
        private readonly IAgreementServices AgreementServices;
        private readonly OtherServices otherServices;
        private readonly IUserAccessService userAccessService;

        public AgreementController(
            IAgreementServices AgreementServices,
            OtherServices otherServices,
            IUserAccessService userAccessService)
        {
            this.AgreementServices = AgreementServices;
            this.otherServices = otherServices;
            this.userAccessService = userAccessService;
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


        [HttpGet("GetOfficesByTenantId")]
        public async Task<ActionResult<ResponseModel<List<OfficeResponseDTO>>>> GetOfficesByTenantId(string id)
        {
            if (!otherServices.Check(id) || !Guid.TryParse(id, out var requestedTenantId))
            {
                return BadRequest(new ResponseModel<List<OfficeResponseDTO>>
                {
                    remarks = "No record found.",
                    success = false
                });
            }

            var currentUser = HttpContext.Items["User"] as UserResponseDTO;
            UserAccessScope? access = null;
            if (currentUser is not null && Guid.TryParse(currentUser.userId, out var currentUserId))
                access = await userAccessService.GetByUserIdAsync(currentUserId, HttpContext.RequestAborted);

            if (access is not null)
            {
                if (!access.IsLoginAllowed) return Unauthorized();
                if (access.IsTenant && access.UserId != requestedTenantId) return Forbid();
            }

            var response = await AgreementServices.GetOfficeByTenantId(id);
            if (access is not null && !access.HasGlobalAccess && response.data is not null)
            {
                response.data = response.data
                    .Where(x => Guid.TryParse(x.fkBusiness, out var businessId) && access.BusinessIds.Contains(businessId))
                    .GroupBy(x => x.officeId)
                    .Select(x => x.First())
                    .ToList();
            }

            return Ok(response);
        }

        [HttpGet("GetByBusinessId")]
        public async Task<ActionResult<ResponseModel<List<AgreementResponseDTO>>>> GetByBusinessId(string id)
        {
            if (otherServices.Check(id))
            {
                var Response = AgreementServices.GetAgreementByBusinessId(id);
                return Ok(await Response);
            }
            else
            {
                var Response = new ResponseModel<List<AgreementResponseDTO>>()
                {
                    remarks = "No record found.",
                    success = false
                };
                return BadRequest(Response);
            }
        }

        [HttpGet("GetOfficesByAgreementId")]
        public async Task<ActionResult<ResponseModel<List<OfficeResponseDTO>>>> GetOfficesByAgreementId(string id)
        {
            if (otherServices.Check(id))
            {
                var Response = AgreementServices.GetOfficeByAgreementId(id);
                return Ok(await Response);
            }
            else
            {
                var Response = new ResponseModel<List<OfficeResponseDTO>>()
                {
                    remarks = "No record found.",
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

        [HttpPost("RemoveOfficeFromAgreement")]
        public async Task<ActionResult<ResponseModel>> RemoveOfficeFromAgreement(RemoveOfficeFromAgreementRequestDTO requestDTO)
        {
            if (ModelState.IsValid)
            {
                var Response = AgreementServices.RemoveOfficeFromAgreement(requestDTO);
                return Ok(await Response);
            }
            else
            {
                var Response = new ResponseModel()
                {
                    remarks = "No record found.",
                    success = false
                };
                return BadRequest(Response);
            }
        }
    }
}
