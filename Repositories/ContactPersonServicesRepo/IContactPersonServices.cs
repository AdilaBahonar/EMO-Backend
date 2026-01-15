using EMO.Models.DTOs.ContactPersonDTOs;
using P3AHR.Models.DTOs.ResponseDTO;

namespace EMO.Repositories.ContactPersonServicesRepo
{
    public interface IContactPersonServices
    {
        Task<ResponseModel<ContactPersonResponseDTO>> AddContactPerson(AddContactPersonDTO requestDto);
        Task<ResponseModel<ContactPersonResponseDTO>> UpdateContactPerson(UpdateContactPersonDTO requestDto);
        Task<ResponseModel<ContactPersonResponseDTO>> GetContactPersonById(string contactPersonId);
        Task<ResponseModel<List<ContactPersonResponseDTO>>> GetAllContactPersons();
        public async Task<ResponseModel> DeleteContactPersonById(string contactPersonId);

    }
}
