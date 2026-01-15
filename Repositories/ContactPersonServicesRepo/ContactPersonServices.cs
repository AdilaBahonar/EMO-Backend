using EMO.Models.DBModels.DBTables;
using EMO.Models.DTOs.ContactPersonDTOs;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using EMO.Models.DBModels;
using EMO.Models.DTOs.ResponseDTO;
using P3AHR.Models.DTOs.ResponseDTO;

namespace EMO.Repositories.ContactPersonServicesRepo
{
    public class ContactPersonServices : IContactPersonServices
    {
        private readonly DBUserManagementContext db;
        private readonly IMapper mapper;

        public ContactPersonServices(DBUserManagementContext db, IMapper mapper)
        {
            this.db = db;
            this.mapper = mapper;
        }

        public async Task<ResponseModel<ContactPersonResponseDTO>> AddContactPerson(AddContactPersonDTO requestDto)
        {
            try
            {
                var newContactPerson = mapper.Map<tbl_contact_person>(requestDto);
                await db.tbl_contact_person.AddAsync(newContactPerson);
                await db.SaveChangesAsync();

                return new ResponseModel<ContactPersonResponseDTO>()
                {
                    data = mapper.Map<ContactPersonResponseDTO>(newContactPerson),
                    remarks = "Success",
                    success = true
                };
            }
            catch (Exception ex)
            {
                return new ResponseModel<ContactPersonResponseDTO>()
                {
                    remarks = $"There was a fatal error: {ex}",
                    success = false
                };
            }
        }

        public async Task<ResponseModel<ContactPersonResponseDTO>> UpdateContactPerson(UpdateContactPersonDTO requestDto)
        {
            try
            {
                var existingPerson = await db.tbl_contact_person
                    .Where(x => x.contact_person_id == Guid.Parse(requestDto.contactPersonId))
                    .FirstOrDefaultAsync();

                if (existingPerson != null)
                {
                    mapper.Map(requestDto, existingPerson);
                    await db.SaveChangesAsync();

                    return new ResponseModel<ContactPersonResponseDTO>()
                    {
                        data = mapper.Map<ContactPersonResponseDTO>(existingPerson),
                        remarks = "Success",
                        success = true
                    };
                }
                else
                {
                    return new ResponseModel<ContactPersonResponseDTO>()
                    {
                        remarks = "No Record Found",
                        success = false
                    };
                }
            }
            catch (Exception ex)
            {
                return new ResponseModel<ContactPersonResponseDTO>()
                {
                    remarks = $"There was a fatal error: {ex}",
                    success = false
                };
            }
        }

        public async Task<ResponseModel<ContactPersonResponseDTO>> GetContactPersonById(string contactPersonId)
        {
            try
            {
                var person = await db.tbl_contact_person
                    .Where(x => x.contact_person_id == Guid.Parse(contactPersonId))
                    .FirstOrDefaultAsync();

                if (person != null)
                {
                    return new ResponseModel<ContactPersonResponseDTO>()
                    {
                        data = mapper.Map<ContactPersonResponseDTO>(person),
                        remarks = "Success",
                        success = true
                    };
                }
                else
                {
                    return new ResponseModel<ContactPersonResponseDTO>()
                    {
                        remarks = "Contact Person not found",
                        success = false
                    };
                }
            }
            catch (Exception ex)
            {
                return new ResponseModel<ContactPersonResponseDTO>()
                {
                    remarks = $"There was a fatal error: {ex}",
                    success = false
                };
            }
        }

        public async Task<ResponseModel<List<ContactPersonResponseDTO>>> GetAllContactPersons()
        {
            try
            {
                var persons = await db.tbl_contact_person.ToListAsync();

                if (persons.Any())
                {
                    return new ResponseModel<List<ContactPersonResponseDTO>>()
                    {
                        data = mapper.Map<List<ContactPersonResponseDTO>>(persons),
                        remarks = "Success",
                        success = true
                    };
                }
                else
                {
                    return new ResponseModel<List<ContactPersonResponseDTO>>()
                    {
                        remarks = "No Contact Person found",
                        success = false
                    };
                }
            }
            catch (Exception ex)
            {
                return new ResponseModel<List<ContactPersonResponseDTO>>()
                {
                    remarks = $"There was a fatal error: {ex}",
                    success = false
                };
            }
        }

        public async Task<ResponseModel> DeleteContactPersonById(string contactPersonId)
        {
            try
            {
                var person = await db.tbl_contact_person.FindAsync(Guid.Parse(contactPersonId));

                if (person != null)
                {
                    db.tbl_contact_person.Remove(person);
                    await db.SaveChangesAsync();

                    return new ResponseModel()
                    {
                        remarks = "Contact Person deleted successfully",
                        success = true
                    };
                }
                else
                {
                    return new ResponseModel()
                    {
                        remarks = "Contact Person not found",
                        success = false
                    };
                }
            }
            catch (Exception ex)
            {
                return new ResponseModel()
                {
                    remarks = $"There was a fatal error: {ex}",
                    success = false
                };
            }
        }
    }
}
