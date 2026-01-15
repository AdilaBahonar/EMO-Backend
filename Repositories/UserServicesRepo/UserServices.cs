using EMO.Models.DBModels.DBTables;
using EMO.Models.DTOs.UserDTOs;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using EMO.Models.DBModels;
using EMO.Models.DTOs.ResponseDTO;
using P3AHR.Models.DTOs.ResponseDTO;
using P3AHR.Repositories.UserServicesRepo;

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
                var existingUser = await db.tbl_user
                    .Where(x => x.user_name.ToLower() == requestDto.userName.ToLower())
                    .FirstOrDefaultAsync();

                if (existingUser == null)
                {
                    var newUser = mapper.Map<tbl_user>(requestDto);
                    await db.tbl_user.AddAsync(newUser);
                    await db.SaveChangesAsync();

                    return new ResponseModel<UserResponseDTO>()
                    {
                        data = mapper.Map<UserResponseDTO>(newUser),
                        remarks = "Success",
                        success = true
                    };
                }
                else
                {
                    return new ResponseModel<UserResponseDTO>()
                    {
                        remarks = "User Already Exists",
                        success = false
                    };
                }
            }
            catch (Exception ex)
            {
                return new ResponseModel<UserResponseDTO>()
                {
                    remarks = $"There was a fatal error: {ex}",
                    success = false
                };
            }
        }

        public async Task<ResponseModel<UserResponseDTO>> UpdateUser(UpdateUserDTO requestDto)
        {
            try
            {
                var existingUser = await db.tbl_user
                    .Where(x => x.user_id == Guid.Parse(requestDto.userId))
                    .FirstOrDefaultAsync();

                if (existingUser != null)
                {
                    mapper.Map(requestDto, existingUser);
                    await db.SaveChangesAsync();

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
                        remarks = "No Record Found",
                        success = false
                    };
                }
            }
            catch (Exception ex)
            {
                return new ResponseModel<UserResponseDTO>()
                {
                    remarks = $"There was a fatal error: {ex}",
                    success = false
                };
            }
        }

        public async Task<ResponseModel<UserResponseDTO>> GetUserById(string userId)
        {
            try
            {
                var user = await db.tbl_user
                    .Include(x => x.user_type)
                    .Where(x => x.user_id == Guid.Parse(userId))
                    .FirstOrDefaultAsync();

                if (user != null)
                {
                    return new ResponseModel<UserResponseDTO>()
                    {
                        data = mapper.Map<UserResponseDTO>(user),
                        remarks = "Success",
                        success = true
                    };
                }
                else
                {
                    return new ResponseModel<UserResponseDTO>()
                    {
                        remarks = "User not found",
                        success = false
                    };
                }
            }
            catch (Exception ex)
            {
                return new ResponseModel<UserResponseDTO>()
                {
                    remarks = $"There was a fatal error: {ex}",
                    success = false
                };
            }
        }

        public async Task<ResponseModel<List<UserResponseDTO>>> GetAllUsers()
        {
            try
            {
                var users = await db.tbl_user
                    .Include(x => x.user_type)
                    .ToListAsync();

                if (users.Any())
                {
                    return new ResponseModel<List<UserResponseDTO>>()
                    {
                        data = mapper.Map<List<UserResponseDTO>>(users),
                        remarks = "Success",
                        success = true
                    };
                }
                else
                {
                    return new ResponseModel<List<UserResponseDTO>>()
                    {
                        remarks = "No User found",
                        success = false
                    };
                }
            }
            catch (Exception ex)
            {
                return new ResponseModel<List<UserResponseDTO>>()
                {
                    remarks = $"There was a fatal error: {ex}",
                    success = false
                };
            }
        }

        public async Task<ResponseModel> DeleteUserById(string userId)
        {
            try
            {
                var user = await db.tbl_user.FindAsync(Guid.Parse(userId));

                if (user != null)
                {
                    db.tbl_user.Remove(user);
                    await db.SaveChangesAsync();

                    return new ResponseModel()
                    {
                        remarks = "User deleted successfully",
                        success = true
                    };
                }
                else
                {
                    return new ResponseModel()
                    {
                        remarks = "User not found",
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
