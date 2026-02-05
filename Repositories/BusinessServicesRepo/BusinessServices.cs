using EMO.Models.DBModels.DBTables;
using EMO.Models.DTOs.BusinessDTOs;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using EMO.Models.DBModels;
using EMO.Models.DTOs.ResponseDTO;

namespace EMO.Repositories.BusinessServicesRepo
{
    public class BusinessServices : IBusinessServices
    {
        private readonly DBUserManagementContext db;
        private readonly IMapper mapper;

        public BusinessServices(DBUserManagementContext db, IMapper mapper)
        {
            this.db = db;
            this.mapper = mapper;
        }

        public async Task<ResponseModel<BusinessResponseDTO>> AddBusiness(AddBusinessDTO requestDto)
        {
            try
            {
                var existingBusiness = await db.tbl_business
                    .Where(x => x.business_name.ToLower() == requestDto.businessName.ToLower())
                    .FirstOrDefaultAsync();

                if (existingBusiness == null)
                {
                    var newBusiness = mapper.Map<tbl_business>(requestDto);
                    await db.tbl_business.AddAsync(newBusiness);
                    await db.SaveChangesAsync();

                    return new ResponseModel<BusinessResponseDTO>()
                    {
                        data = mapper.Map<BusinessResponseDTO>(newBusiness),
                        remarks = "Success",
                        success = true
                    };
                }
                else
                {
                    return new ResponseModel<BusinessResponseDTO>()
                    {
                        remarks = "Business Already Exists",
                        success = false
                    };
                }
            }
            catch (Exception ex)
            {
                return new ResponseModel<BusinessResponseDTO>()
                {
                    remarks = $"There was a fatal error: {ex}",
                    success = false
                };
            }
        }

        public async Task<ResponseModel<BusinessResponseDTO>> UpdateBusiness(UpdateBusinessDTO requestDto)
        {
            try
            {
                var existingBusiness = await db.tbl_business
                    .Where(x => x.business_id == Guid.Parse(requestDto.businessId))
                    .FirstOrDefaultAsync();

                if (existingBusiness != null)
                {
                    mapper.Map(requestDto, existingBusiness);
                    existingBusiness.updated_at = DateTime.Now;
                    await db.SaveChangesAsync();

                    return new ResponseModel<BusinessResponseDTO>()
                    {
                        data = mapper.Map<BusinessResponseDTO>(existingBusiness),
                        remarks = "Success",
                        success = true
                    };
                }
                else
                {
                    return new ResponseModel<BusinessResponseDTO>()
                    {
                        remarks = "No Record Found",
                        success = false
                    };
                }
            }
            catch (Exception ex)
            {
                return new ResponseModel<BusinessResponseDTO>()
                {
                    remarks = $"There was a fatal error: {ex}",
                    success = false
                };
            }
        }

        public async Task<ResponseModel<BusinessResponseDTO>> GetBusinessById(string businessId)
        {
            try
            {
                var business = await db.tbl_business
                    .Where(x => x.business_id == Guid.Parse(businessId))
                    .FirstOrDefaultAsync();

                if (business != null)
                {
                    return new ResponseModel<BusinessResponseDTO>()
                    {
                        data = mapper.Map<BusinessResponseDTO>(business),
                        remarks = "Success",
                        success = true
                    };
                }
                else
                {
                    return new ResponseModel<BusinessResponseDTO>()
                    {
                        remarks = "Business not found",
                        success = false
                    };
                }
            }
            catch (Exception ex)
            {
                return new ResponseModel<BusinessResponseDTO>()
                {
                    remarks = $"There was a fatal error: {ex}",
                    success = false
                };
            }
        }

        public async Task<ResponseModel<BusinessResponseDTO>> GetBusinessByUserId(string userId)
        {
            try
            {

                var user = await db.tbl_user.Where(x => x.user_id == Guid.Parse(userId) && x.sub_user_type.user_type.user_type_name.ToLower() == "business admin").FirstOrDefaultAsync();
                if(user!= null)
                { 
                    var business = await db.tbl_business
                    .Where(x => x.business_id == user.fk_business)
                    .FirstOrDefaultAsync();

                    if (business != null)
                    {
                        return new ResponseModel<BusinessResponseDTO>()
                        {
                            data = mapper.Map<BusinessResponseDTO>(business),
                            remarks = "Success",
                            success = true
                        };
                    }
                    else
                    {
                        return new ResponseModel<BusinessResponseDTO>()
                        {
                            remarks = "Business not found",
                            success = false
                        };
                    }
                }
                else
                {
                    return new ResponseModel<BusinessResponseDTO>()
                    {
                        remarks = "Inavlid Request.",
                        success = false
                    };
                }
            }
            catch (Exception ex)
            {
                return new ResponseModel<BusinessResponseDTO>()
                {
                    remarks = $"There was a fatal error: {ex}",
                    success = false
                };
            }
        }

        public async Task<ResponseModel<List<BusinessResponseDTO>>> GetAllBusinesses()
        {
            try
            {
                var businesses = await db.tbl_business
                    .ToListAsync();

                if (businesses.Any())
                {
                    return new ResponseModel<List<BusinessResponseDTO>>()
                    {
                        data = mapper.Map<List<BusinessResponseDTO>>(businesses),
                        remarks = "Success",
                        success = true
                    };
                }
                else
                {
                    return new ResponseModel<List<BusinessResponseDTO>>()
                    {
                        remarks = "No Business found",
                        success = false
                    };
                }
            }
            catch (Exception ex)
            {
                return new ResponseModel<List<BusinessResponseDTO>>()
                {
                    remarks = $"There was a fatal error: {ex}",
                    success = false
                };
            }
        }

        public async Task<ResponseModel> DeleteBusinessById(string businessId)
        {
            try
            {
                var business = await db.tbl_business.FindAsync(Guid.Parse(businessId));

                if (business != null)
                {
                    db.tbl_business.Remove(business);
                    await db.SaveChangesAsync();

                    return new ResponseModel()
                    {
                        remarks = "Business deleted successfully",
                        success = true
                    };
                }
                else
                {
                    return new ResponseModel()
                    {
                        remarks = "Business not found",
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

        public async Task<ResponseModel<BusinessResponseDTO>> AddBusinessAndBusinessAdmin(AddBusinessAndAdminDTO requestDto)
        {
            try
            {
                var existingBusiness = await db.tbl_business
                    .Where(x => x.business_name.ToLower() == requestDto.businessName.ToLower())
                    .FirstOrDefaultAsync();
                var user = await db.tbl_user.Where(u => u.user_name.ToLower() == requestDto.userName.ToLower()).FirstOrDefaultAsync();

                if (existingBusiness == null)
                {
                    if(user!= null)
                    {
                        return new ResponseModel<BusinessResponseDTO>()
                        {
                            remarks = "User with this username already exist. Try other username.",
                            success = false
                        };
                    }
                    
                    var newBusiness = mapper.Map<tbl_business>(requestDto);
                    await db.tbl_business.AddAsync(newBusiness);
                    
                    var newUser = mapper.Map<tbl_user>(requestDto);
                    newUser.fk_business = newBusiness.business_id;
                    await db.tbl_user.AddAsync(newUser);
                    await db.SaveChangesAsync();

                    if (!string.IsNullOrEmpty(requestDto.imageBase64))
                    {
                        var userImage = new tbl_user_image
                        {
                            fk_user = newUser.user_id,
                            imageBase64 = requestDto.imageBase64
                        };

                        await db.tbl_user_image.AddAsync(userImage);
                        await db.SaveChangesAsync();
                    }

                    return new ResponseModel<BusinessResponseDTO>()
                    {
                        data = mapper.Map<BusinessResponseDTO>(newBusiness),
                        remarks = "Success",
                        success = true
                    };
                }
                else
                {
                    return new ResponseModel<BusinessResponseDTO>()
                    {
                        remarks = "Business Already Exists",
                        success = false
                    };
                }
            }
            catch (Exception ex)
            {
                return new ResponseModel<BusinessResponseDTO>()
                {
                    remarks = $"There was a fatal error: {ex}",
                    success = false
                };
            }
        }
    }
}
