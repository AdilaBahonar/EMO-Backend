using EMO.Models.DTOs.UserTypeDTOs;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using EMO.Models.DBModels;
using EMO.Models.DTOs.ResponseDTO;
using EMO.Models.DBModels.DBTables;
using EMO.Models.DTOs.SubUserTypeDTOs;

namespace EMO.Repositories.SubUserTypeServicesRepo
{
    public class SubUserTypeServices : ISubUserTypeServices
    {
        private readonly DBUserManagementContext db;
        private readonly IMapper mapper;
        public SubUserTypeServices(DBUserManagementContext db, IMapper mapper)
        {
            this.db = db;
            this.mapper = mapper;
        }
        //public async Task<ResponseModel<SubUserTypeResponseDTO>> AddSubUserType(AddSubUserTypeDTO requestDto)
        //{
        //    try
        //    {
        //        var exists = await db.tbl_sub_user_type
        //            .AnyAsync(s => s.sub_user_type_name.ToLower() == requestDto.subUserTypeName.ToLower());

        //        if (exists)
        //            return new ResponseModel<SubUserTypeResponseDTO>
        //            {
        //                remarks = "SubUserType Already Exists",
        //                success = false
        //            };

        //        var newSubUserType = mapper.Map<tbl_sub_user_type>(requestDto);
        //        newSubUserType.fk_user_type = Guid.Parse(requestDto.fkUserTypeId);

        //        await db.tbl_sub_user_type.AddAsync(newSubUserType);
        //        await db.SaveChangesAsync();

        //        return new ResponseModel<SubUserTypeResponseDTO>
        //        {
        //            data = mapper.Map<SubUserTypeResponseDTO>(newSubUserType),
        //            remarks = "Success",
        //            success = true
        //        };
        //    }
        //    catch (Exception ex)
        //    {
        //        return new ResponseModel<SubUserTypeResponseDTO>
        //        {
        //            remarks = $"There was a fatal error: {ex}",
        //            success = false
        //        };
        //    }
        //}
        public async Task<ResponseModel<SubUserTypeResponseDTO>> AddSubUserType(AddSubUserTypeDTO requestDto)
        {
            try
            {
                // 1. Check if SubUserType with same name already exists
                var exists = await db.tbl_sub_user_type
                    .AnyAsync(s => s.sub_user_type_name.ToLower() == requestDto.subUserTypeName.ToLower() && s.fk_user_type == Guid.Parse(requestDto.fkUserTypeId));

                if (exists)
                {
                    return new ResponseModel<SubUserTypeResponseDTO>
                    {
                        remarks = "Sub User Type Already Exists",
                        success = false
                    };
                }

                // 2. Validate that the provided UserType exists
                if (!Guid.TryParse(requestDto.fkUserTypeId, out Guid userTypeGuid))
                {
                    return new ResponseModel<SubUserTypeResponseDTO>
                    {
                        remarks = "Invalid User Type ID format",
                        success = false
                    };
                }

                var userTypeExists = await db.tbl_user_type.AnyAsync(u => u.user_type_id == userTypeGuid);
                if (!userTypeExists)
                {
                    return new ResponseModel<SubUserTypeResponseDTO>
                    {
                        remarks = "User Type does not exist",
                        success = false
                    };
                }

                // 3. Map DTO to entity
                var newSubUserType = mapper.Map<tbl_sub_user_type>(requestDto);
                newSubUserType.fk_user_type = userTypeGuid;

                // 4. Save to database
                await db.tbl_sub_user_type.AddAsync(newSubUserType);
                await db.SaveChangesAsync();

                // 5. Return success response
                return new ResponseModel<SubUserTypeResponseDTO>
                {
                    data = mapper.Map<SubUserTypeResponseDTO>(newSubUserType),
                    remarks = "Success",
                    success = true
                };
            }
            catch (Exception ex)
            {
                return new ResponseModel<SubUserTypeResponseDTO>
                {
                    remarks = $"There was a fatal error",
                    success = false
                };
            }
        }
        public async Task<ResponseModel<SubUserTypeResponseDTO>> UpdateSubUserType(UpdateSubUserTypeDTO requestDto)
        {
            try
            {
                var existing = await db.tbl_sub_user_type
                    .Include(s => s.user_type)
                    .FirstOrDefaultAsync(s => s.sub_user_type_id == Guid.Parse(requestDto.subUserTypeId));

                if (existing == null)
                    return new ResponseModel<SubUserTypeResponseDTO>
                    {
                        remarks = "No Record Found",
                        success = false
                    };

                mapper.Map(requestDto, existing);
                await db.SaveChangesAsync();

                return new ResponseModel<SubUserTypeResponseDTO>
                {
                    data = mapper.Map<SubUserTypeResponseDTO>(existing),
                    remarks = "Success",
                    success = true
                };
            }
            catch (Exception ex)
            {
                return new ResponseModel<SubUserTypeResponseDTO>
                {
                    remarks = $"There was a fatal error",
                    success = false
                };
            }
        }
        public async Task<ResponseModel<SubUserTypeResponseDTO>> GetSubUserTypeById(string subUserTypeId)
        {
            try
            {
                var existing = await db.tbl_sub_user_type
                    .Include(s => s.user_type)
                    .FirstOrDefaultAsync(s => s.sub_user_type_id == Guid.Parse(subUserTypeId));

                if (existing == null)
                    return new ResponseModel<SubUserTypeResponseDTO>
                    {
                        remarks = "Sub User Type not found",
                        success = false
                    };

                return new ResponseModel<SubUserTypeResponseDTO>
                {
                    data = mapper.Map<SubUserTypeResponseDTO>(existing),
                    remarks = "Success",
                    success = true
                };
            }
            catch (Exception ex)
            {
                return new ResponseModel<SubUserTypeResponseDTO>
                {
                    remarks = $"There was a fatal error",
                    success = false
                };
            }
        }
        public async Task<ResponseModel<List<SubUserTypeResponseDTO>>> GetAllSubUserTypes()
        {
            try
            {
                var all = await db.tbl_sub_user_type
                    .Include(s => s.user_type)
                    .ToListAsync();

                if (!all.Any())
                    return new ResponseModel<List<SubUserTypeResponseDTO>>
                    {
                        remarks = "No Sub User Type found",
                        success = false
                    };

                return new ResponseModel<List<SubUserTypeResponseDTO>>
                {
                    data = mapper.Map<List<SubUserTypeResponseDTO>>(all),
                    remarks = "Success",
                    success = true
                };
            }
            catch (Exception ex)
            {
                return new ResponseModel<List<SubUserTypeResponseDTO>>
                {
                    remarks = $"There was a fatal error",
                    success = false
                };
            }
        }
        public async Task<ResponseModel> DeleteSubUserTypeById(string subUserTypeId)
        {
            try
            {
                var existing = await db.tbl_sub_user_type.FindAsync(Guid.Parse(subUserTypeId));
                if (existing == null)
                    return new ResponseModel
                    {
                        remarks = "Sub User Type not found",
                        success = false
                    };

                db.tbl_sub_user_type.Remove(existing);
                await db.SaveChangesAsync();

                return new ResponseModel
                {
                    remarks = "Sub User Type deleted successfully",
                    success = true
                };
            }
            catch (Exception ex)
            {
                return new ResponseModel
                {
                    remarks = $"There was a fatal error",
                    success = false
                };
            }
        }
        public async Task<ResponseModel<List<SubUserTypeResponseDTO>>> GetSubUserTypesByUserId(string userId)
        {
            try
            {
                // 1️⃣ Get the user to determine the userType
                var user = await db.tbl_user
                    .Where(u => u.user_id == Guid.Parse(userId))
                    .Select(u => u.fk_sub_user_type)
                    .FirstOrDefaultAsync();

                if (user == null)
                {
                    return new ResponseModel<List<SubUserTypeResponseDTO>>
                    {
                        remarks = "Invalid userId",
                        success = false
                    };
                }
                var userType = await db.tbl_sub_user_type.Where(u => u.sub_user_type_id == user).Select(u => u.fk_user_type).FirstOrDefaultAsync();




                // 2️⃣ Fetch sub-user-types for the user's userType
                var subUserTypes = await db.tbl_sub_user_type
                    .Where(s => s.fk_user_type == userType)
                    .Include(s => s.user_type) // optional if you need user_type data in DTO
                    .ToListAsync();

                if (!subUserTypes.Any())
                {
                    return new ResponseModel<List<SubUserTypeResponseDTO>>
                    {
                        remarks = "No Sub User Type found for this user",
                        success = false
                    };
                }

                return new ResponseModel<List<SubUserTypeResponseDTO>>
                {
                    data = mapper.Map<List<SubUserTypeResponseDTO>>(subUserTypes),
                    remarks = "Success",
                    success = true
                };
            }
            catch (Exception ex)
            {
                return new ResponseModel<List<SubUserTypeResponseDTO>>
                {
                    remarks = $"There was a fatal error",
                    success = false
                };
            }
        }

        public async Task<ResponseModel<List<SubUserTypeResponseDTO>>> GetSubUserTypesOfBusiness (string userId)
        {
            try
            {
                // 1️⃣ Get the user to determine the userType
                var user = await db.tbl_user
                    .Where(u => u.user_id == Guid.Parse(userId)).Include(x=>x.sub_user_type).ThenInclude(x=>x.user_type)
                    .FirstOrDefaultAsync();

                if (user == null)
                {
                    return new ResponseModel<List<SubUserTypeResponseDTO>>
                    {
                        remarks = "Invalid userId",
                        success = false
                    };
                }
                if (user.sub_user_type.user_type.user_type_level != 0 && user.sub_user_type.user_type.user_type_level != 1)
                {
                    return new ResponseModel<List<SubUserTypeResponseDTO>>
                    {
                        remarks = "You are not authorized to access this data.",
                        success = false
                    };
                }

                var subUserTypes = await db.tbl_sub_user_type.Where(u => u.user_type.user_type_name.ToLower() == "business admin").ToListAsync();




                //// 2️⃣ Fetch sub-user-types for the user's userType
                //var subUserTypes = await db.tbl_sub_user_type
                //    .Where(s => s.fk_user_type == userType)
                //    .Include(s => s.user_type) // optional if you need user_type data in DTO
                //    .ToListAsync();

                if (!subUserTypes.Any())
                {
                    return new ResponseModel<List<SubUserTypeResponseDTO>>
                    {
                        remarks = "No Sub User Type found for this user",
                        success = false
                    };
                }

                return new ResponseModel<List<SubUserTypeResponseDTO>>
                {
                    data = mapper.Map<List<SubUserTypeResponseDTO>>(subUserTypes),
                    remarks = "Success",
                    success = true
                };
            }
            catch (Exception ex)
            {
                return new ResponseModel<List<SubUserTypeResponseDTO>>
                {
                    remarks = $"There was a fatal error",
                    success = false
                };
            }
        }
        public async Task<ResponseModel> UpdateSubUserTypeHierarchy(List<SubUserTypeHierarchyDTO> requestDto)
        {
            try
            {
                foreach (var dto in requestDto)
                {
                    if (!Guid.TryParse(dto.subUserTypeId, out Guid userTypeId))
                        continue; // skip invalid ids

                    var existingUserType = await db.tbl_sub_user_type
                        .FirstOrDefaultAsync(u => u.sub_user_type_id == userTypeId);

                    if (existingUserType == null)
                        continue; // skip if record not found

                    mapper.Map(dto, existingUserType);
                }

                await db.SaveChangesAsync();

                return new ResponseModel
                {
                    remarks = "Success",
                    success = true
                };
            }
            catch (Exception ex)
            {
                return new ResponseModel
                {
                    remarks = $"There was a fatal error",
                    success = false
                };
            }
        }

    }


}
