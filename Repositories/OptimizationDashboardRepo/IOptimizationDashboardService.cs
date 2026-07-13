using EMO.Models.DTOs.OptimizationDashboardDTOs;
using EMO.Models.DTOs.ResponseDTO;

namespace EMO.Repositories.OptimizationDashboardRepo
{
    public interface IOptimizationDashboardService
    {
        Task<ResponseModel<OptimizationDashboardResponseDTO>> GetOptimizationDashboardAsync(string level, Guid id, OptimizationQueryParams q, bool includeBusinessSuggestions = true);
    }
}
