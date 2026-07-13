using AutoMapper;
using Microsoft.EntityFrameworkCore;
using EMO.Models.DBModels;
using EMO.Models.DTOs.UserDTOs;
using EMO.Models.DTOs.ResponseDTO;
using EMO.Repositories.UserAccessRepo;

namespace EMO.Repositories.InnerServicesRepo
{
    public class InnerServices: IInnerServices
    {
        private readonly DBUserManagementContext db;
        private readonly IMapper mapper;
        private readonly IUserAccessService userAccessService;
        public InnerServices(DBUserManagementContext db, IMapper mapper, IUserAccessService userAccessService)
        {
            this.db = db;
            this.mapper = mapper;
            this.userAccessService = userAccessService;
        }
        public async Task<ResponseModel<UserInnerResponseDTO>> GetUserByOfficialEmail(string username)
        {
            try
            {
                var existingUser = await db.tbl_user
                    .Include(u => u.sub_user_type)
                        .ThenInclude(x => x.user_type)
                    .Include(u => u.user_image)
                    .Include(u => u.gender)
                    .Where(u => u.user_name == username
                                && !u.is_deleted
                                && u.is_active
                                && u.sub_user_type.is_active)
                    .FirstOrDefaultAsync();

                if (existingUser == null)
                {
                    return new ResponseModel<UserInnerResponseDTO>()
                    {
                        remarks = "No record found.",
                        success = false,
                    };
                }

                // Central access validation checks active business and, for tenants,
                // at least one agreement whose date range includes today.
                var access = await userAccessService.GetByUserIdAsync(existingUser.user_id);
                if (access is null || !access.IsLoginAllowed)
                {
                    return new ResponseModel<UserInnerResponseDTO>()
                    {
                        remarks = access?.DenialReason ?? "User access could not be validated.",
                        success = false,
                    };
                }

                // ================= SUCCESS =================
                return new ResponseModel<UserInnerResponseDTO>()
                {
                    data = mapper.Map<UserInnerResponseDTO>(existingUser),
                    remarks = "Success",
                    success = true
                };
            }
            catch (Exception)
            {
                return new ResponseModel<UserInnerResponseDTO>()
                {
                    remarks = "There was a fatal error",
                    success = false,
                };
            }
        }
        public async Task<ResponseModel<UserInnerResponseDTO>> GetUserByPhoneNo(string phoneNo)
        {
            try
            {
                var existingUser = await db.tbl_user.Include(u => u.sub_user_type).Where(u => u.user_phone_no == phoneNo).FirstOrDefaultAsync();
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
                    remarks = $"There was a fatal error  ",
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
                    remarks = $"There was a  fatal error:  ",
                    success = false,
                };
            }
        }
    }
}
