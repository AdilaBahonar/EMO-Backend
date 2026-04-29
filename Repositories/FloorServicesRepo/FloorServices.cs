using AutoMapper;
using EMO.Models.DBModels;
using EMO.Models.DBModels.DBTables;
using EMO.Models.DTOs.BuildingDTOs;
using EMO.Models.DTOs.FloorDTOs;
using EMO.Models.DTOs.ResponseDTO;
using Microsoft.EntityFrameworkCore;

namespace EMO.Repositories.FloorServicesRepo
{
    public class FloorServices : IFloorServices
    {
        private readonly DBUserManagementContext db;
        private readonly IMapper mapper;

        public FloorServices(DBUserManagementContext db, IMapper mapper)
        {
            this.db = db;
            this.mapper = mapper;
        }

        public async Task<ResponseModel<List<FloorResponseDTO>>> GetFloorByBusinessId(string businessId)
        {
            try
            {

                if (string.IsNullOrEmpty(businessId))
                {
                    return new ResponseModel<List<FloorResponseDTO>>()
                    {
                        remarks = "Invalid Id.",
                        success = false
                    };
                }
                var floor = await db.tbl_floor
                    .Include(x => x.business).Include(x=>x.building)
                    .Where(x => x.fk_business == Guid.Parse(businessId) && !x.is_deleted)
                    .ToListAsync();

                if (floor.Any())
                {
                    return new ResponseModel<List<FloorResponseDTO>>()
                    {
                        data = mapper.Map<List<FloorResponseDTO>>(floor),
                        remarks = "Success",
                        success = true
                    };
                }
                else
                {
                    return new ResponseModel<List<FloorResponseDTO>>()
                    {
                        remarks = "No record found.",
                        success = false
                    };
                }
            }
            catch (Exception ex)
            {
                return new ResponseModel<List<FloorResponseDTO>>()
                {
                    remarks = $"There was a fatal error",
                    success = false
                };
            }
        }
        public async Task<ResponseModel<FloorResponseDTO>> AddFloor(AddFloorDTO requestDto)
        {
            try
            {
                var existingFloor = await db.tbl_floor
                    .Where(x => x.floor_name.ToLower() == requestDto.floorName.ToLower()
                             && x.fk_building == Guid.Parse(requestDto.fkBuilding ) && !x.is_deleted)
                    .FirstOrDefaultAsync();

                if (existingFloor == null)
                {
                    var newFloor = mapper.Map<tbl_floor>(requestDto);
                    await db.tbl_floor.AddAsync(newFloor);
                    await db.SaveChangesAsync();

                    return new ResponseModel<FloorResponseDTO>()
                    {
                        data = mapper.Map<FloorResponseDTO>(newFloor),
                        remarks = "Success",
                        success = true
                    };
                }
                else
                {
                    return new ResponseModel<FloorResponseDTO>()
                    {
                        remarks = "Floor Already Exists",
                        success = false
                    };
                }
            }
            catch (Exception ex)
            {
                return new ResponseModel<FloorResponseDTO>()
                {
                    remarks = $"There was a fatal error",
                    success = false
                };
            }
        }

        public async Task<ResponseModel<FloorResponseDTO>> UpdateFloor(UpdateFloorDTO requestDto)
        {
            try
            {
                var existingFloor = await db.tbl_floor
                    .Where(x => x.floor_id == Guid.Parse(requestDto.floorId) && !x.is_deleted)
                    .FirstOrDefaultAsync();

                if (existingFloor != null)
                {
                    mapper.Map(requestDto, existingFloor);
                    existingFloor.updated_at = DateTime.Now;
                    await db.SaveChangesAsync();

                    return new ResponseModel<FloorResponseDTO>()
                    {
                        data = mapper.Map<FloorResponseDTO>(existingFloor),
                        remarks = "Success",
                        success = true
                    };
                }
                else
                {
                    return new ResponseModel<FloorResponseDTO>()
                    {
                        remarks = "No Record Found",
                        success = false
                    };
                }
            }
            catch (Exception ex)
            {
                return new ResponseModel<FloorResponseDTO>()
                {
                    remarks = $"There was a fatal error",
                    success = false
                };
            }
        }

        public async Task<ResponseModel<FloorResponseDTO>> GetFloorById(string floorId)
        {
            try
            {
                var floor = await db.tbl_floor
                    .Include(x => x.building)
                    .Where(x => x.floor_id == Guid.Parse(floorId) && !x.is_deleted)
                    .FirstOrDefaultAsync();

                if (floor != null)
                {
                    return new ResponseModel<FloorResponseDTO>()
                    {
                        data = mapper.Map<FloorResponseDTO>(floor),
                        remarks = "Success",
                        success = true
                    };
                }
                else
                {
                    return new ResponseModel<FloorResponseDTO>()
                    {
                        remarks = "Floor not found",
                        success = false
                    };
                }
            }
            catch (Exception ex)
            {
                return new ResponseModel<FloorResponseDTO>()
                {
                    remarks = $"There was a fatal error",
                    success = false
                };
            }
        }

        public async Task<ResponseModel<List<FloorResponseDTO>>> GetAllFloors()
        {
            try
            {
                var floors = await db.tbl_floor
                    .Include(x => x.building)
                    .Where(x => !x.is_deleted)
                    .ToListAsync();

                if (floors.Any())
                {
                    return new ResponseModel<List<FloorResponseDTO>>()
                    {
                        data = mapper.Map<List<FloorResponseDTO>>(floors),
                        remarks = "Success",
                        success = true
                    };
                }
                else
                {
                    return new ResponseModel<List<FloorResponseDTO>>()
                    {
                        remarks = "No Floor found",
                        success = false
                    };
                }
            }
            catch (Exception ex)
            {
                return new ResponseModel<List<FloorResponseDTO>>()
                {
                    remarks = $"There was a fatal error",
                    success = false
                };
            }
        }


        public async Task<ResponseModel<List<FloorResponseDTO>>> GetFloorByBuildingId(string buildingId)
        {
            try
            {
                var Building = await db.tbl_floor
                    .Include(x => x.building)
                    .Where(x => x.fk_building == Guid.Parse(buildingId) && !x.is_deleted)
                    .ToListAsync();

                if (Building.Any())
                {
                    return new ResponseModel<List<FloorResponseDTO>>()
                    {
                        data = mapper.Map<List<FloorResponseDTO>>(Building),
                        remarks = "Success",
                        success = true
                    };
                }
                else
                {
                    return new ResponseModel<List<FloorResponseDTO>>()
                    {
                        remarks = "Floor not found",
                        success = false
                    };
                }
            }
            catch (Exception ex)
            {
                return new ResponseModel<List<FloorResponseDTO>>()
                {
                    remarks = $"There was a fatal error",
                    success = false
                };
            }
        }
        public async Task<ResponseModel> DeleteFloorById(string floorId)
        {
            try
            {
                var floor = await db.tbl_floor.FirstOrDefaultAsync(x => x.floor_id == Guid.Parse(floorId) && !x.is_deleted);

                if (floor != null)
                {
                    floor.is_deleted = true;
                    await db.SaveChangesAsync();

                    return new ResponseModel()
                    {
                        remarks = "Floor deleted successfully",
                        success = true
                    };
                }
                else
                {
                    return new ResponseModel()
                    {
                        remarks = "Floor not found",
                        success = false
                    };
                }
            }
            catch (Exception ex)
            {
                return new ResponseModel()
                {
                    remarks = $"There was a fatal error",
                    success = false
                };
            }
        }
    }
}
