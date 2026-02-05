using EMO.Models.DTOs.OfficeDTOs;
using EMO.Models.DTOs.ResponseDTO;

namespace EMO.Repositories.OfficeServicesRepo
{
    public interface IOfficeServices
    {
        Task<ResponseModel<OfficeResponseDTO>> AddOffice(AddOfficeDTO requestDto);
        Task<ResponseModel<OfficeResponseDTO>> UpdateOffice(UpdateOfficeDTO requestDto);
        Task<ResponseModel<OfficeResponseDTO>> GetOfficeById(string officeId);
        Task<ResponseModel<List<OfficeResponseDTO>>> GetAllOffices();
        Task<ResponseModel> DeleteOfficeById(string officeId);
        public Task<ResponseModel<List<OfficeResponseDTO>>> GetOfficeBySectionId(string sectionId);
        public Task<ResponseModel<List<OfficeResponseDTO>>> GetAvailableOfficesBySectionId(string sectionId);
    }
}
