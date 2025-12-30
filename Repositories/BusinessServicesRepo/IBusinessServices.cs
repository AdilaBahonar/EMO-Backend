using APIProduct.Models.DTOs.BusinessDTOs;
using P3AHR.Models.DTOs.ResponseDTO;

namespace APIProduct.Repositories.BusinessServicesRepo
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
