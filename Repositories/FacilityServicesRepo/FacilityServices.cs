using EMO.Models.DBModels.DBTables;
using EMO.Models.DTOs.FacilityDTOs;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using EMO.Models.DBModels;
using EMO.Models.DTOs.ResponseDTO;

namespace EMO.Repositories.FacilityServicesRepo
{
    public class FacilityServices : IFacilityServices
    {
        private readonly DBUserManagementContext db;
        private readonly IMapper mapper;

        public FacilityServices(DBUserManagementContext db, IMapper mapper)
        {
            this.db = db;
            this.mapper = mapper;
        }

        public async Task<ResponseModel<FacilityResponseDTO>> AddFacility(AddFacilityDTO requestDto)
        {
            try
            {
                var existingFacility = await db.tbl_facility
                    .Where(x => x.facility_name.ToLower() == requestDto.facilityName.ToLower())
                    .FirstOrDefaultAsync();

                if (existingFacility == null)
                {
                    var newFacility = mapper.Map<tbl_facility>(requestDto);
                    await db.tbl_facility.AddAsync(newFacility);
                    await db.SaveChangesAsync();

                    return new ResponseModel<FacilityResponseDTO>()
                    {
                        data = mapper.Map<FacilityResponseDTO>(newFacility),
                        remarks = "Success",
                        success = true
                    };
                }
                else
                {
                    return new ResponseModel<FacilityResponseDTO>()
                    {
                        remarks = "Facility Already Exists",
                        success = false
                    };
                }
            }
            catch (Exception ex)
            {
                return new ResponseModel<FacilityResponseDTO>()
                {
                    remarks = $"There was a fatal error: {ex}",
                    success = false
                };
            }
        }

        public async Task<ResponseModel<FacilityResponseDTO>> UpdateFacility(UpdateFacilityDTO requestDto)
        {
            try
            {
                var existingFacility = await db.tbl_facility
                    .Where(x => x.facility_id == Guid.Parse(requestDto.facilityId))
                    .FirstOrDefaultAsync();

                if (existingFacility != null)
                {
                    mapper.Map(requestDto, existingFacility);
                    existingFacility.updated_at = DateTime.Now;
                    await db.SaveChangesAsync();

                    return new ResponseModel<FacilityResponseDTO>()
                    {
                        data = mapper.Map<FacilityResponseDTO>(existingFacility),
                        remarks = "Success",
                        success = true
                    };
                }
                else
                {
                    return new ResponseModel<FacilityResponseDTO>()
                    {
                        remarks = "No Record Found",
                        success = false
                    };
                }
            }
            catch (Exception ex)
            {
                return new ResponseModel<FacilityResponseDTO>()
                {
                    remarks = $"There was a fatal error: {ex}",
                    success = false
                };
            }
        }

        public async Task<ResponseModel<FacilityResponseDTO>> GetFacilityById(string facilityId)
        {
            try
            {
                var facility = await db.tbl_facility
                    .Include(x => x.business)
                    .Where(x => x.facility_id == Guid.Parse(facilityId))
                    .FirstOrDefaultAsync();

                if (facility != null)
                {
                    return new ResponseModel<FacilityResponseDTO>()
                    {
                        data = mapper.Map<FacilityResponseDTO>(facility),
                        remarks = "Success",
                        success = true
                    };
                }
                else
                {
                    return new ResponseModel<FacilityResponseDTO>()
                    {
                        remarks = "Facility not found",
                        success = false
                    };
                }
            }
            catch (Exception ex)
            {
                return new ResponseModel<FacilityResponseDTO>()
                {
                    remarks = $"There was a fatal error: {ex}",
                    success = false
                };
            }
        }
        public async Task<ResponseModel<List<FacilityResponseDTO>>> GetFacilityByBusinessId(string businessId)
        {
            try
            {
                var facility = await db.tbl_facility
                    .Include(x => x.business)
                    .Where(x => x.fk_business == Guid.Parse(businessId))
                    .ToListAsync();

                if (facility.Any())
                {
                    return new ResponseModel<List<FacilityResponseDTO>>()
                    {
                        data = mapper.Map<List<FacilityResponseDTO>>(facility),
                        remarks = "Success",
                        success = true
                    };
                }
                else
                {
                    return new ResponseModel<List<FacilityResponseDTO>>()
                    {
                        remarks = "Facility not found",
                        success = false
                    };
                }
            }
            catch (Exception ex)
            {
                return new ResponseModel<List<FacilityResponseDTO>>()
                {
                    remarks = $"There was a fatal error: {ex}",
                    success = false
                };
            }
        }
        public async Task<ResponseModel<List<FacilityResponseDTO>>> GetAllFacilities()
        {
            try
            {
                var facilities = await db.tbl_facility
                    .Include(x => x.business)
                    .ToListAsync();

                if (facilities.Any())
                {
                    return new ResponseModel<List<FacilityResponseDTO>>()
                    {
                        data = mapper.Map<List<FacilityResponseDTO>>(facilities),
                        remarks = "Success",
                        success = true
                    };
                }
                else
                {
                    return new ResponseModel<List<FacilityResponseDTO>>()
                    {
                        remarks = "No Facility found",
                        success = false
                    };
                }
            }
            catch (Exception ex)
            {
                return new ResponseModel<List<FacilityResponseDTO>>()
                {
                    remarks = $"There was a fatal error: {ex}",
                    success = false
                };
            }
        }

        public async Task<ResponseModel> DeleteFacilityById(string facilityId)
        {
            try
            {
                var facility = await db.tbl_facility.FindAsync(Guid.Parse(facilityId));

                if (facility != null)
                {
                    db.tbl_facility.Remove(facility);
                    await db.SaveChangesAsync();

                    return new ResponseModel()
                    {
                        remarks = "Facility deleted successfully",
                        success = true
                    };
                }
                else
                {
                    return new ResponseModel()
                    {
                        remarks = "Facility not found",
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
