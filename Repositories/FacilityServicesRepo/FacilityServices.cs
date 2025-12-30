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
                    .Where(f => f.facility_name.ToLower() == requestDto.facilityName.ToLower())
                    .FirstOrDefaultAsync();

                if (existingFacility == null)
                {
                    var newFacility = mapper.Map<tbl_facility>(requestDto);

                    await db.tbl_facility.AddAsync(newFacility);
                    await db.SaveChangesAsync();

                    var response = mapper.Map<FacilityResponseDTO>(newFacility);

                    return new ResponseModel<FacilityResponseDTO>()
                    {
                        data = response,
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
                    .Where(f => f.facility_id == Guid.Parse(requestDto.facilityId))
                    .FirstOrDefaultAsync();

                if (existingFacility != null)
                {
                    mapper.Map(requestDto, existingFacility);

                    await db.SaveChangesAsync();

                    var response = mapper.Map<FacilityResponseDTO>(existingFacility);

                    return new ResponseModel<FacilityResponseDTO>()
                    {
                        data = response,
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
                    .Include(f => f.business)
                    .Where(f => f.facility_id == Guid.Parse(facilityId))
                    .FirstOrDefaultAsync();

                if (facility != null)
                {
                    var response = mapper.Map<FacilityResponseDTO>(facility);

                    return new ResponseModel<FacilityResponseDTO>()
                    {
                        data = response,
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

        public async Task<ResponseModel<List<FacilityResponseDTO>>> GetAllFacilities()
        {
            try
            {
                var facilities = await db.tbl_facility.Include(f => f.business).ToListAsync();

                if (facilities.Any())
                {
                    var responseList = mapper.Map<List<FacilityResponseDTO>>(facilities);

                    return new ResponseModel<List<FacilityResponseDTO>>()
                    {
                        data = responseList,
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
