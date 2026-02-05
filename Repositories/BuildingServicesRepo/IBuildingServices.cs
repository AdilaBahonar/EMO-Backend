using EMO.Models.DTOs.BuildingDTOs;
using EMO.Models.DTOs.ResponseDTO;

namespace EMO.Repositories.BuildingServicesRepo
{
    public interface IBuildingServices
    {
        Task<ResponseModel<BuildingResponseDTO>> AddBuilding(AddBuildingDTO requestDto);
        Task<ResponseModel<BuildingResponseDTO>> UpdateBuilding(UpdateBuildingDTO requestDto);
        Task<ResponseModel<BuildingResponseDTO>> GetBuildingById(string buildingId);
        Task<ResponseModel<List<BuildingResponseDTO>>> GetAllBuildings();
        Task<ResponseModel> DeleteBuildingById(string buildingId);
        public Task<ResponseModel<List<BuildingResponseDTO>>> GetBuidlingByFacilityId(string facilityId);
    }

}
