using EMO.Models.DTOs.ContactPersonDTOs;
using EMO.Repositories.ContactPersonServicesRepo;
using Microsoft.AspNetCore.Mvc;
using EMO.Extensions;
using EMO.Extensions.MiddleWare;
using EMO.Models.DTOs.ResponseDTO;

namespace EMO.Controllers
{
    [ApiKey]
    [Route("api/[controller]")]
    [ApiController]
    public class ContactPersonController : ControllerBase
    {
        private readonly IContactPersonServices ContactPersonServices;
        private readonly OtherServices otherServices;

        public ContactPersonController(IContactPersonServices ContactPersonServices, OtherServices otherServices)
        {
            this.ContactPersonServices = ContactPersonServices;
            this.otherServices = otherServices;
        }

        [HttpPost]
        public async Task<ActionResult<ResponseModel<ContactPersonResponseDTO>>> Post(AddContactPersonDTO model)
        {
            if (ModelState.IsValid)
            {
                var Response = ContactPersonServices.AddContactPerson(model);
                return Ok(await Response);
            }
            else
            {
                var Response = new ResponseModel<ContactPersonResponseDTO>()
                {
                    remarks = "Model Not Verified",
                    success = false
                };
                return BadRequest(Response);
            }
        }

        [HttpPut]
        public async Task<ActionResult<ResponseModel<ContactPersonResponseDTO>>> Put(UpdateContactPersonDTO model)
        {
            if (ModelState.IsValid)
            {
                var Response = ContactPersonServices.UpdateContactPerson(model);
                return Ok(await Response);
            }
            else
            {
                var Response = new ResponseModel<ContactPersonResponseDTO>()
                {
                    remarks = "Model Not Verified",
                    success = false
                };
                return BadRequest(Response);
            }
        }

        [HttpGet("GetById")]
        public async Task<ActionResult<ResponseModel<ContactPersonResponseDTO>>> GetById(string id)
        {
            if (otherServices.Check(id))
            {
                var Response = ContactPersonServices.GetContactPersonById(id);
                return Ok(await Response);
            }
            else
            {
                var Response = new ResponseModel<ContactPersonResponseDTO>()
                {
                    remarks = "Contact Person not found by ID",
                    success = false
                };
                return BadRequest(Response);
            }
        }

        [HttpGet]
        public async Task<ActionResult<ResponseModel<List<ContactPersonResponseDTO>>>> Get()
        {
            var ContactPersons = await ContactPersonServices.GetAllContactPersons();
            if (ContactPersons != null)
            {
                var Response = ContactPersons;
                return Ok(Response);
            }
            else
            {
                var Response = new ResponseModel<ContactPersonResponseDTO>()
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
                var Response = ContactPersonServices.DeleteContactPersonById(id);
                return Ok(await Response);
            }
            else
            {
                var Response = new ResponseModel<ContactPersonResponseDTO>()
                {
                    remarks = "Contact Person not found",
                    success = false
                };
                return BadRequest(Response);
            }
        }
    }
}
