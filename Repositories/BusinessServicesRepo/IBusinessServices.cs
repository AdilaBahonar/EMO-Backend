using EMO.Models.DTOs.BusinessDTOs;
using EMO.Models.DTOs.ResponseDTO;

namespace EMO.Repositories.BusinessServicesRepo
{
    public interface IBusinessServices
    {
        Task<ResponseModel<BusinessResponseDTO>> AddBusiness(AddBusinessDTO requestDto);
        Task<ResponseModel<BusinessResponseDTO>> UpdateBusiness(UpdateBusinessDTO requestDto);
        Task<ResponseModel<BusinessResponseDTO>> GetBusinessById(string businessId);
        Task<ResponseModel<List<BusinessResponseDTO>>> GetAllBusinesses();
        Task<ResponseModel> DeleteBusinessById(string businessId);
    }
}
