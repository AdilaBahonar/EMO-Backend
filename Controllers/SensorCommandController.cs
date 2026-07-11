using EMO.Extensions.MiddleWare;
using EMO.Models.DTOs.SensorCommandDTOs;
using EMO.Repositories.SensorCommandRepo;
using Microsoft.AspNetCore.Mvc;

namespace EMO.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [ApiKey]
    public class SensorCommandController : ControllerBase
    {
        private readonly ISensorCommandService sensorCommandService;

        public SensorCommandController(ISensorCommandService sensorCommandService)
        {
            this.sensorCommandService = sensorCommandService;
        }

        [HttpPost("Relay")]
        public async Task<IActionResult> Relay([FromBody] SensorRelayCommandRequestDTO requestDto)
        {
            var response = await sensorCommandService.SendRelayCommandAsync(requestDto);
            return Ok(response);
        }
    }
}
