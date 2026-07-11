using AutoMapper;
using EMO.Models.DBModels;
using EMO.Models.DBModels.DBTables;
using EMO.Models.DTOs.ApplianceDTOs;
using EMO.Models.DTOs.ResponseDTO;
using EMO.Repositories.ApplianceRedisRepo;
using Microsoft.EntityFrameworkCore;

namespace EMO.Repositories.ApplianceServicesRepo
{
    public class ApplianceServices : IApplianceServices
    {
        private readonly DBUserManagementContext db;
        private readonly IMapper mapper;
        private readonly IApplianceRedisCacheService applianceRedisCacheService;

        public ApplianceServices(DBUserManagementContext db, IMapper mapper, IApplianceRedisCacheService applianceRedisCacheService)
        {
            this.db = db;
            this.mapper = mapper;
            this.applianceRedisCacheService = applianceRedisCacheService;
        }

        public async Task<ResponseModel<ApplianceResponseDTO>> AddAppliance(AddApplianceDTO requestDto)
        {
            try
            {
                var utilityId = Guid.Parse(requestDto.fkUtility);

                var utilityExists = await db.tbl_utility.AnyAsync(x => x.utility_id == utilityId && !x.is_deleted);
                if (!utilityExists)
                {
                    return new ResponseModel<ApplianceResponseDTO>()
                    {
                        remarks = "Utility not found",
                        success = false
                    };
                }

                var existingAppliance = await db.tbl_appliance
                    .Where(x => x.appliance_name.ToLower() == requestDto.applianceName.ToLower()
                             && x.company_name.ToLower() == requestDto.companyName.ToLower()
                             && x.model_number.ToLower() == requestDto.modelNumber.ToLower()
                             && x.fk_utility == utilityId
                             && !x.is_deleted)
                    .FirstOrDefaultAsync();

                if (existingAppliance != null)
                {
                    return new ResponseModel<ApplianceResponseDTO>()
                    {
                        remarks = "Appliance already exists for this utility",
                        success = false
                    };
                }

                var newAppliance = mapper.Map<tbl_appliance>(requestDto);
                await db.tbl_appliance.AddAsync(newAppliance);
                await db.SaveChangesAsync();

                var appliance = await db.tbl_appliance
                    .Include(x => x.utility)
                    .Where(x => x.appliance_id == newAppliance.appliance_id)
                    .FirstOrDefaultAsync();

                return new ResponseModel<ApplianceResponseDTO>()
                {
                    data = mapper.Map<ApplianceResponseDTO>(appliance),
                    remarks = "Success",
                    success = true
                };
            }
            catch (Exception)
            {
                return new ResponseModel<ApplianceResponseDTO>()
                {
                    remarks = "There was a fatal error",
                    success = false
                };
            }
        }

        public async Task<ResponseModel<ApplianceResponseDTO>> UpdateAppliance(UpdateApplianceDTO requestDto)
        {
            try
            {
                var existingAppliance = await db.tbl_appliance
                    .Where(x => x.appliance_id == Guid.Parse(requestDto.applianceId) && !x.is_deleted)
                    .FirstOrDefaultAsync();

                if (existingAppliance == null)
                {
                    return new ResponseModel<ApplianceResponseDTO>()
                    {
                        remarks = "Appliance not found",
                        success = false
                    };
                }

                mapper.Map(requestDto, existingAppliance);
                existingAppliance.updated_at = DateTime.Now;
                await db.SaveChangesAsync();
                var appliance = await db.tbl_appliance
                    .Include(x => x.utility)
                    .Where(x => x.appliance_id == existingAppliance.appliance_id)
                    .FirstOrDefaultAsync();

                return new ResponseModel<ApplianceResponseDTO>()
                {
                    data = mapper.Map<ApplianceResponseDTO>(appliance),
                    remarks = "Success",
                    success = true
                };
            }
            catch (Exception)
            {
                return new ResponseModel<ApplianceResponseDTO>()
                {
                    remarks = "There was a fatal error",
                    success = false
                };
            }
        }

        public async Task<ResponseModel<ApplianceResponseDTO>> GetApplianceById(string applianceId)
        {
            try
            {
                var appliance = await db.tbl_appliance
                    .Include(x => x.utility)
                    .Where(x => x.appliance_id == Guid.Parse(applianceId) && !x.is_deleted)
                    .FirstOrDefaultAsync();

                if (appliance == null)
                {
                    return new ResponseModel<ApplianceResponseDTO>()
                    {
                        remarks = "Appliance not found",
                        success = false
                    };
                }

                return new ResponseModel<ApplianceResponseDTO>()
                {
                    data = mapper.Map<ApplianceResponseDTO>(appliance),
                    remarks = "Success",
                    success = true
                };
            }
            catch (Exception)
            {
                return new ResponseModel<ApplianceResponseDTO>()
                {
                    remarks = "There was a fatal error",
                    success = false
                };
            }
        }

        public async Task<ResponseModel<List<ApplianceResponseDTO>>> GetAllAppliances()
        {
            try
            {
                var appliances = await db.tbl_appliance
                    .Include(x => x.utility)
                    .Where(x => !x.is_deleted)
                    .OrderBy(x => x.utility.utility_name)
                    .ThenBy(x => x.appliance_name)
                    .ToListAsync();

                if (appliances.Any())
                {
                    return new ResponseModel<List<ApplianceResponseDTO>>()
                    {
                        data = mapper.Map<List<ApplianceResponseDTO>>(appliances),
                        remarks = "Success",
                        success = true
                    };
                }

                return new ResponseModel<List<ApplianceResponseDTO>>()
                {
                    remarks = "No appliance found",
                    success = false
                };
            }
            catch (Exception)
            {
                return new ResponseModel<List<ApplianceResponseDTO>>()
                {
                    remarks = "There was a fatal error",
                    success = false
                };
            }
        }

        public async Task<ResponseModel<List<ApplianceResponseDTO>>> GetAppliancesByUtilityId(string utilityId)
        {
            try
            {
                var utilityGuid = Guid.Parse(utilityId);

                var appliances = await db.tbl_appliance
                    .Include(x => x.utility)
                    .Where(x => x.fk_utility == utilityGuid && x.is_active && !x.is_deleted)
                    .OrderBy(x => x.appliance_name)
                    .ToListAsync();

                if (appliances.Any())
                {
                    return new ResponseModel<List<ApplianceResponseDTO>>()
                    {
                        data = mapper.Map<List<ApplianceResponseDTO>>(appliances),
                        remarks = "Success",
                        success = true
                    };
                }

                return new ResponseModel<List<ApplianceResponseDTO>>()
                {
                    remarks = "No appliance found for this utility",
                    success = false
                };
            }
            catch (Exception)
            {
                return new ResponseModel<List<ApplianceResponseDTO>>()
                {
                    remarks = "There was a fatal error",
                    success = false
                };
            }
        }

        public async Task<ResponseModel> DeleteApplianceById(string applianceId)
        {
            try
            {
                var appliance = await db.tbl_appliance
                    .Where(x => x.appliance_id == Guid.Parse(applianceId) && !x.is_deleted)
                    .FirstOrDefaultAsync();

                if (appliance == null)
                {
                    return new ResponseModel()
                    {
                        remarks = "Appliance not found",
                        success = false
                    };
                }

                var isAssigned = await db.tbl_business_appliance
                    .AnyAsync(x => x.fk_appliance == appliance.appliance_id && !x.is_deleted);

                if (isAssigned)
                {
                    return new ResponseModel()
                    {
                        remarks = "This default appliance is already copied into business appliance lists.",
                        success = false
                    };
                }

                appliance.is_deleted = true;
                appliance.updated_at = DateTime.Now;
                await db.SaveChangesAsync();

                return new ResponseModel()
                {
                    remarks = "Appliance deleted successfully",
                    success = true
                };
            }
            catch (Exception)
            {
                return new ResponseModel()
                {
                    remarks = "There was a fatal error",
                    success = false
                };
            }
        }

        public async Task<ResponseModel<List<ApplianceResponseDTO>>> SeedBusinessDefaultAppliances(string businessId)
        {
            try
            {
                var businessGuid = Guid.Parse(businessId);
                var businessExists = await db.tbl_business.AnyAsync(x => x.business_id == businessGuid && !x.is_deleted);

                if (!businessExists)
                {
                    return new ResponseModel<List<ApplianceResponseDTO>>()
                    {
                        remarks = "Business not found",
                        success = false
                    };
                }

                await AddMissingDefaultAppliancesToBusinessAsync(businessGuid);
                await db.SaveChangesAsync();

                var appliances = await db.tbl_business_appliance
                    .Include(x => x.business)
                    .Include(x => x.utility)
                    .Where(x => x.fk_business == businessGuid && !x.is_deleted)
                    .OrderBy(x => x.utility.utility_name)
                    .ThenBy(x => x.appliance_name)
                    .ToListAsync();

                foreach (var appliance in appliances)
                    await applianceRedisCacheService.SetApplianceAsync(appliance.business_appliance_id);

                return new ResponseModel<List<ApplianceResponseDTO>>()
                {
                    data = mapper.Map<List<ApplianceResponseDTO>>(appliances),
                    remarks = "Business default appliances seeded successfully",
                    success = true
                };
            }
            catch (Exception)
            {
                return new ResponseModel<List<ApplianceResponseDTO>>()
                {
                    remarks = "There was a fatal error",
                    success = false
                };
            }
        }

        public async Task<ResponseModel<ApplianceResponseDTO>> AddBusinessAppliance(AddApplianceDTO requestDto)
        {
            try
            {
                var businessId = Guid.Parse(requestDto.fkBusiness);
                var utilityId = Guid.Parse(requestDto.fkUtility);

                var businessExists = await db.tbl_business.AnyAsync(x => x.business_id == businessId && !x.is_deleted);
                if (!businessExists)
                {
                    return new ResponseModel<ApplianceResponseDTO>()
                    {
                        remarks = "Business not found",
                        success = false
                    };
                }

                var utilityExists = await db.tbl_utility.AnyAsync(x => x.utility_id == utilityId && !x.is_deleted);
                if (!utilityExists)
                {
                    return new ResponseModel<ApplianceResponseDTO>()
                    {
                        remarks = "Utility not found",
                        success = false
                    };
                }

                var existingAppliance = await db.tbl_business_appliance
                    .Where(x => x.fk_business == businessId
                             && x.appliance_name.ToLower() == requestDto.applianceName.ToLower()
                             && x.company_name.ToLower() == requestDto.companyName.ToLower()
                             && x.model_number.ToLower() == requestDto.modelNumber.ToLower()
                             && x.fk_utility == utilityId
                             && !x.is_deleted)
                    .FirstOrDefaultAsync();

                if (existingAppliance != null)
                {
                    return new ResponseModel<ApplianceResponseDTO>()
                    {
                        remarks = "Appliance already exists for this business and utility",
                        success = false
                    };
                }

                var newAppliance = mapper.Map<tbl_business_appliance>(requestDto);
                newAppliance.fk_appliance = null;
                newAppliance.is_default = false;
                newAppliance.is_custom = true;

                await db.tbl_business_appliance.AddAsync(newAppliance);
                await db.SaveChangesAsync();
                await applianceRedisCacheService.SetApplianceAsync(newAppliance.business_appliance_id);

                var appliance = await db.tbl_business_appliance
                    .Include(x => x.business)
                    .Include(x => x.utility)
                    .Where(x => x.business_appliance_id == newAppliance.business_appliance_id)
                    .FirstOrDefaultAsync();

                return new ResponseModel<ApplianceResponseDTO>()
                {
                    data = mapper.Map<ApplianceResponseDTO>(appliance),
                    remarks = "Success",
                    success = true
                };
            }
            catch (Exception)
            {
                return new ResponseModel<ApplianceResponseDTO>()
                {
                    remarks = "There was a fatal error",
                    success = false
                };
            }
        }

        public async Task<ResponseModel<ApplianceResponseDTO>> UpdateBusinessAppliance(UpdateApplianceDTO requestDto)
        {
            try
            {
                var applianceId = Guid.Parse(requestDto.applianceId);
                var existingAppliance = await db.tbl_business_appliance
                    .Include(x => x.business)
                    .Include(x => x.utility)
                    .Where(x => x.business_appliance_id == applianceId && !x.is_deleted)
                    .FirstOrDefaultAsync();

                if (existingAppliance == null)
                {
                    return new ResponseModel<ApplianceResponseDTO>()
                    {
                        remarks = "Business appliance not found",
                        success = false
                    };
                }

                if (!string.IsNullOrWhiteSpace(requestDto.fkBusiness)
                    && Guid.Parse(requestDto.fkBusiness) != existingAppliance.fk_business)
                {
                    return new ResponseModel<ApplianceResponseDTO>()
                    {
                        remarks = "Business appliance cannot be moved to another business",
                        success = false
                    };
                }

                var finalUtilityId = string.IsNullOrWhiteSpace(requestDto.fkUtility)
                    ? existingAppliance.fk_utility
                    : Guid.Parse(requestDto.fkUtility);

                var utilityExists = await db.tbl_utility.AnyAsync(x => x.utility_id == finalUtilityId && !x.is_deleted);
                if (!utilityExists)
                {
                    return new ResponseModel<ApplianceResponseDTO>()
                    {
                        remarks = "Utility not found",
                        success = false
                    };
                }

                var finalApplianceName = string.IsNullOrWhiteSpace(requestDto.applianceName)
                    ? existingAppliance.appliance_name
                    : requestDto.applianceName;
                var finalCompanyName = string.IsNullOrWhiteSpace(requestDto.companyName)
                    ? existingAppliance.company_name
                    : requestDto.companyName;
                var finalModelNumber = string.IsNullOrWhiteSpace(requestDto.modelNumber)
                    ? existingAppliance.model_number
                    : requestDto.modelNumber;

                var duplicateExists = await db.tbl_business_appliance.AnyAsync(x =>
                    x.business_appliance_id != applianceId
                    && x.fk_business == existingAppliance.fk_business
                    && x.fk_utility == finalUtilityId
                    && x.appliance_name.ToLower() == finalApplianceName.ToLower()
                    && x.company_name.ToLower() == finalCompanyName.ToLower()
                    && x.model_number.ToLower() == finalModelNumber.ToLower()
                    && !x.is_deleted);

                if (duplicateExists)
                {
                    return new ResponseModel<ApplianceResponseDTO>()
                    {
                        remarks = "Appliance already exists for this business and utility",
                        success = false
                    };
                }

                mapper.Map(requestDto, existingAppliance);
                existingAppliance.is_default = false;
                existingAppliance.is_custom = true;
                existingAppliance.updated_at = DateTime.Now;
                await db.SaveChangesAsync();
                await applianceRedisCacheService.SetApplianceAsync(existingAppliance.business_appliance_id);
                await applianceRedisCacheService.RefreshSensorChainsForApplianceAsync(existingAppliance.business_appliance_id);

                var appliance = await db.tbl_business_appliance
                    .Include(x => x.business)
                    .Include(x => x.utility)
                    .Where(x => x.business_appliance_id == existingAppliance.business_appliance_id)
                    .FirstOrDefaultAsync();

                return new ResponseModel<ApplianceResponseDTO>()
                {
                    data = mapper.Map<ApplianceResponseDTO>(appliance),
                    remarks = "Success",
                    success = true
                };
            }
            catch (Exception)
            {
                return new ResponseModel<ApplianceResponseDTO>()
                {
                    remarks = "There was a fatal error",
                    success = false
                };
            }
        }

        public async Task<ResponseModel<ApplianceResponseDTO>> GetBusinessApplianceById(string businessApplianceId)
        {
            try
            {
                var appliance = await db.tbl_business_appliance
                    .Include(x => x.business)
                    .Include(x => x.utility)
                    .Where(x => x.business_appliance_id == Guid.Parse(businessApplianceId) && !x.is_deleted)
                    .FirstOrDefaultAsync();

                if (appliance == null)
                {
                    return new ResponseModel<ApplianceResponseDTO>()
                    {
                        remarks = "Business appliance not found",
                        success = false
                    };
                }

                return new ResponseModel<ApplianceResponseDTO>()
                {
                    data = mapper.Map<ApplianceResponseDTO>(appliance),
                    remarks = "Success",
                    success = true
                };
            }
            catch (Exception)
            {
                return new ResponseModel<ApplianceResponseDTO>()
                {
                    remarks = "There was a fatal error",
                    success = false
                };
            }
        }

        public async Task<ResponseModel<List<ApplianceResponseDTO>>> GetBusinessAppliances(string businessId)
        {
            try
            {
                var businessGuid = Guid.Parse(businessId);

                var appliances = await db.tbl_business_appliance
                    .Include(x => x.business)
                    .Include(x => x.utility)
                    .Where(x => x.fk_business == businessGuid && !x.is_deleted)
                    .OrderBy(x => x.utility.utility_name)
                    .ThenBy(x => x.appliance_name)
                    .ToListAsync();

                if (appliances.Any())
                {
                    return new ResponseModel<List<ApplianceResponseDTO>>()
                    {
                        data = mapper.Map<List<ApplianceResponseDTO>>(appliances),
                        remarks = "Success",
                        success = true
                    };
                }

                return new ResponseModel<List<ApplianceResponseDTO>>()
                {
                    remarks = "No appliance found for this business",
                    success = false
                };
            }
            catch (Exception)
            {
                return new ResponseModel<List<ApplianceResponseDTO>>()
                {
                    remarks = "There was a fatal error",
                    success = false
                };
            }
        }

        public async Task<ResponseModel<List<ApplianceResponseDTO>>> GetBusinessAppliancesByUtilityId(string businessId, string utilityId)
        {
            try
            {
                var businessGuid = Guid.Parse(businessId);
                var utilityGuid = Guid.Parse(utilityId);

                var appliances = await db.tbl_business_appliance
                    .Include(x => x.business)
                    .Include(x => x.utility)
                    .Where(x => x.fk_business == businessGuid
                             && x.fk_utility == utilityGuid
                             && x.is_active
                             && !x.is_deleted)
                    .OrderBy(x => x.appliance_name)
                    .ToListAsync();

                if (appliances.Any())
                {
                    return new ResponseModel<List<ApplianceResponseDTO>>()
                    {
                        data = mapper.Map<List<ApplianceResponseDTO>>(appliances),
                        remarks = "Success",
                        success = true
                    };
                }

                return new ResponseModel<List<ApplianceResponseDTO>>()
                {
                    remarks = "No business appliance found for this utility",
                    success = false
                };
            }
            catch (Exception)
            {
                return new ResponseModel<List<ApplianceResponseDTO>>()
                {
                    remarks = "There was a fatal error",
                    success = false
                };
            }
        }

        public async Task<ResponseModel> DeleteBusinessApplianceById(string businessApplianceId)
        {
            try
            {
                var applianceId = Guid.Parse(businessApplianceId);
                var appliance = await db.tbl_business_appliance
                    .Where(x => x.business_appliance_id == applianceId && !x.is_deleted)
                    .FirstOrDefaultAsync();

                if (appliance == null)
                {
                    return new ResponseModel()
                    {
                        remarks = "Business appliance not found",
                        success = false
                    };
                }

                var isAssigned = await db.tbl_sensor_appliance
                    .AnyAsync(x => x.fk_appliance == applianceId && !x.is_deleted && x.is_active);

                if (isAssigned)
                {
                    return new ResponseModel()
                    {
                        remarks = "This business appliance is assigned to a sensor. Remove sensor assignment first.",
                        success = false
                    };
                }

                appliance.is_deleted = true;
                appliance.is_active = false;
                appliance.updated_at = DateTime.Now;
                await db.SaveChangesAsync();
                await applianceRedisCacheService.DeleteApplianceAsync(appliance.business_appliance_id);

                return new ResponseModel()
                {
                    remarks = "Business appliance deleted successfully",
                    success = true
                };
            }
            catch (Exception)
            {
                return new ResponseModel()
                {
                    remarks = "There was a fatal error",
                    success = false
                };
            }
        }

        public async Task<ResponseModel<List<ApplianceResponseDTO>>> SeedDefaultAppliances()
        {
            try
            {
                var utilities = await db.tbl_utility
                    .Where(x => !x.is_deleted)
                    .ToListAsync();

                Guid utilityId(string name)
                {
                    return utilities
                        .First(x => x.utility_name.ToLower() == name.ToLower())
                        .utility_id;
                }

                var defaults = new List<tbl_appliance>
                {
                    new() { fk_utility = utilityId("HVAC"), appliance_name = "Split AC",  company_name = "Generic", model_number = "1.5 Ton", rated_voltage = 220, min_current = 4, max_current = 12, min_power = 800, max_power = 2500, standby_power = 15, normal_power_factor = 0.85f, is_default = true, is_custom = false, is_active = true },
                    new() { fk_utility = utilityId("HVAC"), appliance_name = "Central AC",  company_name = "Generic", model_number = "Standard", rated_voltage = 220, min_current = 8, max_current = 25, min_power = 1800, max_power = 6000, standby_power = 30, normal_power_factor = 0.85f, is_default = true, is_custom = false, is_active = true },
                    new() { fk_utility = utilityId("HVAC"), appliance_name = "Exhaust Fan", company_name = "Generic", model_number = "Standard", rated_voltage = 220, min_current = 0.2f, max_current = 1.5f, min_power = 40, max_power = 300, standby_power = 0, normal_power_factor = 0.80f, is_default = true, is_custom = false, is_active = true },

                    new() { fk_utility = utilityId("Computing"), appliance_name = "Desktop PC",  company_name = "Generic", model_number = "Standard", rated_voltage = 220, min_current = 0.5f, max_current = 3, min_power = 100, max_power = 650, standby_power = 10, normal_power_factor = 0.75f, is_default = true, is_custom = false, is_active = true },
                    new() { fk_utility = utilityId("Computing"), appliance_name = "Laptop",  company_name = "Generic", model_number = "Standard", rated_voltage = 220, min_current = 0.2f, max_current = 1.2f, min_power = 30, max_power = 150, standby_power = 5, normal_power_factor = 0.75f, is_default = true, is_custom = false, is_active = true },
                    new() { fk_utility = utilityId("Computing"), appliance_name = "Server", company_name = "Generic", model_number = "Standard", rated_voltage = 220, min_current = 1, max_current = 8, min_power = 250, max_power = 1800, standby_power = 80, normal_power_factor = 0.90f, is_default = true, is_custom = false, is_active = true },
                    new() { fk_utility = utilityId("Computing"), appliance_name = "Printer",  company_name = "Generic", model_number = "Standard", rated_voltage = 220, min_current = 0.1f, max_current = 5, min_power = 10, max_power = 1000, standby_power = 10, normal_power_factor = 0.70f, is_default = true, is_custom = false, is_active = true },
                    new() { fk_utility = utilityId("Computing"), appliance_name = "Router",  company_name = "Generic", model_number = "Standard", rated_voltage = 220, min_current = 0.05f, max_current = 0.3f, min_power = 5, max_power = 40, standby_power = 5, normal_power_factor = 0.70f, is_default = true, is_custom = false, is_active = true },

                    new() { fk_utility = utilityId("Lighting"), appliance_name = "LED Light", company_name = "Generic", model_number = "Standard", rated_voltage = 220, min_current = 0.02f, max_current = 0.4f, min_power = 5, max_power = 80, standby_power = 0, normal_power_factor = 0.90f, is_default = true, is_custom = false, is_active = true },
                    new() { fk_utility = utilityId("Lighting"), appliance_name = "Tube Light",  company_name = "Generic", model_number = "Standard", rated_voltage = 220, min_current = 0.05f, max_current = 0.6f, min_power = 10, max_power = 120, standby_power = 0, normal_power_factor = 0.85f, is_default = true, is_custom = false, is_active = true },
                    new() { fk_utility = utilityId("Lighting"), appliance_name = "Flood Light",  company_name = "Generic", model_number = "Standard", rated_voltage = 220, min_current = 0.2f, max_current = 2.5f, min_power = 50, max_power = 500, standby_power = 0, normal_power_factor = 0.90f, is_default = true, is_custom = false, is_active = true },

                    new() { fk_utility = utilityId("Miscellaneous"), appliance_name = "Refrigerator",  company_name = "Generic", model_number = "Standard", rated_voltage = 220, min_current = 0.5f, max_current = 4, min_power = 100, max_power = 800, standby_power = 20, normal_power_factor = 0.75f, is_default = true, is_custom = false, is_active = true },
                    new() { fk_utility = utilityId("Miscellaneous"), appliance_name = "Microwave", company_name = "Generic", model_number = "Standard", rated_voltage = 220, min_current = 2, max_current = 8, min_power = 500, max_power = 1800, standby_power = 3, normal_power_factor = 0.90f, is_default = true, is_custom = false, is_active = true },
                    new() { fk_utility = utilityId("Miscellaneous"), appliance_name = "Water Dispenser",  company_name = "Generic", model_number = "Standard", rated_voltage = 220, min_current = 0.3f, max_current = 4, min_power = 80, max_power = 800, standby_power = 10, normal_power_factor = 0.75f, is_default = true, is_custom = false, is_active = true },
                    new() { fk_utility = utilityId("Miscellaneous"), appliance_name = "Pump",  company_name = "Generic", model_number = "Standard", rated_voltage = 220, min_current = 1, max_current = 12, min_power = 250, max_power = 2500, standby_power = 0, normal_power_factor = 0.80f, is_default = true, is_custom = false, is_active = true }
                };

                foreach (var item in defaults)
                {
                    bool exists = await db.tbl_appliance.AnyAsync(x =>
                        x.appliance_name.ToLower() == item.appliance_name.ToLower()
                        && x.model_number.ToLower() == item.model_number.ToLower()
                        && x.fk_utility == item.fk_utility
                        && x.is_default
                        && !x.is_deleted);

                    if (!exists)
                    {
                        await db.tbl_appliance.AddAsync(item);
                    }
                }

                await db.SaveChangesAsync();

                var appliances = await db.tbl_appliance
                    .Include(x => x.utility)
                    .Where(x => !x.is_deleted)
                    .OrderBy(x => x.utility.utility_name)
                    .ThenBy(x => x.appliance_name)
                    .ToListAsync();

                return new ResponseModel<List<ApplianceResponseDTO>>()
                {
                    data = mapper.Map<List<ApplianceResponseDTO>>(appliances),
                    remarks = "Default appliances seeded successfully",
                    success = true
                };
            }
            catch (Exception)
            {
                return new ResponseModel<List<ApplianceResponseDTO>>()
                {
                    remarks = "There was a fatal error. Make sure HVAC, Computing, Lighting, and Miscellaneous utilities exist.",
                    success = false
                };
            }
        }

        private async Task AddMissingDefaultAppliancesToBusinessAsync(Guid businessId)
        {
            var defaultAppliances = await db.tbl_appliance
                .Where(x => x.is_default && x.is_active && !x.is_deleted)
                .ToListAsync();

            var copiedDefaultIds = await db.tbl_business_appliance
                .Where(x => x.fk_business == businessId && x.fk_appliance.HasValue && !x.is_deleted)
                .Select(x => x.fk_appliance!.Value)
                .ToListAsync();

            foreach (var defaultAppliance in defaultAppliances)
            {
                if (copiedDefaultIds.Contains(defaultAppliance.appliance_id))
                    continue;

                await db.tbl_business_appliance.AddAsync(CreateBusinessApplianceFromDefault(businessId, defaultAppliance));
            }
        }

        private static tbl_business_appliance CreateBusinessApplianceFromDefault(Guid businessId, tbl_appliance defaultAppliance)
        {
            return new tbl_business_appliance
            {
                fk_business = businessId,
                fk_appliance = defaultAppliance.appliance_id,
                appliance_name = defaultAppliance.appliance_name,
                company_name = defaultAppliance.company_name,
                model_number = defaultAppliance.model_number,
                rated_voltage = defaultAppliance.rated_voltage,
                min_current = defaultAppliance.min_current,
                max_current = defaultAppliance.max_current,
                min_power = defaultAppliance.min_power,
                max_power = defaultAppliance.max_power,
                standby_power = defaultAppliance.standby_power,
                normal_power_factor = defaultAppliance.normal_power_factor,
                description = defaultAppliance.description,
                is_shiftable = defaultAppliance.is_shiftable,
                priority_level = defaultAppliance.priority_level,
                normal_operating_hours = defaultAppliance.normal_operating_hours,
                can_auto_control = defaultAppliance.can_auto_control,
                is_critical = defaultAppliance.is_critical,
                allow_optimization_suggestions = defaultAppliance.allow_optimization_suggestions,
                allowed_shift_start_time = defaultAppliance.allowed_shift_start_time,
                allowed_shift_end_time = defaultAppliance.allowed_shift_end_time,
                minimum_on_duration_minutes = defaultAppliance.minimum_on_duration_minutes,
                minimum_off_duration_minutes = defaultAppliance.minimum_off_duration_minutes,
                is_default = true,
                is_custom = false,
                is_active = defaultAppliance.is_active,
                fk_utility = defaultAppliance.fk_utility
            };
        }
    }
}
