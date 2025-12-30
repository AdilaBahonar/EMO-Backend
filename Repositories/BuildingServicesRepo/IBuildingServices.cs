using APIProduct.Models.DTOs.BuildingDTOs;
using P3AHR.Models.DTOs.ResponseDTO;

namespace APIProduct.Repositories.BuildingServicesRepo
{
    public interface IBuildingServices
    {
        Task<ResponseModel<BuildingResponseDTO>> AddBuilding(AddBuildingDTO requestDto);
        Task<ResponseModel<BuildingResponseDTO>> UpdateBuilding(UpdateBuildingDTO requestDto);
        Task<ResponseModel<BuildingResponseDTO>> GetBuildingById(string buildingId);
        Task<ResponseModel<List<BuildingResponseDTO>>> GetAllBuildings();
        Task<ResponseModel> DeleteBuildingById(string buildingId);
    }

}
