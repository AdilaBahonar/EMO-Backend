using AutoMapper;
using P3AHR.Models.DBModels.DBTables;
using P3AHR.Models.DBModels;
using P3AHR.Models.DTOs.ResponseDTO;
using APIProduct.Models.DTOs.UserTypeDTOs;
using APIProduct.Models.DBModels.DBTables;
using Microsoft.EntityFrameworkCore;

namespace APIProduct.Repositories.UserTypeServicesRepo
{
    public class UserTypeService : IUserTypeService
    {
        private readonly DBUserManagementContext db;
        private readonly IMapper mapper;
        public UserTypeService(DBUserManagementContext db, IMapper mapper)
        {
            this.db = db;
            this.mapper = mapper;
        }
        public async Task<ResponseModel<UserTypeResponseDTO>> AddUserType(AddUserTypeDTO requestDto)
        {
            try
            {
                var UserType = await db.tbl_user_type.Where(u => u.user_type_name.ToLower() == requestDto.userTypeName.ToLower()).FirstOrDefaultAsync();
                if (UserType == null)
                {
                    var newUserType = mapper.Map<tbl_user_type>(requestDto);
                    await db.tbl_user_type.AddAsync(newUserType);
                    await db.SaveChangesAsync();
                    return new ResponseModel<UserTypeResponseDTO>()
                    {
                        data = mapper.Map<UserTypeResponseDTO>(newUserType),
                        remarks = "Success",
                        success = true,
                    };
                }
                else
                {
                    return new ResponseModel<UserTypeResponseDTO>()
                    {
                        remarks = "UserType Already Exists",
                        success = false,
                    };
                }
            }
            catch (Exception ex)
            {
                return new ResponseModel<UserTypeResponseDTO>()
                {
                    remarks = $"There was a  fatal error: {ex.ToString()}",
                    success = false,
                };
            }
        }
        public async Task<ResponseModel<UserTypeResponseDTO>> UpdateUserType(UpdateUserTypeDTO requestDto)
        {
            try
            {
                var existingUserType = await db.tbl_user_type.Where(u => u.user_type_id == Guid.Parse(requestDto.userTypeId)).FirstOrDefaultAsync();
                if (existingUserType != null)
                {
                    mapper.Map(requestDto, existingUserType);
                    await db.SaveChangesAsync();
                    return new ResponseModel<UserTypeResponseDTO>()
                    {
                        data = mapper.Map<UserTypeResponseDTO>(existingUserType),
                        remarks = "Success",
                        success = true,
                    };
                }
                else
                {
                    return new ResponseModel<UserTypeResponseDTO>()
                    {
                        remarks = "No Record Found",
                        success = true,
                    };
                }

            }
            catch (Exception ex)
            {
                return new ResponseModel<UserTypeResponseDTO>()
                {
                    remarks = $"There was a  fatal error: {ex.ToString()}",
                    success = false,
                };
            }
        }
        public async Task<ResponseModel<UserTypeResponseDTO>> GetUserTypeById(string UserTypeId)
        {
            try
            {
                var existingUserType = await db.tbl_user_type.Where(u => u.user_type_id == Guid.Parse(UserTypeId)).FirstOrDefaultAsync();
                if (existingUserType != null)
                {
                    return new ResponseModel<UserTypeResponseDTO>()
                    {
                        data = mapper.Map<UserTypeResponseDTO>(existingUserType),
                        remarks = "Success",
                        success = true
                    };
                }
                else
                {
                    return new ResponseModel<UserTypeResponseDTO>()
                    {
                        remarks = "UserType not found",
                        success = false,
                    };
                }
            }
            catch (Exception ex)
            {
                return new ResponseModel<UserTypeResponseDTO>()
                {
                    remarks = $"There was a fatal error {ex.ToString()}",
                    success = false,
                };
            }
        }
        public async Task<ResponseModel<List<UserTypeResponseDTO>>> GetAllUserTypes()
        {
            try
            {
                var allUserType = await db.tbl_user_type.ToListAsync();
                if (allUserType.Any())
                {
                    return new ResponseModel<List<UserTypeResponseDTO>>()
                    {
                        data = mapper.Map<List<UserTypeResponseDTO>>(allUserType),
                        remarks = "Success",
                        success = true
                    };
                }
                else
                {
                    return new ResponseModel<List<UserTypeResponseDTO>>()
                    {
                        remarks = "No UserType found",
                        success = false,
                    };
                }
            }
            catch (Exception ex)
            {
                return new ResponseModel<List<UserTypeResponseDTO>>()
                {
                    remarks = $"There was a fatal error {ex.ToString()}",
                    success = false,
                };
            }
        }
        public async Task<ResponseModel> DeleteUserTypeById(string UserTypeId)
        {
            try
            {
                var existingUserType = await db.tbl_user_type.FindAsync(Guid.Parse(UserTypeId));
                if (existingUserType != null)
                {
                    db.tbl_user_type.Remove(existingUserType); // Mark the entity for deletion
                    await db.SaveChangesAsync();
                    return new ResponseModel()
                    {
                        remarks = "UserType deleted successfully",
                        success = true,
                    };
                }
                else
                {
                    return new ResponseModel()
                    {
                        remarks = "UserType not found",
                        success = false,
                    };
                }
            }
            catch (Exception ex)
            {
                return new ResponseModel()
                {
                    remarks = $"There was a fatal error {ex.ToString()}",
                    success = false,
                };
            }
        }
    }
}
