using EMO.Models.DBModels.DBTables;
using EMO.Models.DTOs.SubTypeDTOs;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using EMO.Models.DBModels;
using EMO.Models.DTOs.ResponseDTO;
using EMO.Models.DTOs.SubTypeDTOs.EMO.Models.DTOs.SubTypeDTOs;
using P3AHR.Models.DTOs.ResponseDTO;

namespace EMO.Repositories.SubTypeServicesRepo
{
    public class SubTypeServices : ISubTypeServices
    {
        private readonly DBUserManagementContext db;
        private readonly IMapper mapper;

        public SubTypeServices(DBUserManagementContext db, IMapper mapper)
        {
            this.db = db;
            this.mapper = mapper;
        }

        public async Task<ResponseModel<SubTypeResponseDTO>> AddSubType(AddSubTypeDTO requestDto)
        {
            try
            {
                var existingSubType = await db.tbl_sub_type
                    .Where(x => x.sub_type_name.ToLower() == requestDto.subTypeName.ToLower())
                    .FirstOrDefaultAsync();

                if (existingSubType == null)
                {
                    var newSubType = mapper.Map<tbl_sub_type>(requestDto);
                    await db.tbl_sub_type.AddAsync(newSubType);
                    await db.SaveChangesAsync();

                    return new ResponseModel<SubTypeResponseDTO>()
                    {
                        data = mapper.Map<SubTypeResponseDTO>(newSubType),
                        remarks = "Success",
                        success = true
                    };
                }
                else
                {
                    return new ResponseModel<SubTypeResponseDTO>()
                    {
                        remarks = "Sub Type Already Exists",
                        success = false
                    };
                }
            }
            catch (Exception ex)
            {
                return new ResponseModel<SubTypeResponseDTO>()
                {
                    remarks = $"There was a fatal error: {ex}",
                    success = false
                };
            }
        }

        public async Task<ResponseModel<SubTypeResponseDTO>> UpdateSubType(UpdateSubTypeDTO requestDto)
        {
            try
            {
                var existingSubType = await db.tbl_sub_type
                    .Where(x => x.sub_type_id == Guid.Parse(requestDto.subTypeId))
                    .FirstOrDefaultAsync();

                if (existingSubType != null)
                {
                    mapper.Map(requestDto, existingSubType);
                    await db.SaveChangesAsync();

                    return new ResponseModel<SubTypeResponseDTO>()
                    {
                        data = mapper.Map<SubTypeResponseDTO>(existingSubType),
                        remarks = "Success",
                        success = true
                    };
                }
                else
                {
                    return new ResponseModel<SubTypeResponseDTO>()
                    {
                        remarks = "No Record Found",
                        success = false
                    };
                }
            }
            catch (Exception ex)
            {
                return new ResponseModel<SubTypeResponseDTO>()
                {
                    remarks = $"There was a fatal error: {ex}",
                    success = false
                };
            }
        }

        public async Task<ResponseModel<SubTypeResponseDTO>> GetSubTypeById(string subTypeId)
        {
            try
            {
                var subType = await db.tbl_sub_type
                    .Where(x => x.sub_type_id == Guid.Parse(subTypeId))
                    .FirstOrDefaultAsync();

                if (subType != null)
                {
                    return new ResponseModel<SubTypeResponseDTO>()
                    {
                        data = mapper.Map<SubTypeResponseDTO>(subType),
                        remarks = "Success",
                        success = true
                    };
                }
                else
                {
                    return new ResponseModel<SubTypeResponseDTO>()
                    {
                        remarks = "Sub Type not found",
                        success = false
                    };
                }
            }
            catch (Exception ex)
            {
                return new ResponseModel<SubTypeResponseDTO>()
                {
                    remarks = $"There was a fatal error: {ex}",
                    success = false
                };
            }
        }

        public async Task<ResponseModel<List<SubTypeResponseDTO>>> GetAllSubTypes()
        {
            try
            {
                var subTypes = await db.tbl_sub_type.ToListAsync();

                if (subTypes.Any())
                {
                    return new ResponseModel<List<SubTypeResponseDTO>>()
                    {
                        data = mapper.Map<List<SubTypeResponseDTO>>(subTypes),
                        remarks = "Success",
                        success = true
                    };
                }
                else
                {
                    return new ResponseModel<List<SubTypeResponseDTO>>()
                    {
                        remarks = "No Sub Type found",
                        success = false
                    };
                }
            }
            catch (Exception ex)
            {
                return new ResponseModel<List<SubTypeResponseDTO>>()
                {
                    remarks = $"There was a fatal error: {ex}",
                    success = false
                };
            }
        }

        public async Task<ResponseModel> DeleteSubTypeById(string subTypeId)
        {
            try
            {
                var subType = await db.tbl_sub_type.FindAsync(Guid.Parse(subTypeId));

                if (subType != null)
                {
                    db.tbl_sub_type.Remove(subType);
                    await db.SaveChangesAsync();

                    return new ResponseModel()
                    {
                        remarks = "Sub Type deleted successfully",
                        success = true
                    };
                }
                else
                {
                    return new ResponseModel()
                    {
                        remarks = "Sub Type not found",
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
