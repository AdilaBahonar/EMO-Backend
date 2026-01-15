using AutoMapper;
using EMO.Models.DBModels.DBTables;
using EMO.Models.DTOs.UserDTOs;
using EMO.Models.DTOs.ResponseDTO;
using P3AHR.Models.DTOs.ResponseDTO;

namespace EMO.Repositories.UserServicesRepo
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
