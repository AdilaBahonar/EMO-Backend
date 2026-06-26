namespace EMO.Models.DTOs.SuperAdminDashboardDTOs
{
    public class SuperAdminDashboardResponseDTO
    {
        public DashboardCountDTO counts { get; set; } = new DashboardCountDTO();
        //public OfficeStatusDTO officeStatus { get; set; } = new OfficeStatusDTO();
        public List<BusinessWiseDashboardDTO> businessWiseSummary { get; set; } = new List<BusinessWiseDashboardDTO>();
        //public List<RecentActivityDTO> recentActivities { get; set; } = new List<RecentActivityDTO>();
    }

    public class DashboardCountDTO
    {
        public int totalBusinesses { get; set; } = 0;
        public int totalFacilities { get; set; } = 0;
        public int totalBuildings { get; set; } = 0;
        public int totalFloors { get; set; } = 0;
        public int totalSections { get; set; } = 0;
        public int totalOffices { get; set; } = 0;
    }

    //public class OfficeStatusDTO
    //{
    //    public int activeOffices { get; set; } = 0;
    //    public int inactiveOffices { get; set; } = 0;
    //    public int occupiedOffices { get; set; } = 0;
    //    public int vacantOffices { get; set; } = 0;
    //    public double occupiedPercentage { get; set; } = 0;
    //    public double vacantPercentage { get; set; } = 0;
    //}

    public class BusinessWiseDashboardDTO
    {
        public string businessId { get; set; } = string.Empty;
        public string businessName { get; set; } = string.Empty;

        public int facilityCount { get; set; } = 0;
        public int buildingCount { get; set; } = 0;
        public int floorCount { get; set; } = 0;
        public int sectionCount { get; set; } = 0;
        public int officeCount { get; set; } = 0;

        public int activeOfficeCount { get; set; } = 0;
        public int occupiedOfficeCount { get; set; } = 0;
        public int vacantOfficeCount { get; set; } = 0;
        public int devicesCount { get; set; } = 0;
        public int sensorsCount { get; set; } = 0;
    }

    //public class RecentActivityDTO
    //{
    //    public string type { get; set; } = string.Empty;
    //    public string title { get; set; } = string.Empty;
    //    public string businessName { get; set; } = string.Empty;
    //    public string createdAt { get; set; } = string.Empty;
    //}
}
