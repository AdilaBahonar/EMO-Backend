using EMO.Models.DBModels.DBTables;
using EMO.Models.DTOs.TenantDTOs;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using EMO.Models.DBModels;
using EMO.Models.DTOs.ResponseDTO;
using EMO.Models.DTOs.TenantDTOs.EMO.Models.DTOs.TenantDTOs;

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

        public async Task<ResponseModel<TenantResponseDTO>> AddTenant(AddTenantDTO requestDto)
        {
            try
            {
                var existingTenant = await db.tbl_tenant
                    .Where(x => x.tenant_name.ToLower() == requestDto.tenantName.ToLower())
                    .FirstOrDefaultAsync();

                if (existingTenant == null)
                {
                    var newTenant = mapper.Map<tbl_tenant>(requestDto);
                    await db.tbl_tenant.AddAsync(newTenant);
                    await db.SaveChangesAsync();

                    return new ResponseModel<TenantResponseDTO>()
                    {
                        data = mapper.Map<TenantResponseDTO>(newTenant),
                        remarks = "Success",
                        success = true
                    };
                }
                else
                {
                    return new ResponseModel<TenantResponseDTO>()
                    {
                        remarks = "Tenant Already Exists",
                        success = false
                    };
                }
            }
            catch (Exception ex)
            {
                return new ResponseModel<TenantResponseDTO>()
                {
                    remarks = $"There was a fatal error: {ex}",
                    success = false
                };
            }
        }

        public async Task<ResponseModel<TenantResponseDTO>> UpdateTenant(UpdateTenantDTO requestDto)
        {
            try
            {
                var existingTenant = await db.tbl_tenant
                    .Where(x => x.tenant_id == Guid.Parse(requestDto.tenantId))
                    .FirstOrDefaultAsync();

                if (existingTenant != null)
                {
                    mapper.Map(requestDto, existingTenant);
                    existingTenant.updated_at = DateTime.Now;
                    await db.SaveChangesAsync();

                    return new ResponseModel<TenantResponseDTO>()
                    {
                        data = mapper.Map<TenantResponseDTO>(existingTenant),
                        remarks = "Success",
                        success = true
                    };
                }
                else
                {
                    return new ResponseModel<TenantResponseDTO>()
                    {
                        remarks = "No Record Found",
                        success = false
                    };
                }
            }
            catch (Exception ex)
            {
                return new ResponseModel<TenantResponseDTO>()
                {
                    remarks = $"There was a fatal error: {ex}",
                    success = false
                };
            }
        }

        public async Task<ResponseModel<TenantResponseDTO>> GetTenantById(string tenantId)
        {
            try
            {
                var tenant = await db.tbl_tenant
                    .Where(x => x.tenant_id == Guid.Parse(tenantId))
                    .FirstOrDefaultAsync();

                if (tenant != null)
                {
                    return new ResponseModel<TenantResponseDTO>()
                    {
                        data = mapper.Map<TenantResponseDTO>(tenant),
                        remarks = "Success",
                        success = true
                    };
                }
                else
                {
                    return new ResponseModel<TenantResponseDTO>()
                    {
                        remarks = "Tenant not found",
                        success = false
                    };
                }
            }
            catch (Exception ex)
            {
                return new ResponseModel<TenantResponseDTO>()
                {
                    remarks = $"There was a fatal error: {ex}",
                    success = false
                };
            }
        }

        public async Task<ResponseModel<List<TenantResponseDTO>>> GetAllTenants()
        {
            try
            {
                var tenants = await db.tbl_tenant.ToListAsync();

                if (tenants.Any())
                {
                    return new ResponseModel<List<TenantResponseDTO>>()
                    {
                        data = mapper.Map<List<TenantResponseDTO>>(tenants),
                        remarks = "Success",
                        success = true
                    };
                }
                else
                {
                    return new ResponseModel<List<TenantResponseDTO>>()
                    {
                        remarks = "No Tenant found",
                        success = false
                    };
                }
            }
            catch (Exception ex)
            {
                return new ResponseModel<List<TenantResponseDTO>>()
                {
                    remarks = $"There was a fatal error: {ex}",
                    success = false
                };
            }
        }

        public async Task<ResponseModel> DeleteTenantById(string tenantId)
        {
            try
            {
                var tenant = await db.tbl_tenant.FindAsync(Guid.Parse(tenantId));

                if (tenant != null)
                {
                    db.tbl_tenant.Remove(tenant);
                    await db.SaveChangesAsync();

                    return new ResponseModel()
                    {
                        remarks = "Tenant deleted successfully",
                        success = true
                    };
                }
                else
                {
                    return new ResponseModel()
                    {
                        remarks = "Tenant not found",
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
