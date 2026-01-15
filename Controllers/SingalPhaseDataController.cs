using EMO.Models.DTOs.SingalPhaseDTOs;
using EMO.Repositories.SingalPhaseDataRepo;
using Microsoft.AspNetCore.Mvc;
using EMO.Extensions;
using EMO.Extensions.MiddleWare;
using EMO.Models.DTOs.ResponseDTO;
using EMO.Repositories.SingalPhaseDataServicesRepo;
using P3AHR.Extensions.MiddleWare;
using P3AHR.Models.DTOs.ResponseDTO;

namespace EMO.Controllers
{
    [ApiKey]
    [Route("api/[controller]")]
    [ApiController]
    public class SingalPhaseDataController : ControllerBase
    {
        private readonly ISingalPhaseDataService SingalPhaseDataService;
        private readonly OtherServices otherServices;

        public SingalPhaseDataController(ISingalPhaseDataService SingalPhaseDataService, OtherServices otherServices)
        {
            this.SingalPhaseDataService = SingalPhaseDataService;
            this.otherServices = otherServices;
        }

        [HttpPost]
        public async Task<ActionResult<ResponseModel<SingalPhaseDataResponseDTO>>> Post(AddSingalPhaseDataDTO model)
        {
            if (ModelState.IsValid)
            {
                var Response = SingalPhaseDataService.AddSingalPhaseData(model);
                return Ok(await Response);
            }
            else
            {
                var Response = new ResponseModel<SingalPhaseDataResponseDTO>()
                {
                    remarks = "Model Not Verified",
                    success = false
                };
                return BadRequest(Response);
            }
        }

        [HttpPut]
        public async Task<ActionResult<ResponseModel<SingalPhaseDataResponseDTO>>> Put(UpdateSingalPhaseDataDTO model)
        {
            if (ModelState.IsValid)
            {
                var Response = SingalPhaseDataService.UpdateSingalPhaseData(model);
                return Ok(await Response);
            }
            else
            {
                var Response = new ResponseModel<SingalPhaseDataResponseDTO>()
                {
                    remarks = "Model Not Verified",
                    success = false
                };
                return BadRequest(Response);
            }
        }

        [HttpGet("GetById")]
        public async Task<ActionResult<ResponseModel<SingalPhaseDataResponseDTO>>> GetById(string id)
        {
            if (otherServices.Check(id))
            {
                var Response = SingalPhaseDataService.GetSingalPhaseDataById(Guid.Parse(id));
                return Ok(await Response);
            }
            else
            {
                var Response = new ResponseModel<SingalPhaseDataResponseDTO>()
                {
                    remarks = "Singal Phase Data not found by ID",
                    success = false
                };
                return BadRequest(Response);
            }
        }

        [HttpGet]
        public async Task<ActionResult<ResponseModel<List<SingalPhaseDataResponseDTO>>>> Get()
        {
            var DataList = await SingalPhaseDataService.GetAllSingalPhaseData();
            if (DataList != null)
            {
                var Response = DataList;
                return Ok(Response);
            }
            else
            {
                var Response = new ResponseModel<SingalPhaseDataResponseDTO>()
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
                var Response = SingalPhaseDataService.DeleteSingalPhaseDataById(Guid.Parse(id));
                return Ok(await Response);
            }
            else
            {
                var Response = new ResponseModel<SingalPhaseDataResponseDTO>()
                {
                    remarks = "Singal Phase Data not found",
                    success = false
                };
                return BadRequest(Response);
            }
        }
    }
}
