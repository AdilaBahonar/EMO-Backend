using EMO.Models.DTOs.SectionDTOs;
using EMO.Models.DTOs.ResponseDTO;

namespace EMO.Repositories.SectionServicesRepo
{
    public interface ISectionServices
    {
        Task<ResponseModel<SectionResponseDTO>> AddSection(AddSectionDTO requestDto);
        Task<ResponseModel<SectionResponseDTO>> UpdateSection(UpdateSectionDTO requestDto);
        Task<ResponseModel<SectionResponseDTO>> GetSectionById(string sectionId);
        Task<ResponseModel<List<SectionResponseDTO>>> GetAllSections();
        Task<ResponseModel> DeleteSectionById(string sectionId);
        public Task<ResponseModel<List<SectionResponseDTO>>> GetSectionsByFloorId(string floorId);
        public Task<ResponseModel<List<SectionResponseDTO>>> GetSectionByBusinessId(string businessId);
    }
}
