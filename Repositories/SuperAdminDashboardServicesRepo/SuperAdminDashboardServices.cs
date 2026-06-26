using EMO.Models.DBModels;
using EMO.Models.DTOs.ResponseDTO;
using EMO.Models.DTOs.SuperAdminDashboardDTOs;
using Microsoft.EntityFrameworkCore;

namespace EMO.Repositories.SuperAdminDashboardServicesRepo
{
    public class SuperAdminDashboardServices : ISuperAdminDashboardServices
    {
        private readonly DBUserManagementContext db;

        public SuperAdminDashboardServices(DBUserManagementContext db)
        {
            this.db = db;
        }

        public async Task<ResponseModel<SuperAdminDashboardResponseDTO>> GetDashboard()
        {
            try
            {
                var totalBusinesses = await db.tbl_business.CountAsync(x => !x.is_deleted);
                var totalFacilities = await db.tbl_facility.CountAsync(x => !x.is_deleted);
                var totalBuildings = await db.tbl_building.CountAsync(x => !x.is_deleted);
                var totalFloors = await db.tbl_floor.CountAsync(x => !x.is_deleted);
                var totalSections = await db.tbl_section.CountAsync(x => !x.is_deleted);
                var totalOffices = await db.tbl_office.CountAsync(x => !x.is_deleted);

                //var activeOffices = await db.tbl_office.CountAsync(x => !x.is_deleted && x.is_active);
                //var inactiveOffices = await db.tbl_office.CountAsync(x => !x.is_deleted && !x.is_active);
                //var occupiedOffices = await db.tbl_office.CountAsync(x => !x.is_deleted && x.is_occupied);
                //var vacantOffices = await db.tbl_office.CountAsync(x => !x.is_deleted && !x.is_occupied);

                //double occupiedPercentage = totalOffices > 0
                //    ? Math.Round((double)occupiedOffices / totalOffices * 100, 2)
                //    : 0;

                //double vacantPercentage = totalOffices > 0
                //    ? Math.Round((double)vacantOffices / totalOffices * 100, 2)
                //    : 0;

                var businessWiseSummary = await GetBusinessWiseSummaryData();

                //var recentBusinesses = await db.tbl_business
                //    .Where(x => !x.is_deleted)
                //    .OrderByDescending(x => x.created_at)
                //    .Take(5)
                //    .Select(x => new RecentActivityDTO
                //    {
                //        type = "Business",
                //        title = x.business_name,
                //        businessName = x.business_name,
                //        createdAt = x.created_at.ToString("yyyy-MM-dd HH:mm")
                //    })
                //    .ToListAsync();

                //var recentFacilities = await db.tbl_facility
                //    .Include(x => x.business)
                //    .Where(x => !x.is_deleted)
                //    .OrderByDescending(x => x.created_at)
                //    .Take(5)
                //    .Select(x => new RecentActivityDTO
                //    {
                //        type = "Facility",
                //        title = x.facility_name,
                //        businessName = x.business.business_name,
                //        createdAt = x.created_at.ToString("yyyy-MM-dd HH:mm")
                //    })
                //    .ToListAsync();

                //var recentOffices = await db.tbl_office
                //    .Include(x => x.business)
                //    .Where(x => !x.is_deleted)
                //    .OrderByDescending(x => x.created_at)
                //    .Take(5)
                //    .Select(x => new RecentActivityDTO
                //    {
                //        type = "Office",
                //        title = x.office_name,
                //        businessName = x.business.business_name,
                //        createdAt = x.created_at.ToString("yyyy-MM-dd HH:mm")
                //    })
                //    .ToListAsync();

                //var recentActivities = recentBusinesses
                //    .Concat(recentFacilities)
                //    .Concat(recentOffices)
                //    .OrderByDescending(x => x.createdAt)
                //    .Take(10)
                //    .ToList();

                var response = new SuperAdminDashboardResponseDTO
                {
                    counts = new DashboardCountDTO
                    {
                        totalBusinesses = totalBusinesses,
                        totalFacilities = totalFacilities,
                        totalBuildings = totalBuildings,
                        totalFloors = totalFloors,
                        totalSections = totalSections,
                        totalOffices = totalOffices
                    },
                    //officeStatus = new OfficeStatusDTO
                    //{
                    //    activeOffices = activeOffices,
                    //    inactiveOffices = inactiveOffices,
                    //    occupiedOffices = occupiedOffices,
                    //    vacantOffices = vacantOffices,
                    //    occupiedPercentage = occupiedPercentage,
                    //    vacantPercentage = vacantPercentage
                    //},
                    businessWiseSummary = businessWiseSummary,
                    //recentActivities = recentActivities
                };

                return new ResponseModel<SuperAdminDashboardResponseDTO>
                {
                    data = response,
                    remarks = "Success",
                    success = true
                };
            }
            catch (Exception ex)
            {
                return new ResponseModel<SuperAdminDashboardResponseDTO>
                {
                    remarks = "There was a fatal error",
                    success = false
                };
            }
        }

        public async Task<ResponseModel<List<BusinessWiseDashboardDTO>>> GetBusinessWiseSummary()
        {
            try
            {
                var data = await GetBusinessWiseSummaryData();

                if (data.Any())
                {
                    return new ResponseModel<List<BusinessWiseDashboardDTO>>
                    {
                        data = data,
                        remarks = "Success",
                        success = true
                    };
                }

                return new ResponseModel<List<BusinessWiseDashboardDTO>>
                {
                    remarks = "No business summary found",
                    success = false
                };
            }
            catch (Exception ex)
            {
                return new ResponseModel<List<BusinessWiseDashboardDTO>>
                {
                    remarks = "There was a fatal error",
                    success = false
                };
            }
        }

        private async Task<List<BusinessWiseDashboardDTO>> GetBusinessWiseSummaryData()
        {
            var data = await db.tbl_business
                .Where(b => !b.is_deleted)
                .Select(b => new BusinessWiseDashboardDTO
                {
                    businessId = b.business_id.ToString(),
                    businessName = b.business_name,

                    facilityCount = db.tbl_facility
                        .Count(f => f.fk_business == b.business_id && !f.is_deleted),

                    buildingCount = db.tbl_building
                        .Count(x => x.fk_business == b.business_id && !x.is_deleted),

                    floorCount = db.tbl_floor
                        .Count(x => x.fk_business == b.business_id && !x.is_deleted),

                    sectionCount = db.tbl_section
                        .Count(x => x.fk_business == b.business_id && !x.is_deleted),

                    officeCount = db.tbl_office
                        .Count(x => x.fk_business == b.business_id && !x.is_deleted),

                    activeOfficeCount = db.tbl_office
                        .Count(x => x.fk_business == b.business_id && !x.is_deleted && x.is_active),

                    occupiedOfficeCount = db.tbl_office
                        .Count(x => x.fk_business == b.business_id && !x.is_deleted && x.is_occupied),

                    vacantOfficeCount = db.tbl_office
                        .Count(x => x.fk_business == b.business_id && !x.is_deleted && !x.is_occupied),
                    sensorsCount = db.tbl_sensor.Count(x=>x.device.fk_business == b.business_id && !x.is_deleted),
                    devicesCount = db.tbl_device.Count(x => x.fk_business == b.business_id && !x.is_deleted),
                })
                .OrderByDescending(x => x.officeCount)
                .ToListAsync();

            return data;
        }

    }
}