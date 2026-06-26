using AutoMapper;
using EMO.Models.DBModels;
using EMO.Models.DBModels.DBTables;
using EMO.Models.DTOs.OfficeDTOs;
using EMO.Models.DTOs.ResponseDTO;
using Microsoft.EntityFrameworkCore;

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

        public async Task<ResponseModel<List<OfficeResponseDTO>>> GetOfficeByBusinessId(string businessId)
        {
            try
            {
                if (string.IsNullOrEmpty(businessId))
                {
                    return new ResponseModel<List<OfficeResponseDTO>>()
                    {
                        remarks = "Invalid Id.",
                        success = false
                    };
                }

                var offices = await db.tbl_office
                    .Include(x => x.business)
                    .Include(x => x.section)
                    .Where(x => x.fk_business == Guid.Parse(businessId) && !x.is_deleted)
                    .ToListAsync();

                if (offices.Any())
                {
                    return new ResponseModel<List<OfficeResponseDTO>>()
                    {
                        data = mapper.Map<List<OfficeResponseDTO>>(offices),
                        remarks = "Success",
                        success = true
                    };
                }

                return new ResponseModel<List<OfficeResponseDTO>>()
                {
                    remarks = "No record Found.",
                    success = false
                };
            }
            catch
            {
                return new ResponseModel<List<OfficeResponseDTO>>()
                {
                    remarks = "There was a fatal error",
                    success = false
                };
            }
        }

        public async Task<ResponseModel<OfficeResponseDTO>> AddOffice(AddOfficeDTO requestDto)
        {
            try
            {
                var existingOffice = await db.tbl_office
                    .Where(x => x.office_name.ToLower() == requestDto.officeName.ToLower()
                             && x.fk_section == Guid.Parse(requestDto.fkSection)
                             && !x.is_deleted)
                    .FirstOrDefaultAsync();

                if (existingOffice != null)
                {
                    return new ResponseModel<OfficeResponseDTO>()
                    {
                        remarks = "Office Already Exists",
                        success = false
                    };
                }

                var newOffice = mapper.Map<tbl_office>(requestDto);
                await db.tbl_office.AddAsync(newOffice);
                await db.SaveChangesAsync();

                var createdOffice = await db.tbl_office
                    .Include(x => x.business)
                    .Include(x => x.section)
                    .FirstOrDefaultAsync(x => x.office_id == newOffice.office_id);

                return new ResponseModel<OfficeResponseDTO>()
                {
                    data = mapper.Map<OfficeResponseDTO>(createdOffice ?? newOffice),
                    remarks = "Success",
                    success = true
                };
            }
            catch
            {
                return new ResponseModel<OfficeResponseDTO>()
                {
                    remarks = "There was a fatal error",
                    success = false
                };
            }
        }

        public async Task<ResponseModel<OfficeResponseDTO>> UpdateOffice(UpdateOfficeDTO requestDto)
        {
            try
            {
                var existingOffice = await db.tbl_office
                    .Where(x => x.office_id == Guid.Parse(requestDto.officeId) && !x.is_deleted)
                    .FirstOrDefaultAsync();

                if (existingOffice == null)
                {
                    return new ResponseModel<OfficeResponseDTO>()
                    {
                        remarks = "No Record Found",
                        success = false
                    };
                }

                mapper.Map(requestDto, existingOffice);
                existingOffice.updated_at = DateTime.Now;
                await db.SaveChangesAsync();

                var updatedOffice = await db.tbl_office
                    .Include(x => x.business)
                    .Include(x => x.section)
                    .FirstOrDefaultAsync(x => x.office_id == existingOffice.office_id);

                return new ResponseModel<OfficeResponseDTO>()
                {
                    data = mapper.Map<OfficeResponseDTO>(updatedOffice ?? existingOffice),
                    remarks = "Success",
                    success = true
                };
            }
            catch
            {
                return new ResponseModel<OfficeResponseDTO>()
                {
                    remarks = "There was a fatal error",
                    success = false
                };
            }
        }

        public async Task<ResponseModel<OfficeResponseDTO>> GetOfficeById(string officeId)
        {
            try
            {
                var office = await db.tbl_office
                    .Include(x => x.business)
                    .Include(x => x.section)
                    .Where(x => x.office_id == Guid.Parse(officeId) && !x.is_deleted)
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

                return new ResponseModel<OfficeResponseDTO>()
                {
                    remarks = "Office not found",
                    success = false
                };
            }
            catch
            {
                return new ResponseModel<OfficeResponseDTO>()
                {
                    remarks = "There was a fatal error",
                    success = false
                };
            }
        }

        public async Task<ResponseModel<List<OfficeResponseDTO>>> GetAllOffices()
        {
            try
            {
                var offices = await db.tbl_office
                    .Include(x => x.business)
                    .Include(x => x.section)
                    .Where(x => !x.is_deleted)
                    .ToListAsync();

                if (offices.Any())
                {
                    return new ResponseModel<List<OfficeResponseDTO>>()
                    {
                        data = mapper.Map<List<OfficeResponseDTO>>(offices),
                        remarks = "Success",
                        success = true
                    };
                }

                return new ResponseModel<List<OfficeResponseDTO>>()
                {
                    remarks = "No Office found",
                    success = false
                };
            }
            catch
            {
                return new ResponseModel<List<OfficeResponseDTO>>()
                {
                    remarks = "There was a fatal error",
                    success = false
                };
            }
        }

        public async Task<ResponseModel> DeleteOfficeById(string officeId)
        {
            try
            {
                var office = await db.tbl_office
                    .Where(x => x.office_id == Guid.Parse(officeId) && !x.is_deleted)
                    .FirstOrDefaultAsync();

                if (office != null)
                {
                    office.is_deleted = true;
                    office.updated_at = DateTime.Now;
                    await db.SaveChangesAsync();

                    return new ResponseModel()
                    {
                        remarks = "Office deleted successfully",
                        success = true
                    };
                }

                return new ResponseModel()
                {
                    remarks = "Office not found",
                    success = false
                };
            }
            catch
            {
                return new ResponseModel()
                {
                    remarks = "There was a fatal error",
                    success = false
                };
            }
        }

        public async Task<ResponseModel<List<OfficeResponseDTO>>> GetOfficeBySectionId(string sectionId)
        {
            try
            {
                var offices = await db.tbl_office
                    .Include(x => x.business)
                    .Include(x => x.section)
                    .Where(x => x.fk_section == Guid.Parse(sectionId) && !x.is_deleted)
                    .ToListAsync();

                if (offices.Any())
                {
                    return new ResponseModel<List<OfficeResponseDTO>>()
                    {
                        data = mapper.Map<List<OfficeResponseDTO>>(offices),
                        remarks = "Success",
                        success = true
                    };
                }

                return new ResponseModel<List<OfficeResponseDTO>>()
                {
                    remarks = "No record found.",
                    success = false
                };
            }
            catch
            {
                return new ResponseModel<List<OfficeResponseDTO>>()
                {
                    remarks = "There was a fatal error",
                    success = false
                };
            }
        }

        public async Task<ResponseModel<List<OfficeResponseDTO>>> GetAvailableOfficesBySectionId(string sectionId)
        {
            try
            {
                var offices = await db.tbl_office
                    .Include(x => x.business)
                    .Include(x => x.section)
                    .Where(x => x.fk_section == Guid.Parse(sectionId)
                             && x.is_occupied == false
                             && !x.is_deleted)
                    .ToListAsync();

                if (offices.Any())
                {
                    return new ResponseModel<List<OfficeResponseDTO>>()
                    {
                        data = mapper.Map<List<OfficeResponseDTO>>(offices),
                        remarks = "Success",
                        success = true
                    };
                }

                return new ResponseModel<List<OfficeResponseDTO>>()
                {
                    remarks = "No record found.",
                    success = false
                };
            }
            catch
            {
                return new ResponseModel<List<OfficeResponseDTO>>()
                {
                    remarks = "There was a fatal error",
                    success = false
                };
            }
        }

        public async Task<ResponseModel<List<OfficeResponseDTO>>> GetAvailableOfficesByBusinessId(string businessId)
        {
            try
            {
                var offices = await db.tbl_office
                    .Include(x => x.business)
                    .Include(x => x.section)
                    .Where(x => x.fk_business == Guid.Parse(businessId)
                             && x.is_occupied == false
                             && !x.is_deleted)
                    .ToListAsync();

                if (offices.Any())
                {
                    return new ResponseModel<List<OfficeResponseDTO>>()
                    {
                        data = mapper.Map<List<OfficeResponseDTO>>(offices),
                        remarks = "Success",
                        success = true
                    };
                }

                return new ResponseModel<List<OfficeResponseDTO>>()
                {
                    remarks = "No record found.",
                    success = false
                };
            }
            catch
            {
                return new ResponseModel<List<OfficeResponseDTO>>()
                {
                    remarks = "There was a fatal error",
                    success = false
                };
            }
        }
    }
}
