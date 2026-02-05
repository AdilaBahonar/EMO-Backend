
using EMO.Extensions;
using EMO.Extensions.MiddleWare;
using EMO.Models.DTOs.ResponseDTO;
using EMO.Repositories.InnerServicesRepo;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using EMO.Repositories.SubUserTypeServicesRepo;
using EMO.Models.DTOs.SubUserTypeDTOs;

namespace EMO.Controllers
{
    [ApiKey]
    [Route("api/[controller]")]
    [ApiController]
    public class SubUserTypeController : ControllerBase
    {
        private readonly ISubUserTypeServices SubUserTypeService;
        private readonly IInnerServices innerServices;
        private readonly OtherServices otherServices;

        public SubUserTypeController(ISubUserTypeServices SubUserTypeService,OtherServices otherServices,IInnerServices innerServices)
        {
            this.SubUserTypeService = SubUserTypeService;
            this.innerServices = innerServices;
            this.otherServices = otherServices;
        }
        [HttpPost]
        public async Task<ActionResult<ResponseModel<SubUserTypeResponseDTO>>> Post(AddSubUserTypeDTO model)
        {
            if (ModelState.IsValid)
            {
                var Response = SubUserTypeService.AddSubUserType(model);
                return Ok(await Response);
            }
            else
            {
                var Response = new ResponseModel<SubUserTypeResponseDTO>()
                {
                    remarks = "Model Not Verified",
                    success = false
                };
                return BadRequest(Response);
            }
        }

        [HttpPut]
        public async Task<ActionResult<ResponseModel<SubUserTypeResponseDTO>>> Put(UpdateSubUserTypeDTO model)
        {
            if (ModelState.IsValid)
            {
                var Response = SubUserTypeService.UpdateSubUserType(model);
                return Ok(await Response);
            }
            else
            {
                var Response = new ResponseModel<SubUserTypeResponseDTO>()
                {
                    remarks = "Model Not Verified",
                    success = false
                };
                return BadRequest(Response);
            }
        }

        [HttpGet("GetById")]
        public async Task<ActionResult<ResponseModel<SubUserTypeResponseDTO>>> GetById(string id)
        {
            if (otherServices.Check(id))
            {
                var Response = SubUserTypeService.GetSubUserTypeById(id);
                return Ok(await Response);
            }
            else
            {
                var Response = new ResponseModel<SubUserTypeResponseDTO>()
                {
                    remarks = "SubUserType not found by ID",
                    success = false
                };
                return BadRequest(Response);
            }
        }

        [HttpGet]
        public async Task<ActionResult<ResponseModel<List<SubUserTypeResponseDTO>>>> Get()
        {
            var SubUserTypes = await SubUserTypeService.GetAllSubUserTypes();
            if (SubUserTypes != null)
            {
                return Ok(SubUserTypes);
            }
            else
            {
                var Response = new ResponseModel<SubUserTypeResponseDTO>()
                {
                    remarks = "No SubUserType found",
                    success = false
                };
                return BadRequest(Response);
            }
        }
        [HttpGet("byUserId")]
        public async Task<ActionResult<ResponseModel<List<SubUserTypeResponseDTO>>>> GetList(string userId)
        {
            var SubUserTypes = await SubUserTypeService.GetSubUserTypesByUserId(userId);
            if (SubUserTypes != null)
            {
                return Ok(SubUserTypes);
            }
            else
            {
                var Response = new ResponseModel<SubUserTypeResponseDTO>()
                {
                    remarks = "No SubUserType found",
                    success = false
                };
                return BadRequest(Response);
            }
        }

        [HttpGet("GetSubUserTypesOfBusiness")]
        public async Task<ActionResult<ResponseModel<List<SubUserTypeResponseDTO>>>> GetSubUserTypesOfBusiness(string userId)
        {
            var SubUserTypes = await SubUserTypeService.GetSubUserTypesOfBusiness(userId);
            if (SubUserTypes != null)
            {
                return Ok(SubUserTypes);
            }
            else
            {
                var Response = new ResponseModel<SubUserTypeResponseDTO>()
                {
                    remarks = "No SubUserType found",
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
                var Response = SubUserTypeService.DeleteSubUserTypeById(id);
                return Ok(await Response);
            }
            else
            {
                var Response = new ResponseModel<SubUserTypeResponseDTO>()
                {
                    remarks = "SubUserType not found",
                    success = false
                };
                return BadRequest(Response);
            }
        }
        [HttpPut("UpdateHierarchy")]
        public async Task<ActionResult<ResponseModel<SubUserTypeResponseDTO>>> UpdateHierarchy(List<SubUserTypeHierarchyDTO> model)
        {
            if (ModelState.IsValid)
            {
                var Response = SubUserTypeService.UpdateSubUserTypeHierarchy(model);
                return Ok(await Response);
            }
            else
            {
                var Response = new ResponseModel<SubUserTypeResponseDTO>()
                {
                    remarks = "Model Not Verified",
                    success = false
                };
                return BadRequest(Response);
            }
        }
    }
}
