using APIProduct.Models.DTOs.PocDTOs;
using P3AHR.Models.DTOs.ResponseDTO;

namespace APIProduct.Repositories.PocServicesRepo
{
    public interface IPocServices
    {
        Task<ResponseModel<PocResponseDTO>> AddPoc(AddPocDTO requestDto);
        Task<ResponseModel<PocResponseDTO>> UpdatePoc(UpdatePocDTO requestDto);
        Task<ResponseModel<PocResponseDTO>> GetPocById(string pocId);
        Task<ResponseModel<List<PocResponseDTO>>> GetAllPoces();
        Task<ResponseModel> DeletePocById(string pocId);

    }
}
