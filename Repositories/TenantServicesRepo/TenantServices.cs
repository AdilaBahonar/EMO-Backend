using AutoMapper;
using EMO.Models.DBModels;
using EMO.Models.DBModels.DBTables;
using EMO.Models.DTOs.BusinessDTOs;
using EMO.Models.DTOs.ResponseDTO;
using EMO.Models.DTOs.TenantDTOs;
using EMO.Models.DTOs.UserDTOs;
using Microsoft.EntityFrameworkCore;
using Npgsql.Replication;
using System.Collections.Generic;

namespace EMO.Repositories.TenantServicesRepo
{
    public class TenantServices : ITenantServices
    {
        private readonly DBUserManagementContext db;
        private readonly IMapper mapper;

        public TenantServices(DBUserManagementContext db, IMapper mapper)
        {
            this.db = db;
            this.mapper = mapper;
        }


        public async Task<ResponseModel> AssignTenant(AssignTenantDTO requestDto)
        {
            using var transaction = await db.Database.BeginTransactionAsync();

            try
            {
                if (!DateTime.TryParse(
                        requestDto.agreement.agreementStartDate,
                        out var startDate) ||
                    !DateTime.TryParse(
                        requestDto.agreement.agreementEndDate,
                        out var endDate))
                {
                    return new ResponseModel
                    {
                        success = false,
                        remarks = "Invalid agreement date."
                    };
                }

                startDate = startDate.Date;
                endDate = endDate.Date;

                if (startDate > endDate)
                {
                    return new ResponseModel
                    {
                        success = false,
                        remarks = "Agreement start date cannot be greater than agreement end date."
                    };
                }

                var officeIds = requestDto.fkOffices
                    .Select(Guid.Parse)
                    .Distinct()
                    .ToList();

                if (officeIds.Count == 0)
                {
                    return new ResponseModel
                    {
                        success = false,
                        remarks = "Select at least one office."
                    };
                }

                Guid tenantId;
                Guid businessId;

                if (!string.IsNullOrWhiteSpace(requestDto.fkTenant))
                {
                    tenantId = Guid.Parse(requestDto.fkTenant);

                    var existingTenant = await db.tbl_user
                        .Where(x =>
                            x.user_id == tenantId &&
                            !x.is_deleted)
                        .Select(x => new
                        {
                            x.fk_business
                        })
                        .FirstOrDefaultAsync();

                    if (existingTenant is null ||
                        !existingTenant.fk_business.HasValue)
                    {
                        return new ResponseModel
                        {
                            success = false,
                            remarks = "Tenant or tenant business was not found."
                        };
                    }

                    businessId = existingTenant.fk_business.Value;
                }
                else
                {
                    if (!Guid.TryParse(
                            requestDto.tenant.fkBusiness,
                            out businessId))
                    {
                        return new ResponseModel
                        {
                            success = false,
                            remarks = "A valid business is required."
                        };
                    }

                    var tenantSubUserTypeId = await db.tbl_sub_user_type
                        .Where(x =>
                            x.user_type.user_type_name.ToLower() == "tenant" &&
                            x.is_active &&
                            !x.is_deleted)
                        .Select(x => x.sub_user_type_id)
                        .FirstOrDefaultAsync();

                    if (tenantSubUserTypeId == Guid.Empty)
                    {
                        return new ResponseModel
                        {
                            success = false,
                            remarks = "Tenant user type is not configured."
                        };
                    }

                    requestDto.tenant.fkSubUserType =
                        tenantSubUserTypeId.ToString();

                    var newTenant = mapper.Map<tbl_user>(
                        requestDto.tenant);

                    newTenant.fk_business = businessId;

                    await db.tbl_user.AddAsync(newTenant);
                    await db.SaveChangesAsync();

                    tenantId = newTenant.user_id;
                }

                var offices = await db.tbl_office
                    .Where(x =>
                        officeIds.Contains(x.office_id) &&
                        x.fk_business == businessId &&
                        x.is_active &&
                        !x.is_deleted)
                    .ToListAsync();

                if (offices.Count != officeIds.Count)
                {
                    return new ResponseModel
                    {
                        success = false,
                        remarks = "One or more offices are invalid, inactive, or belong to another business."
                    };
                }

                // Expired agreements do not block an office from being assigned again.
                // Only enabled agreements with overlapping dates block assignment.
                var overlappingOfficeIds = await (
                    from existingAgreement in db.tbl_agreement.AsNoTracking()
                    join officeAgreement in db.tbl_office_agreement.AsNoTracking()
                        on existingAgreement.agreement_id
                        equals officeAgreement.fk_agreement
                    where existingAgreement.fk_business == businessId
                          && existingAgreement.is_active
                          && !existingAgreement.is_deleted
                          && !officeAgreement.is_deleted
                          && officeIds.Contains(officeAgreement.fk_office)
                          && existingAgreement.agreement_start_date.Date <= endDate
                          && existingAgreement.agreement_end_date.Date >= startDate
                    select officeAgreement.fk_office)
                    .Distinct()
                    .ToListAsync();

                if (overlappingOfficeIds.Count > 0)
                {
                    var overlappingOfficeNames = offices
                        .Where(x =>
                            overlappingOfficeIds.Contains(x.office_id))
                        .Select(x => x.office_name)
                        .ToList();

                    return new ResponseModel
                    {
                        success = false,
                        remarks =
                            $"Agreement dates overlap for: {string.Join(", ", overlappingOfficeNames)}."
                    };
                }

                var newAgreement = mapper.Map<tbl_agreement>(
                    requestDto.agreement);

                newAgreement.fk_tenant = tenantId;
                newAgreement.fk_business = businessId;
                newAgreement.agreement_start_date = startDate;
                newAgreement.agreement_end_date = endDate;
                newAgreement.is_active = true;
                newAgreement.updated_at = DateTime.Now;

                await db.tbl_agreement.AddAsync(newAgreement);

                var officeAgreements = officeIds
                    .Select(officeId => new tbl_office_agreement
                    {
                        fk_agreement = newAgreement.agreement_id,
                        fk_office = officeId
                    })
                    .ToList();

                await db.tbl_office_agreement.AddRangeAsync(
                    officeAgreements);

                if (requestDto.contactPerson?.Any() == true)
                {
                    foreach (var contactPersonDto in requestDto.contactPerson)
                    {
                        contactPersonDto.fkAgreement =
                            newAgreement.agreement_id.ToString();

                        var contactPerson =
                            mapper.Map<tbl_contact_person>(
                                contactPersonDto);

                        await db.tbl_contact_person.AddAsync(
                            contactPerson);
                    }
                }

                var today = DateTime.Today;

                if (startDate <= today && endDate >= today)
                {
                    foreach (var office in offices)
                    {
                        office.is_occupied = true;
                        office.updated_at = DateTime.Now;
                    }
                }

                await db.SaveChangesAsync();
                await transaction.CommitAsync();

                return new ResponseModel
                {
                    success = true,
                    remarks = "Tenant assigned successfully."
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                return new ResponseModel
                {
                    success = false,
                    remarks = $"Fatal error: {ex.Message}"
                };
            }
        }
        //public async Task<ResponseModel<TenantResponseDTO>> AddTenant(AddTenantDTO requestDto)
        //{
        //    try
        //    {
        //        var existingTenant = await db.tbl_tenant
        //            .Where(x => x.tenant_name.ToLower() == requestDto.tenantName.ToLower())
        //            .FirstOrDefaultAsync();

        //        if (existingTenant == null)
        //        {
        //            var newTenant = mapper.Map<tbl_tenant>(requestDto);
        //            await db.tbl_tenant.AddAsync(newTenant);
        //            await db.SaveChangesAsync();

        //            return new ResponseModel<TenantResponseDTO>()
        //            {
        //                data = mapper.Map<TenantResponseDTO>(newTenant),
        //                remarks = "Success",
        //                success = true
        //            };
        //        }
        //        else
        //        {
        //            return new ResponseModel<TenantResponseDTO>()
        //            {
        //                remarks = "Tenant Already Exists",
        //                success = false
        //            };
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return new ResponseModel<TenantResponseDTO>()
        //        {
        //            remarks = $"There was a fatal error",
        //            success = false
        //        };
        //    }
        //}
        //public async Task<ResponseModel<TenantResponseDTO>> UpdateTenant(UpdateTenantDTO requestDto)
        //{
        //    try
        //    {
        //        var existingTenant = await db.tbl_tenant
        //            .Where(x => x.tenant_id == Guid.Parse(requestDto.tenantId))
        //            .FirstOrDefaultAsync();

        //        if (existingTenant != null)
        //        {
        //            mapper.Map(requestDto, existingTenant);
        //            existingTenant.updated_at = DateTime.Now;
        //            await db.SaveChangesAsync();

        //            return new ResponseModel<TenantResponseDTO>()
        //            {
        //                data = mapper.Map<TenantResponseDTO>(existingTenant),
        //                remarks = "Success",
        //                success = true
        //            };
        //        }
        //        else
        //        {
        //            return new ResponseModel<TenantResponseDTO>()
        //            {
        //                remarks = "No Record Found",
        //                success = false
        //            };
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return new ResponseModel<TenantResponseDTO>()
        //        {
        //            remarks = $"There was a fatal error",
        //            success = false
        //        };
        //    }
        //}
        //public async Task<ResponseModel<TenantResponseDTO>> GetTenantById(string tenantId)
        //{
        //    try
        //    {
        //        var tenant = await db.tbl_tenant
        //            .Where(x => x.tenant_id == Guid.Parse(tenantId))
        //            .FirstOrDefaultAsync();

        //        if (tenant != null)
        //        {
        //            return new ResponseModel<TenantResponseDTO>()
        //            {
        //                data = mapper.Map<TenantResponseDTO>(tenant),
        //                remarks = "Success",
        //                success = true
        //            };
        //        }
        //        else
        //        {
        //            return new ResponseModel<TenantResponseDTO>()
        //            {
        //                remarks = "Tenant not found",
        //                success = false
        //            };
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return new ResponseModel<TenantResponseDTO>()
        //        {
        //            remarks = $"There was a fatal error",
        //            success = false
        //        };
        //    }
        //}
        public async Task<ResponseModel<List<tenantResponseDTO>>> GetTenantByBusinessId(string BusinessId)
        {
            try
            {
                var tenants = await db.tbl_user
                    .Where(x =>
                        x.fk_business == Guid.Parse(BusinessId) &&
                        x.sub_user_type.user_type.user_type_name.ToLower() == "tenant")
                    .Select(x => new tenantResponseDTO
                    {
                        tenantId = x.user_id.ToString(),
                        tenantName = x.name,
                        tenantUserName = x.user_name,
                        tenantEmail = x.user_email
                    })
                    .ToListAsync();

                if (tenants.Any())
                {
                    return new ResponseModel<List<tenantResponseDTO>>()
                    {
                        data = tenants,
                        remarks = "Success",
                        success = true
                    };
                }

                return new ResponseModel<List<tenantResponseDTO>>()
                {
                    remarks = "No record found.",
                    success = false
                };
            }
            catch (Exception ex)
            {
                return new ResponseModel<List<tenantResponseDTO>>()
                {
                    remarks = $"There was a fatal error",
                    success = false
                };
            }
        }

        //public async Task<ResponseModel<tenantResponseDTO>> GetTenantByAgreementId(string AgreementId)
        //{
        //    try
        //    {
        //        if (!Guid.TryParse(AgreementId, out Guid agreementGuid))
        //        {
        //            return new ResponseModel<tenantResponseDTO>()
        //            {
        //                remarks = "Invalid Agreement Id",
        //                success = false
        //            };
        //        }

        //        var tenants = await db.tbl_agreement
        //            .Where(a => a.agreement_id == agreementGuid && !a.is_deleted)
        //            .Select(a => a.tenant) // navigation property (tenant)
        //            .Where(u => u != null &&
        //                        !u.is_deleted &&
        //                        u.is_active &&
        //                        u.sub_user_type.user_type.user_type_name.ToLower() == "tenant")
        //            .Select(u => new tenantResponseDTO
        //            {
        //                tenantId = u.user_id.ToString(),
        //                tenantName = u.name,
        //                tenantUserName = u.user_name,
        //                tenantEmail = u.user_email
        //            })
        //            .FirstOrDefaultAsync();

        //        if (tenants!= null)
        //        {
        //            return new ResponseModel<tenantResponseDTO>()
        //            {
        //                data = tenants,
        //                remarks = "Success",
        //                success = true
        //            };
        //        }

        //        return new ResponseModel<tenantResponseDTO>()
        //        {
        //            remarks = "No tenant found for this agreement.",
        //            success = false
        //        };
        //    }
        //    catch (Exception)
        //    {
        //        return new ResponseModel<tenantResponseDTO>()
        //        {
        //            remarks = "There was a fatal error",
        //            success = false
        //        };
        //    }
        //}
        public async Task<ResponseModel<List<UserResponseDTO>>> GetTenantByAgreementId(string AgreementId)
        {
            try
            {
                if (!Guid.TryParse(AgreementId, out Guid agreementGuid))
                {
                    return new ResponseModel<List<UserResponseDTO>>()
                    {
                        remarks = "Invalid Agreement Id",
                        success = false
                    };
                }

                var tenantId = await db.tbl_agreement
                    .Where(x => x.agreement_id == agreementGuid)
                    .Select(x => x.fk_tenant)
                    .FirstOrDefaultAsync();

                if (tenantId == Guid.Empty)
                {
                    return new ResponseModel<List<UserResponseDTO>>()
                    {
                        remarks = "No tenant found for this agreement",
                        success = false
                    };
                }

                var tenant = await db.tbl_user
                    .Include(x => x.sub_user_type)
                    .Include(x => x.gender).Include(x=>x.user_image)
                    .FirstOrDefaultAsync(x => x.user_id == tenantId);

                if (tenant == null)
                {
                    return new ResponseModel<List<UserResponseDTO>>()
                    {
                        remarks = "User not found",
                        success = false
                    };
                }

                // ✅ Wrap single item into list
                var result = new List<UserResponseDTO>
        {
            mapper.Map<UserResponseDTO>(tenant)
        };

                return new ResponseModel<List<UserResponseDTO>>()
                {
                    data = result,
                    remarks = "Success",
                    success = true
                };
            }
            catch (Exception)
            {
                return new ResponseModel<List<UserResponseDTO>>()
                {
                    remarks = "There was a fatal error",
                    success = false,
                };
            }
        }
        public async Task<ResponseModel<List<UserResponseDTO>>> GetTenantsByBusinessId(string BusinessId)
        {
            try
            {
                if (!Guid.TryParse(BusinessId, out Guid BusinessGuid))
                {
                    return new ResponseModel<List<UserResponseDTO>>()
                    {
                        remarks = "Invalid Business Id",
                        success = false
                    };
                }

                var business = await db.tbl_business
                    .Where(x => x.business_id == BusinessGuid && !x.is_deleted && x.is_active)
                    .FirstOrDefaultAsync();

                if (business == null)
                {
                    return new ResponseModel<List<UserResponseDTO>>()
                    {
                        remarks = "No such business found.",
                        success = false
                    };
                }

                var tenants = await db.tbl_user
                    .Include(x => x.sub_user_type)
                    .Include(x => x.gender).Include(x => x.user_image).Where(x => x.fk_business == business.business_id && x.sub_user_type.user_type.user_type_name.ToLower() == "tenant" && !x.is_deleted)
                    .ToListAsync();

                if (tenants == null)
                {
                    return new ResponseModel<List<UserResponseDTO>>()
                    {
                        remarks = "No record found",
                        success = false
                    };
                }


                return new ResponseModel<List<UserResponseDTO>>()
                {
                    data = mapper.Map<List<UserResponseDTO>>(tenants),
                    remarks = "Success",
                    success = true
                };
            }
            catch (Exception)
            {
                return new ResponseModel<List<UserResponseDTO>>()
                {
                    remarks = "There was a fatal error",
                    success = false,
                };
            }
        }
        //public async Task<ResponseModel<List<TenantResponseDTO>>> GetAllTenants()
        //{
        //    try
        //    {
        //        var tenants = await db.tbl_tenant.ToListAsync();

        //        if (tenants.Any())
        //        {
        //            return new ResponseModel<List<TenantResponseDTO>>()
        //            {
        //                data = mapper.Map<List<TenantResponseDTO>>(tenants),
        //                remarks = "Success",
        //                success = true
        //            };
        //        }
        //        else
        //        {
        //            return new ResponseModel<List<TenantResponseDTO>>()
        //            {
        //                remarks = "No Tenant found",
        //                success = false
        //            };
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return new ResponseModel<List<TenantResponseDTO>>()
        //        {
        //            remarks = $"There was a fatal error",
        //            success = false
        //        };
        //    }
        //}

        //public async Task<ResponseModel> DeleteTenantById(string tenantId)
        //{
        //    try
        //    {
        //        var tenant = await db.tbl_tenant.FindAsync(Guid.Parse(tenantId));

        //        if (tenant != null)
        //        {
        //            db.tbl_tenant.Remove(tenant);
        //            await db.SaveChangesAsync();

        //            return new ResponseModel()
        //            {
        //                remarks = "Tenant deleted successfully",
        //                success = true
        //            };
        //        }
        //        else
        //        {
        //            return new ResponseModel()
        //            {
        //                remarks = "Tenant not found",
        //                success = false
        //            };
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        return new ResponseModel()
        //        {
        //            remarks = $"There was a fatal error",
        //            success = false
        //        };
        //    }
        //}
    }
}
