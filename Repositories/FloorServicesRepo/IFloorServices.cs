using APIProduct.Models.DTOs.FloorDTOs;
using P3AHR.Models.DTOs.ResponseDTO;

namespace APIProduct.Repositories.FloorServicesRepo
{
    public interface IFloorServices
    {
        Task<ResponseModel<FloorResponseDTO>> AddFloor(AddFloorDTO requestDto);
        Task<ResponseModel<FloorResponseDTO>> UpdateFloor(UpdateFloorDTO requestDto);
        Task<ResponseModel<FloorResponseDTO>> GetFloorById(string floorId);
        Task<ResponseModel<List<FloorResponseDTO>>> GetAllFloors();
        Task<ResponseModel> DeleteFloorById(string floorId);
    }
}
