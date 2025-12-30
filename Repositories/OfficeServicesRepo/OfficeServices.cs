using EMO.Models.DBModels.DBTables;
using EMO.Models.DTOs.OfficeDTOs;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using EMO.Models.DBModels;
using EMO.Models.DTOs.ResponseDTO;

namespace EMO.Repositories.OfficeServicesRepo
{
    public class OfficeServices : IOfficeServices
    {
        private readonly DBUserManagementContext db;
        private readonly IMapper mapper;

        public OfficeServices(DBUserManagementContext db, IMapper mapper)
        {
            this.db = db;
            this.mapper = mapper;
        }

        public async Task<ResponseModel<OfficeResponseDTO>> AddOffice(AddOfficeDTO requestDto)
        {
            try
            {
                var existingOffice = await db.tbl_office
                    .Where(o => o.office_name.ToLower() == requestDto.officeName.ToLower())
                    .FirstOrDefaultAsync();

                if (existingOffice == null)
                {
                    var newOffice = mapper.Map<tbl_office>(requestDto);
                    await db.tbl_office.AddAsync(newOffice);
                    await db.SaveChangesAsync();

                    return new ResponseModel<OfficeResponseDTO>()
                    {
                        data = mapper.Map<OfficeResponseDTO>(newOffice),
                        remarks = "Success",
                        success = true
                    };
                }
                else
                {
                    return new ResponseModel<OfficeResponseDTO>()
                    {
                        remarks = "Office Already Exists",
                        success = false
                    };
                }
            }
            catch (Exception ex)
            {
                return new ResponseModel<OfficeResponseDTO>()
                {
                    remarks = $"There was a fatal error: {ex}",
                    success = false
                };
            }
        }

        public async Task<ResponseModel<OfficeResponseDTO>> UpdateOffice(UpdateOfficeDTO requestDto)
        {
            try
            {
                var existingOffice = await db.tbl_office
                    .Where(o => o.office_id == Guid.Parse(requestDto.officeId))
                    .FirstOrDefaultAsync();

                if (existingOffice != null)
                {
                    mapper.Map(requestDto, existingOffice);
                    await db.SaveChangesAsync();

                    return new ResponseModel<OfficeResponseDTO>()
                    {
                        data = mapper.Map<OfficeResponseDTO>(existingOffice),
                        remarks = "Success",
                        success = true
                    };
                }
                else
                {
                    return new ResponseModel<OfficeResponseDTO>()
                    {
                        remarks = "No Record Found",
                        success = false
                    };
                }
            }
            catch (Exception ex)
            {
                return new ResponseModel<OfficeResponseDTO>()
                {
                    remarks = $"There was a fatal error: {ex}",
                    success = false
                };
            }
        }

        public async Task<ResponseModel<OfficeResponseDTO>> GetOfficeById(string officeId)
        {
            try
            {
                var office = await db.tbl_office
                    .Where(o => o.office_id == Guid.Parse(officeId))
                    .FirstOrDefaultAsync();

                if (office != null)
                {
                    return new ResponseModel<OfficeResponseDTO>()
                    {
                        data = mapper.Map<OfficeResponseDTO>(office),
                        remarks = "Success",
                        success = true
                    };
                }
                else
                {
                    return new ResponseModel<OfficeResponseDTO>()
                    {
                        remarks = "Office not found",
                        success = false
                    };
                }
            }
            catch (Exception ex)
            {
                return new ResponseModel<OfficeResponseDTO>()
                {
                    remarks = $"There was a fatal error: {ex}",
                    success = false
                };
            }
        }

        public async Task<ResponseModel<List<OfficeResponseDTO>>> GetAllOffices()
        {
            try
            {
                var offices = await db.tbl_office.ToListAsync();

                if (offices.Any())
                {
                    return new ResponseModel<List<OfficeResponseDTO>>()
                    {
                        data = mapper.Map<List<OfficeResponseDTO>>(offices),
                        remarks = "Success",
                        success = true
                    };
                }
                else
                {
                    return new ResponseModel<List<OfficeResponseDTO>>()
                    {
                        remarks = "No Office found",
                        success = false
                    };
                }
            }
            catch (Exception ex)
            {
                return new ResponseModel<List<OfficeResponseDTO>>()
                {
                    remarks = $"There was a fatal error: {ex}",
                    success = false
                };
            }
        }

        public async Task<ResponseModel> DeleteOfficeById(string officeId)
        {
            try
            {
                var office = await db.tbl_office.FindAsync(Guid.Parse(officeId));

                if (office != null)
                {
                    db.tbl_office.Remove(office);
                    await db.SaveChangesAsync();

                    return new ResponseModel()
                    {
                        remarks = "Office deleted successfully",
                        success = true
                    };
                }
                else
                {
                    return new ResponseModel()
                    {
                        remarks = "Office not found",
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
