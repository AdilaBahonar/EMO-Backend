using EMO.Models.DTOs.ResponseDTO;
using EMO.Models.DTOs.SuperAdminDashboardDTOs;

namespace EMO.Repositories.SuperAdminDashboardServicesRepo
{
    public interface ISuperAdminDashboardServices
    {
        Task<ResponseModel<SuperAdminDashboardResponseDTO>> GetDashboard();
        Task<ResponseModel<List<BusinessWiseDashboardDTO>>> GetBusinessWiseSummary();



    }
}
