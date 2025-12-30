using EMO.Models.DTOs.OfficeDTOs;
using 
    .Models.DTOs.ResponseDTO;

namespace EMO.Repositories.OfficeServicesRepo
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
