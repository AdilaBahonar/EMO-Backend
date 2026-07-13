using EMO.Extensions.MiddleWare;
using EMO.Models.DTOs.EnergyConfigurationDTOs;
using EMO.Models.DTOs.ResponseDTO;
using EMO.Repositories.EnergyConfigurationRepo;
using Microsoft.AspNetCore.Mvc;

namespace EMO.Controllers
{
    [ApiKey]
    [Route("api/[controller]")]
    [ApiController]
    public class EnergyConfigurationController : ControllerBase
    {
        private readonly IEnergyConfigurationService service;
        public EnergyConfigurationController(IEnergyConfigurationService service) { this.service = service; }

        [HttpGet("GetByBusinessId")]
        public async Task<ActionResult<ResponseModel<EnergyConfigurationDTO>>> GetByBusinessId(string id)
            => Ok(await service.GetByBusinessId(id));

        [HttpPut]
        public async Task<ActionResult<ResponseModel<EnergyConfigurationDTO>>> Put(EnergyConfigurationDTO model)
        {
            if (!ModelState.IsValid) return BadRequest(new ResponseModel<EnergyConfigurationDTO> { remarks = "Model Not Verified", success = false });
            return Ok(await service.Save(model));
        }
    }
}
