using EMO.Models.DTOs.FloorDTOs;
using EMO.Models.DTOs.ResponseDTO;

namespace EMO.Repositories.FloorServicesRepo
{
    public interface IFloorServices
    {
        Task<ResponseModel<FloorResponseDTO>> AddFloor(AddFloorDTO requestDto);
        Task<ResponseModel<FloorResponseDTO>> UpdateFloor(UpdateFloorDTO requestDto);
        Task<ResponseModel<FloorResponseDTO>> GetFloorById(string floorId);
        Task<ResponseModel<List<FloorResponseDTO>>> GetAllFloors();
        Task<ResponseModel> DeleteFloorById(string floorId);
        public Task<ResponseModel<List<FloorResponseDTO>>> GetFloorByBuildingId(string buildingId);
    }
}
