using EMO.Models.DTOs.AgreementDTOs;
using EMO.Models.DTOs.ResponseDTO;

namespace EMO.Repositories.AgreementServicesRepo
{
    public interface IAgreementServices
    {
        Task<ResponseModel<AgreementResponseDTO>> AddAgreement(AddAgreementDTO requestDto);
        Task<ResponseModel<AgreementResponseDTO>> UpdateAgreement(UpdateAgreementDTO requestDto);
        Task<ResponseModel<AgreementResponseDTO>> GetAgreementById(string agreementId);
        Task<ResponseModel<List<AgreementResponseDTO>>> GetAllAgreements();
        Task<ResponseModel> DeleteAgreementById(string agreementId);
    }

}
