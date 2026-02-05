using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using EMO.Models.DTOs.GenderDTOs;
using EMO.Repositories.GenderServicesRepo;

namespace EMO.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GenderController : ControllerBase
    {
        private readonly IGenderServices _genderService;

        public GenderController(IGenderServices genderService)
        {
            _genderService = genderService;
        }
        [HttpPost]
        public async Task<IActionResult> Add([FromBody] AddGenderDTO requestDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _genderService.AddGender(requestDto);
            return result.success ? Ok(result) : BadRequest(result);
        }

        // ================= Update Gender =================
        [HttpPut]
        public async Task<IActionResult> Update([FromBody] UpdateGenderDTO requestDto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _genderService.UpdateGender(requestDto);
            return result.success ? Ok(result) : BadRequest(result);
        }

        // ================= Get All Genders =================
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _genderService.GetAllGenders();
            return result.success ? Ok(result) : NotFound(result);
        }

        // ================= Get Gender By Id =================
        [HttpGet("GetById")]
        public async Task<IActionResult> GetById(string id)
        {
            var result = await _genderService.GetGenderById(id);
            return result.success ? Ok(result) : NotFound(result);
        }

        // ================= Delete Gender =================
        [HttpDelete]
        public async Task<IActionResult> Delete(string id)
        {
            var result = await _genderService.DeleteGender(id);
            return result.success ? Ok(result) : NotFound(result);
        }
    }
}
