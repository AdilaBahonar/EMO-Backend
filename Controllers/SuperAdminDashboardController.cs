using EMO.Models.DTOs.ResponseDTO;
using EMO.Models.DTOs.SuperAdminDashboardDTOs;
using EMO.Repositories.SuperAdminDashboardServicesRepo;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace EMO.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SuperAdminDashboardController : ControllerBase
    {
        private readonly ISuperAdminDashboardServices superAdminDashboardServices;

        public SuperAdminDashboardController(ISuperAdminDashboardServices superAdminDashboardServices)
        {
            this.superAdminDashboardServices = superAdminDashboardServices;
        }

        [HttpGet]
        public async Task<ActionResult<ResponseModel<SuperAdminDashboardResponseDTO>>> Get()
        {
            var response = await superAdminDashboardServices.GetDashboard();

            if (response.success)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }

        [HttpGet("BusinessWiseSummary")]
        public async Task<ActionResult<ResponseModel<List<BusinessWiseDashboardDTO>>>> GetBusinessWiseSummary()
        {
            var response = await superAdminDashboardServices.GetBusinessWiseSummary();

            if (response.success)
            {
                return Ok(response);
            }

            return BadRequest(response);
        }
    }
}
