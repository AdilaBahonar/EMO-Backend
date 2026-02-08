using AutoMapper;
using EMO.Models.DBModels;
using EMO.Models.DBModels.DBTables;
using EMO.Models.DTOs.BusinessDTOs;
using EMO.Models.DTOs.ResponseDTO;
using EMO.Models.DTOs.TenantDTOs;
using Microsoft.EntityFrameworkCore;

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
                /* ===============================
                 * 1️⃣ VALIDATE OFFICES
                 * =============================== */

                var officeIds = requestDto.fkOffices
                    .Select(Guid.Parse)
                    .ToList();

                var offices = await db.tbl_office
                    .Where(x => officeIds.Contains(x.office_id))
                    .ToListAsync();

                if (offices.Count != officeIds.Count)
                {
                    return new ResponseModel
                    {
                        success = false,
                        remarks = "One or more office IDs are invalid."
                    };
                }

                var occupiedOffice = offices.Where(x => x.is_occupied).ToList();

                if (occupiedOffice.Any())
                {
                    return new ResponseModel
                    {
                        success = false,
                        remarks = "One or more selected Offices are already in use."
                    };
                }

                /* ===============================
                 * 2️⃣ RESOLVE TENANT (NEW OR EXISTING)
                 * =============================== */

                Guid tenantId;

                if (!string.IsNullOrWhiteSpace(requestDto.fkTenant))
                {
                    // ✅ USE EXISTING TENANT
                    tenantId = Guid.Parse(requestDto.fkTenant);
                }
                else
                {
                    // ✅ CREATE NEW TENANT

                    var tenantFk = await db.tbl_sub_user_type
                        .Where(x => x.user_type.user_type_name.ToLower() == "tenant")
                        .Select(x => x.sub_user_type_id)
                        .FirstOrDefaultAsync();

                    requestDto.tenant.fkSubUserType = tenantFk.ToString();
                    var newTenant = mapper.Map<tbl_user>(requestDto.tenant);
                    

                    await db.tbl_user.AddAsync(newTenant);
                    await db.SaveChangesAsync();

                    tenantId = newTenant.user_id;
                }

                /* ===============================
                 * 3️⃣ CREATE AGREEMENTS
                 * =============================== */

                foreach (var officeId in officeIds)
                {
                    var agreement = mapper.Map<tbl_agreement>(requestDto.agreement);

                    agreement.fk_tenant = tenantId;
                    agreement.fk_office = officeId;

                    await db.tbl_agreement.AddAsync(agreement);
                }

                /* ===============================
                 * 4️⃣ CREATE CONTACT PERSONS
                 * =============================== */

                if (requestDto.contactPerson != null && requestDto.contactPerson.Any())
                {
                    foreach (var cp in requestDto.contactPerson)
                    {
                        cp.fkTenant = tenantId.ToString();
                        var contact = mapper.Map<tbl_contact_person>(cp);

                        await db.tbl_contact_person.AddAsync(contact);
                    }
                }

                /* ===============================
                 * 5️⃣ MARK OFFICES OCCUPIED
                 * =============================== */

                foreach (var office in offices)
                {
                    office.is_occupied = true;
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
        //            remarks = $"There was a fatal error: {ex}",
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
        //            remarks = $"There was a fatal error: {ex}",
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
        //            remarks = $"There was a fatal error: {ex}",
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
                    remarks = $"There was a fatal error: {ex}",
                    success = false
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
        //            remarks = $"There was a fatal error: {ex}",
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
        //            remarks = $"There was a fatal error: {ex}",
        //            success = false
        //        };
        //    }
        //}
    }
}
