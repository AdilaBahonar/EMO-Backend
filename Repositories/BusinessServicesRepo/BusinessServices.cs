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
                    .Where(b => b.business_name.ToLower() == requestDto.businessName.ToLower())
                    .FirstOrDefaultAsync();

                if (existingBusiness == null)
                {
                    var newBusiness = mapper.Map<tbl_business>(requestDto);
                    await db.tbl_business.AddAsync(newBusiness);
                    await db.SaveChangesAsync();

                    var response = mapper.Map<BusinessResponseDTO>(newBusiness);

                    return new ResponseModel<BusinessResponseDTO>()
                    {
                        data = response,
                        remarks = "Success",
                        success = true,
                    };
                }
                else
                {
                    return new ResponseModel<BusinessResponseDTO>()
                    {
                        remarks = "Business Already Exists",
                        success = false,
                    };
                }
            }
            catch (Exception ex)
            {
                return new ResponseModel<BusinessResponseDTO>()
                {
                    remarks = $"There was a fatal error: {ex}",
                    success = false,
                };
            }
        }

        public async Task<ResponseModel<BusinessResponseDTO>> UpdateBusiness(UpdateBusinessDTO requestDto)
        {
            try
            {
                var existingBusiness = await db.tbl_business
                    .Where(b => b.business_id == Guid.Parse(requestDto.businessId))
                    .FirstOrDefaultAsync();

                if (existingBusiness != null)
                {
                    mapper.Map(requestDto, existingBusiness);

                    await db.SaveChangesAsync();

                    var response = mapper.Map<BusinessResponseDTO>(existingBusiness);

                    return new ResponseModel<BusinessResponseDTO>()
                    {
                        data = response,
                        remarks = "Success",
                        success = true,
                    };
                }
                else
                {
                    return new ResponseModel<BusinessResponseDTO>()
                    {
                        remarks = "No Record Found",
                        success = false,
                    };
                }
            }
            catch (Exception ex)
            {
                return new ResponseModel<BusinessResponseDTO>()
                {
                    remarks = $"There was a fatal error: {ex}",
                    success = false,
                };
            }
        }

        public async Task<ResponseModel<BusinessResponseDTO>> GetBusinessById(string businessId)
        {
            try
            {
                var business = await db.tbl_business
                    .Include(b => b.user)
                    .Where(b => b.business_id == Guid.Parse(businessId))
                    .FirstOrDefaultAsync();

                if (business != null)
                {
                    var response = mapper.Map<BusinessResponseDTO>(business);

                    return new ResponseModel<BusinessResponseDTO>()
                    {
                        data = response,
                        remarks = "Success",
                        success = true
                    };
                }
                else
                {
                    return new ResponseModel<BusinessResponseDTO>()
                    {
                        remarks = "Business not found",
                        success = false,
                    };
                }
            }
            catch (Exception ex)
            {
                return new ResponseModel<BusinessResponseDTO>()
                {
                    remarks = $"There was a fatal error {ex}",
                    success = false,
                };
            }
        }

        public async Task<ResponseModel<List<BusinessResponseDTO>>> GetAllBusinesses()
        {
            try
            {
                var businesses = await db.tbl_business.Include(b => b.user).ToListAsync();

                if (businesses.Any())
                {
                    var responseList = mapper.Map<List<BusinessResponseDTO>>(businesses);

                    return new ResponseModel<List<BusinessResponseDTO>>()
                    {
                        data = responseList,
                        remarks = "Success",
                        success = true
                    };
                }
                else
                {
                    return new ResponseModel<List<BusinessResponseDTO>>()
                    {
                        remarks = "No Business found",
                        success = false,
                    };
                }
            }
            catch (Exception ex)
            {
                return new ResponseModel<List<BusinessResponseDTO>>()
                {
                    remarks = $"There was a fatal error {ex}",
                    success = false,
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
                        success = true,
                    };
                }
                else
                {
                    return new ResponseModel()
                    {
                        remarks = "Business not found",
                        success = false,
                    };
                }
            }
            catch (Exception ex)
            {
                return new ResponseModel()
                {
                    remarks = $"There was a fatal error {ex}",
                    success = false,
                };
            }
        }
    }

}
