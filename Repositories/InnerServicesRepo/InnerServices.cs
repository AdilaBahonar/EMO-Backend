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

                // ================= BUSINESS CHECK =================
                if (existingUser.fk_business != null)
                {
                    var business = await db.tbl_business
                        .FirstOrDefaultAsync(b => b.business_id == existingUser.fk_business
                                               && b.is_active
                                               && !b.is_deleted);

                    if (business == null)
                    {
                        return new ResponseModel<UserInnerResponseDTO>()
                        {
                            remarks = "Business isn't active. Contact your administrator.",
                            success = false,
                        };
                    }

                    // ================= AGREEMENT CHECK =================
                    bool isBusinessAdmin = existingUser.sub_user_type?.user_type?.user_type_name == "Business Admin";

                    if (!isBusinessAdmin)
                    {
                        bool hasActiveAgreement = await db.tbl_agreement
                            .AnyAsync(x => x.fk_tenant == existingUser.user_id && x.is_active);

                        if (!hasActiveAgreement)
                        {
                            return new ResponseModel<UserInnerResponseDTO>()
                            {
                                remarks = "Your agreement has expired. You can't log in.",
                                success = false,
                            };
                        }
                    }
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
