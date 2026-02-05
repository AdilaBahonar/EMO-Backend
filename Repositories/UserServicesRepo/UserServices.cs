using AutoMapper;
using EMO.Models.DBModels;
using EMO.Models.DBModels.DBTables;
using EMO.Models.DTOs.ResponseDTO;
using EMO.Models.DTOs.UserDTOs;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace EMO.Repositories.UserServicesRepo
{
    public class UserServices : IUserServices
    {
        private readonly DBUserManagementContext db;
        private readonly IMapper mapper;

        public UserServices(DBUserManagementContext db, IMapper mapper)
        {
            this.db = db;
            this.mapper = mapper;
        }

        public async Task<ResponseModel<UserResponseDTO>> AddUser(AddUserDTO requestDto)
        {
            try
            {
                var user = await db.tbl_user.Where(u => u.user_name.ToLower() == requestDto.userName.ToLower()).FirstOrDefaultAsync();
                if (user == null)
                {
                    var newUser = mapper.Map<tbl_user>(requestDto);
                    await db.tbl_user.AddAsync(newUser);
                    await db.SaveChangesAsync();

                    if (!string.IsNullOrEmpty(requestDto.imageBase64))
                    {
                        var userImage = new tbl_user_image
                        {
                            fk_user= newUser.user_id,
                            imageBase64 = requestDto.imageBase64
                        };

                        await db.tbl_user_image.AddAsync(userImage);
                        await db.SaveChangesAsync();
                    }
                    return new ResponseModel<UserResponseDTO>()
                    {
                        data = mapper.Map<UserResponseDTO>(newUser),
                        remarks = "Success",
                        success = true,
                    };
                }
                else
                {
                    return new ResponseModel<UserResponseDTO>()
                    {
                        remarks = "User Already Exists",
                        success = false,
                    };
                }
            }
            catch (Exception ex)
            {
                return new ResponseModel<UserResponseDTO>()
                {
                    remarks = $"There was a  fatal error: {ex.ToString()}",
                    success = false,
                };
            }
        }
        public async Task<ResponseModel<UserResponseDTO>> UpdateUser(UpdateUserDTO requestDto)
        {
            try
            {
                var existingUser = await db.tbl_user.Where(u => u.user_id == Guid.Parse(requestDto.userId)).Include(u => u.user_image).FirstOrDefaultAsync();
                if (existingUser != null)
                {
                    mapper.Map(requestDto, existingUser);
                    if (requestDto.isImageChanged && !string.IsNullOrEmpty(requestDto.imageBase64))
                    {
                        if (existingUser.user_image != null)
                        {
                            // Update existing image
                            existingUser.user_image.imageBase64 = requestDto.imageBase64;
                        }
                        else
                        {
                            // Create new image row if it doesn't exist
                            var newUserImage = new tbl_user_image
                            {
                                fk_user= existingUser.user_id,
                                imageBase64 = requestDto.imageBase64
                            };
                            await db.tbl_user_image.AddAsync(newUserImage);
                        }
                    }
                    await db.SaveChangesAsync();
                    return new ResponseModel<UserResponseDTO>()
                    {
                        data = mapper.Map<UserResponseDTO>(existingUser),
                        remarks = "Success",
                        success = true,
                    };
                }
                else
                {
                    return new ResponseModel<UserResponseDTO>()
                    {
                        remarks = "No Record Found",
                        success = true,
                    };
                }

            }
            catch (Exception ex)
            {
                return new ResponseModel<UserResponseDTO>()
                {
                    remarks = $"There was a  fatal error: {ex.ToString()}",
                    success = false,
                };
            }
        }
        public async Task<ResponseModel<UserResponseDTO>> GetUserById(string userId)
        {
            try
            {
                var existingUser = await db.tbl_user.Include(u => u.sub_user_type).Where(u => u.user_id == Guid.Parse(userId)).Include(u => u.user_image).Include(u => u.gender).FirstOrDefaultAsync();
                if (existingUser != null)
                {
                    return new ResponseModel<UserResponseDTO>()
                    {
                        data = mapper.Map<UserResponseDTO>(existingUser),
                        remarks = "Success",
                        success = true
                    };
                }
                else
                {
                    return new ResponseModel<UserResponseDTO>()
                    {
                        remarks = "User not found",
                        success = false,
                    };
                }
            }
            catch (Exception ex)
            {
                return new ResponseModel<UserResponseDTO>()
                {
                    remarks = $"There was a fatal error {ex.ToString()}",
                    success = false,
                };
            }
        }

        public async Task<ResponseModel<List<UserResponseDTO>>> GetAllUsers()
        {
            try
            {
                var allUser = await db.tbl_user.Include(u => u.sub_user_type).Include(u => u.user_image).Include(u => u.gender).ToListAsync();
                if (allUser.Any())
                {
                    return new ResponseModel<List<UserResponseDTO>>()
                    {
                        data = mapper.Map<List<UserResponseDTO>>(allUser),
                        remarks = "Success",
                        success = true
                    };
                }
                else
                {
                    return new ResponseModel<List<UserResponseDTO>>()
                    {
                        remarks = "No User found",
                        success = false,
                    };
                }
            }
            catch (Exception ex)
            {
                return new ResponseModel<List<UserResponseDTO>>()
                {
                    remarks = $"There was a fatal error {ex.ToString()}",
                    success = false,
                };
            }
        }
        public async Task<ResponseModel<List<UserResponseDTO>>> GetByUserTypeId(string userTypeId)
        {
            try
            {
                var allUser = await db.tbl_user.Include(u => u.sub_user_type).Where(x => x.fk_sub_user_type == Guid.Parse(userTypeId)).Include(u => u.user_image).Include(u => u.gender).ToListAsync();
                if (allUser.Any())
                {
                    return new ResponseModel<List<UserResponseDTO>>()
                    {
                        data = mapper.Map<List<UserResponseDTO>>(allUser),
                        remarks = "Success",
                        success = true
                    };
                }
                else
                {
                    return new ResponseModel<List<UserResponseDTO>>()
                    {
                        remarks = "No User found By This User Type",
                        success = false,
                    };
                }
            }
            catch (Exception ex)
            {
                return new ResponseModel<List<UserResponseDTO>>()
                {
                    remarks = $"There was a fatal error {ex.ToString()}",
                    success = false,
                };
            }
        }

        public async Task<ResponseModel<List<UserResponseDTO>>> GetBusinessAdmins(string userId)
        {
            try
            {

                var user = await db.tbl_user.Where(x => x.user_id == Guid.Parse(userId) && x.sub_user_type.user_type.user_type_name == "system admin").FirstOrDefaultAsync();
                if(user== null)
                {
                    return new ResponseModel<List<UserResponseDTO>>()
                    {
                        remarks = "You are not authorized to access business admins list.",
                        success = false,
                    };
                }
                var BuisnessAdmins = await db.tbl_user.Where(x => x.sub_user_type.user_type.user_type_name.ToLower() == "business admin" && x.sub_user_type.sub_user_type_name.ToLower() == "root").ToListAsync();

/*                var allUser = await db.tbl_user.Include(u => u.sub_user_type).Where(x => x.fk_sub_user_type == Guid.Parse(userTypeId)).Include(u => u.user_image).Include(u => u.gender).ToListAsync();
*/                if (BuisnessAdmins.Any())
                {
                    return new ResponseModel<List<UserResponseDTO>>()
                    {
                        data = mapper.Map<List<UserResponseDTO>>(BuisnessAdmins),
                        remarks = "Success",
                        success = true
                    };
                }
                else
                {
                    return new ResponseModel<List<UserResponseDTO>>()
                    {
                        remarks = "No User found By This User Type",
                        success = false,
                    };
                }
            }
            catch (Exception ex)
            {
                return new ResponseModel<List<UserResponseDTO>>()
                {
                    remarks = $"There was a fatal error {ex.ToString()}",
                    success = false,
                };
            }
        }
        public async Task<ResponseModel<List<UserResponseDTO>>> GetUnderUsersByUserId(string userId)
        {
            try
            {
                var userGuid = Guid.Parse(userId);

                var currentUserInfo = await db.tbl_user.Where(u => u.user_id == userGuid).Select(u => new{
                        SubUserTypeId = u.fk_sub_user_type,  SubUserTypeLevel = u.sub_user_type.sub_user_type_level,UserTypelevel = u.sub_user_type.user_type.user_type_level , fkBusiness = u.fk_business,
                    UserTypeId= u.sub_user_type.fk_user_type,
                })
                    .FirstOrDefaultAsync();

                if (currentUserInfo == null)
                {
                    return new ResponseModel<List<UserResponseDTO>>
                    {
                        remarks = "User not found",
                        success = false
                    };
                }
                var response = new List<UserResponseDTO>();

                if (currentUserInfo.UserTypelevel== 0)
                {
                    var users = await db.tbl_user
                           .Include(u => u.sub_user_type)
                           .Include(u => u.user_image)
                           .Include(u => u.gender)
                           .Where(
                           u =>
                               u.sub_user_type.fk_user_type == currentUserInfo.UserTypeId &&
                               u.sub_user_type.sub_user_type_level >= currentUserInfo.SubUserTypeLevel
                           )
                           .ToListAsync();
                    response = mapper.Map<List<UserResponseDTO>>(users);
                }
                else if(currentUserInfo.UserTypelevel == 1)
                {
                  if(currentUserInfo.SubUserTypeLevel == 0)
                  {  var users = await db.tbl_user
                           .Include(u => u.sub_user_type)
                           .Include(u => u.user_image)
                           .Include(u => u.gender)
                           .Where(
                           u => 
                               u.fk_business == currentUserInfo.fkBusiness &&
                               u.sub_user_type.fk_user_type == currentUserInfo.UserTypeId &&
                               u.sub_user_type.sub_user_type_level >= currentUserInfo.SubUserTypeLevel
                           )
                           .ToListAsync();
                        response = mapper.Map<List<UserResponseDTO>>(users);
                  }
                    else
                    {
                        var users = await db.tbl_user
                           .Include(u => u.sub_user_type)
                           .Include(u => u.user_image)
                           .Include(u => u.gender)
                           .Where(
                           u =>
                               u.fk_business == currentUserInfo.fkBusiness && u.fk_handler == userGuid &&
                               u.sub_user_type.fk_user_type == currentUserInfo.UserTypeId &&
                               u.sub_user_type.sub_user_type_level >= currentUserInfo.SubUserTypeLevel
                           )
                           .ToListAsync();
                        response = mapper.Map<List<UserResponseDTO>>(users);
                    }
                }
                else
                {
                    return new ResponseModel<List<UserResponseDTO>>
                    {
                        remarks = "You are not allowed to access this data.",
                        success = false
                    };
                }
                // 2️⃣ Get all users under this hierarchy
                //var users = await db.tbl_user
                //    .Include(u => u.sub_user_type)
                //    .Include(u => u.user_image)
                //    .Include(u => u.gender)
                //    .Where(
                //    u =>
                //        u.sub_user_type.fk_user_type == currentUserInfo.UserTypeId &&
                //        u.sub_user_type.sub_user_type_level >= currentUserInfo.SubUserTypeLevel
                //    )
                //    .ToListAsync();

                if (!response.Any())
                {
                    return new ResponseModel<List<UserResponseDTO>>
                    {
                        remarks = "No users found under this user",
                        success = false
                    };
                }

                return new ResponseModel<List<UserResponseDTO>>
                {
                    data = response,
                    remarks = "Success",
                    success = true
                };
            }
            catch (Exception ex)
            {
                return new ResponseModel<List<UserResponseDTO>>
                {
                    remarks = $"There was a fatal error {ex}",
                    success = false
                };
            }
        }
        public async Task<ResponseModel> DeleteUserById(string userId)
        {
            try
            {
                var existingUser = await db.tbl_user.FindAsync(Guid.Parse(userId));
                if (existingUser != null)
                {
                    db.tbl_user.Remove(existingUser); // Mark the entity for deletion
                    await db.SaveChangesAsync();
                    return new ResponseModel()
                    {
                        remarks = "User deleted successfully",
                        success = true,
                    };
                }
                else
                {
                    return new ResponseModel()
                    {
                        remarks = "User not found",
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
