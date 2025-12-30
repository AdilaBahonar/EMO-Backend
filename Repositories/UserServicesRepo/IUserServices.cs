using AutoMapper;
using P3AHR.Models.DBModels.DBTables;
using P3AHR.Models.DTOs.UserDTOs;
using P3AHR.Models.DTOs.ResponseDTO;

namespace P3AHR.Repositories.UserServicesRepo
{
    public interface IUserServices
    {
        public Task<ResponseModel<UserResponseDTO>> AddUser(AddUserDTO requestDto);
        public Task<ResponseModel<UserResponseDTO>> UpdateUser(UpdateUserDTO requestDto);
        public Task<ResponseModel<UserResponseDTO>> GetUserById(string userId);
        public Task<ResponseModel<List<UserResponseDTO>>> GetAllUsers();
        public Task<ResponseModel<List<UserResponseDTO>>> GetByUserTypeId(string userTypeId);
        public Task<ResponseModel> DeleteUserById(string userId);
    }
}
