using AutoMapper;
using Microsoft.EntityFrameworkCore;
using EMO.Models.DBModels;
using EMO.Models.DTOs.UserDTOs;
using EMO.Models.DTOs.ResponseDTO;

namespace EMO.Repositories.InnerServicesRepo
{
    public class InnerServices: IInnerServices
    {
        private readonly DBUserManagementContext db;
        private readonly IMapper mapper;
        public InnerServices(DBUserManagementContext db, IMapper mapper)
        {
            this.db = db;
            this.mapper = mapper;
        }
        public async Task<ResponseModel<UserInnerResponseDTO>> GetUserByOfficialEmail(string officialEmail)
        {
            try
            {
                var existingUser = await db.tbl_user.Include(u => u.user_type).Where(u => u.user_official_email == officialEmail).FirstOrDefaultAsync();
                if (existingUser != null)
                {
                    return new ResponseModel<UserInnerResponseDTO>()
                    {
                        data = mapper.Map<UserInnerResponseDTO>(existingUser),
                        remarks = "Success",
                        success = true
                    };
                }
                else
                {
                    return new ResponseModel<UserInnerResponseDTO>()
                    {
                        remarks = "User not found",
                        success = false,
                    };
                }
            }
            catch (Exception ex)
            {
                return new ResponseModel<UserInnerResponseDTO>()
                {
                    remarks = $"There was a fatal error {ex.ToString()}",
                    success = false,
                };
            }
        }
        public async Task<ResponseModel<UserInnerResponseDTO>> GetUserByPhoneNo(string phoneNo)
        {
            try
            {
                var existingUser = await db.tbl_user.Include(u => u.user_type).Where(u => u.user_phone_no == phoneNo).FirstOrDefaultAsync();
                if (existingUser != null)
                {
                    return new ResponseModel<UserInnerResponseDTO>()
                    {
                        data = mapper.Map<UserInnerResponseDTO>(existingUser),
                        remarks = "Success",
                        success = true
                    };
                }
                else
                {
                    return new ResponseModel<UserInnerResponseDTO>()
                    {
                        remarks = "User not found",
                        success = false,
                    };
                }
            }
            catch (Exception ex)
            {
                return new ResponseModel<UserInnerResponseDTO>()
                {
                    remarks = $"There was a fatal error {ex.ToString()}",
                    success = false,
                };
            }
        }
        public async Task<ResponseModel<UserResponseDTO>> UpdateUser(UpdateInnerUserDTO requestDto)
        {
            try
            {
                var existingUser = await db.tbl_user.Where(u => u.user_id == Guid.Parse(requestDto.userId)).FirstOrDefaultAsync();
                if (existingUser != null)
                {
                    mapper.Map(requestDto, existingUser);
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
    }
}
