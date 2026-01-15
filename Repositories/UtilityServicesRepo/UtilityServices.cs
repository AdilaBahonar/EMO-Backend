using EMO.Models.DBModels.DBTables;
using EMO.Models.DTOs.UtilityDTOs;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using EMO.Models.DBModels;
using EMO.Models.DTOs.ResponseDTO;
using EMO.Models.DTOs.UtilityDTOs.EMO.Models.DTOs.UtilityDTOs;
using P3AHR.Models.DTOs.ResponseDTO;

namespace EMO.Repositories.UtilityServicesRepo
{
    public class UtilityServices : IUtilityServices
    {
        private readonly DBUserManagementContext db;
        private readonly IMapper mapper;

        public UtilityServices(DBUserManagementContext db, IMapper mapper)
        {
            this.db = db;
            this.mapper = mapper;
        }

        public async Task<ResponseModel<UtilityResponseDTO>> AddUtility(AddUtilityDTO requestDto)
        {
            try
            {
                var existingUtility = await db.tbl_utility
                    .Where(x => x.utility_name.ToLower() == requestDto.utilityName.ToLower())
                    .FirstOrDefaultAsync();

                if (existingUtility == null)
                {
                    var newUtility = mapper.Map<tbl_utility>(requestDto);
                    await db.tbl_utility.AddAsync(newUtility);
                    await db.SaveChangesAsync();

                    return new ResponseModel<UtilityResponseDTO>()
                    {
                        data = mapper.Map<UtilityResponseDTO>(newUtility),
                        remarks = "Success",
                        success = true
                    };
                }
                else
                {
                    return new ResponseModel<UtilityResponseDTO>()
                    {
                        remarks = "Utility Already Exists",
                        success = false
                    };
                }
            }
            catch (Exception ex)
            {
                return new ResponseModel<UtilityResponseDTO>()
                {
                    remarks = $"There was a fatal error: {ex}",
                    success = false
                };
            }
        }

        public async Task<ResponseModel<UtilityResponseDTO>> UpdateUtility(UpdateUtilityDTO requestDto)
        {
            try
            {
                var existingUtility = await db.tbl_utility
                    .Where(x => x.utility_id == Guid.Parse(requestDto.utilityId))
                    .FirstOrDefaultAsync();

                if (existingUtility != null)
                {
                    mapper.Map(requestDto, existingUtility);
                    await db.SaveChangesAsync();

                    return new ResponseModel<UtilityResponseDTO>()
                    {
                        data = mapper.Map<UtilityResponseDTO>(existingUtility),
                        remarks = "Success",
                        success = true
                    };
                }
                else
                {
                    return new ResponseModel<UtilityResponseDTO>()
                    {
                        remarks = "No Record Found",
                        success = false
                    };
                }
            }
            catch (Exception ex)
            {
                return new ResponseModel<UtilityResponseDTO>()
                {
                    remarks = $"There was a fatal error: {ex}",
                    success = false
                };
            }
        }

        public async Task<ResponseModel<UtilityResponseDTO>> GetUtilityById(string utilityId)
        {
            try
            {
                var utility = await db.tbl_utility
                    .Where(x => x.utility_id == Guid.Parse(utilityId))
                    .FirstOrDefaultAsync();

                if (utility != null)
                {
                    return new ResponseModel<UtilityResponseDTO>()
                    {
                        data = mapper.Map<UtilityResponseDTO>(utility),
                        remarks = "Success",
                        success = true
                    };
                }
                else
                {
                    return new ResponseModel<UtilityResponseDTO>()
                    {
                        remarks = "Utility not found",
                        success = false
                    };
                }
            }
            catch (Exception ex)
            {
                return new ResponseModel<UtilityResponseDTO>()
                {
                    remarks = $"There was a fatal error: {ex}",
                    success = false
                };
            }
        }

        public async Task<ResponseModel<List<UtilityResponseDTO>>> GetAllUtilities()
        {
            try
            {
                var utilities = await db.tbl_utility.ToListAsync();

                if (utilities.Any())
                {
                    return new ResponseModel<List<UtilityResponseDTO>>()
                    {
                        data = mapper.Map<List<UtilityResponseDTO>>(utilities),
                        remarks = "Success",
                        success = true
                    };
                }
                else
                {
                    return new ResponseModel<List<UtilityResponseDTO>>()
                    {
                        remarks = "No Utility found",
                        success = false
                    };
                }
            }
            catch (Exception ex)
            {
                return new ResponseModel<List<UtilityResponseDTO>>()
                {
                    remarks = $"There was a fatal error: {ex}",
                    success = false
                };
            }
        }

        public async Task<ResponseModel> DeleteUtilityById(string utilityId)
        {
            try
            {
                var utility = await db.tbl_utility.FindAsync(Guid.Parse(utilityId));

                if (utility != null)
                {
                    db.tbl_utility.Remove(utility);
                    await db.SaveChangesAsync();

                    return new ResponseModel()
                    {
                        remarks = "Utility deleted successfully",
                        success = true
                    };
                }
                else
                {
                    return new ResponseModel()
                    {
                        remarks = "Utility not found",
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
