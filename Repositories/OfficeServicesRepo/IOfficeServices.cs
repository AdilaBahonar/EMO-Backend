using APIProduct.Models.DTOs.OfficeDTOs;
using P3AHR.Models.DTOs.ResponseDTO;

namespace APIProduct.Repositories.OfficeServicesRepo
{
    public interface IOfficeServices
    {
        Task<ResponseModel<OfficeResponseDTO>> AddOffice(AddOfficeDTO requestDto);
        Task<ResponseModel<OfficeResponseDTO>> UpdateOffice(UpdateOfficeDTO requestDto);
        Task<ResponseModel<OfficeResponseDTO>> GetOfficeById(string officeId);
        Task<ResponseModel<List<OfficeResponseDTO>>> GetAllOffices();
        Task<ResponseModel> DeleteOfficeById(string officeId);
    }
}
