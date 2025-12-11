using APIProduct.Models.DTOs.SectionDTOs;
using P3AHR.Models.DTOs.ResponseDTO;

namespace APIProduct.Repositories.SectionServicesRepo
{
    public interface ISectionServices
    {
        Task<ResponseModel<SectionResponseDTO>> AddSection(AddSectionDTO requestDto);
        Task<ResponseModel<SectionResponseDTO>> UpdateSection(UpdateSectionDTO requestDto);
        Task<ResponseModel<SectionResponseDTO>> GetSectionById(string sectionId);
        Task<ResponseModel<List<SectionResponseDTO>>> GetAllSections();
        Task<ResponseModel> DeleteSectionById(string sectionId);
    }
}
