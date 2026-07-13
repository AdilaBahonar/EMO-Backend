using EMO.Models.DTOs.EnergyConfigurationDTOs;
using EMO.Models.DTOs.ResponseDTO;

namespace EMO.Repositories.EnergyConfigurationRepo
{
    public interface IEnergyConfigurationService
    {
        Task<ResponseModel<EnergyConfigurationDTO>> GetByBusinessId(string businessId);
        Task<ResponseModel<EnergyConfigurationDTO>> Save(EnergyConfigurationDTO request);
    }
}
