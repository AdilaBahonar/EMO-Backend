using EMO.Models.DBModels.DBTables;
using EMO.Models.DTOs.BuildingDTOs;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using EMO.Models.DBModels;
using EMO.Models.DTOs.ResponseDTO;

namespace EMO.Repositories.BuildingServicesRepo
{
    public class BuildingServices : IBuildingServices
    {
        private readonly DBUserManagementContext db;
        private readonly IMapper mapper;

        public BuildingServices(DBUserManagementContext db, IMapper mapper)
        {
            this.db = db;
            this.mapper = mapper;
        }

        public async Task<ResponseModel<BuildingResponseDTO>> AddBuilding(AddBuildingDTO requestDto)
        {
            try
            {
                var existingBuilding = await db.tbl_building
                    .Where(b => b.building_name.ToLower() == requestDto.buildingName.ToLower())
                    .FirstOrDefaultAsync();

                if (existingBuilding == null)
                {
                    var newBuilding = mapper.Map<tbl_building>(requestDto);
                    await db.tbl_building.AddAsync(newBuilding);
                    await db.SaveChangesAsync();

                    return new ResponseModel<BuildingResponseDTO>()
                    {
                        data = mapper.Map<BuildingResponseDTO>(newBuilding),
                        remarks = "Success",
                        success = true
                    };
                }
                else
                {
                    return new ResponseModel<BuildingResponseDTO>()
                    {
                        remarks = "Building Already Exists",
                        success = false
                    };
                }
            }
            catch (Exception ex)
            {
                return new ResponseModel<BuildingResponseDTO>()
                {
                    remarks = $"There was a fatal error: {ex}",
                    success = false
                };
            }
        }

        public async Task<ResponseModel<BuildingResponseDTO>> UpdateBuilding(UpdateBuildingDTO requestDto)
        {
            try
            {
                var existingBuilding = await db.tbl_building
                    .Where(b => b.building_id == Guid.Parse(requestDto.buildingId))
                    .FirstOrDefaultAsync();

                if (existingBuilding != null)
                {
                    mapper.Map(requestDto, existingBuilding);
                    await db.SaveChangesAsync();

                    return new ResponseModel<BuildingResponseDTO>()
                    {
                        data = mapper.Map<BuildingResponseDTO>(existingBuilding),
                        remarks = "Success",
                        success = true
                    };
                }
                else
                {
                    return new ResponseModel<BuildingResponseDTO>()
                    {
                        remarks = "No Record Found",
                        success = false
                    };
                }
            }
            catch (Exception ex)
            {
                return new ResponseModel<BuildingResponseDTO>()
                {
                    remarks = $"There was a fatal error: {ex}",
                    success = false
                };
            }
        }

        public async Task<ResponseModel<BuildingResponseDTO>> GetBuildingById(string buildingId)
        {
            try
            {
                var building = await db.tbl_building
                    .Where(b => b.building_id == Guid.Parse(buildingId))
                    .FirstOrDefaultAsync();

                if (building != null)
                {
                    return new ResponseModel<BuildingResponseDTO>()
                    {
                        data = mapper.Map<BuildingResponseDTO>(building),
                        remarks = "Success",
                        success = true
                    };
                }
                else
                {
                    return new ResponseModel<BuildingResponseDTO>()
                    {
                        remarks = "Building not found",
                        success = false
                    };
                }
            }
            catch (Exception ex)
            {
                return new ResponseModel<BuildingResponseDTO>()
                {
                    remarks = $"There was a fatal error: {ex}",
                    success = false
                };
            }
        }

        public async Task<ResponseModel<List<BuildingResponseDTO>>> GetAllBuildings()
        {
            try
            {
                var buildings = await db.tbl_building.ToListAsync();

                if (buildings.Any())
                {
                    return new ResponseModel<List<BuildingResponseDTO>>()
                    {
                        data = mapper.Map<List<BuildingResponseDTO>>(buildings),
                        remarks = "Success",
                        success = true
                    };
                }
                else
                {
                    return new ResponseModel<List<BuildingResponseDTO>>()
                    {
                        remarks = "No Building found",
                        success = false
                    };
                }
            }
            catch (Exception ex)
            {
                return new ResponseModel<List<BuildingResponseDTO>>()
                {
                    remarks = $"There was a fatal error: {ex}",
                    success = false
                };
            }
        }

        public async Task<ResponseModel> DeleteBuildingById(string buildingId)
        {
            try
            {
                var building = await db.tbl_building.FindAsync(Guid.Parse(buildingId));

                if (building != null)
                {
                    db.tbl_building.Remove(building);
                    await db.SaveChangesAsync();

                    return new ResponseModel()
                    {
                        remarks = "Building deleted successfully",
                        success = true
                    };
                }
                else
                {
                    return new ResponseModel()
                    {
                        remarks = "Building not found",
                        success = false
                    };
                }
            }
            catch (Exception ex)
            {
                return new ResponseModel()
                {
                    remarks = $"There was a fatal error: {ex}",
                    success = false
                };
            }
        }
    }

}
