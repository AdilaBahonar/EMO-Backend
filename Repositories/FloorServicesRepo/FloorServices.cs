using EMO.Models.DBModels.DBTables;
using EMO.Models.DTOs.FloorDTOs;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using EMO.Models.DBModels;
using EMO.Models.DTOs.ResponseDTO;

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

        public async Task<ResponseModel<FloorResponseDTO>> AddFloor(AddFloorDTO requestDto)
        {
            try
            {
                var existingFloor = await db.tbl_floor
                    .Where(f => f.floor_name.ToLower() == requestDto.floorName.ToLower())
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
                    remarks = $"There was a fatal error: {ex}",
                    success = false
                };
            }
        }

        public async Task<ResponseModel<FloorResponseDTO>> UpdateFloor(UpdateFloorDTO requestDto)
        {
            try
            {
                var existingFloor = await db.tbl_floor
                    .Where(f => f.floor_id == Guid.Parse(requestDto.floorId))
                    .FirstOrDefaultAsync();

                if (existingFloor != null)
                {
                    mapper.Map(requestDto, existingFloor);
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
                    remarks = $"There was a fatal error: {ex}",
                    success = false
                };
            }
        }

        public async Task<ResponseModel<FloorResponseDTO>> GetFloorById(string floorId)
        {
            try
            {
                var floor = await db.tbl_floor
                    .Where(f => f.floor_id == Guid.Parse(floorId))
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
                    remarks = $"There was a fatal error: {ex}",
                    success = false
                };
            }
        }

        public async Task<ResponseModel<List<FloorResponseDTO>>> GetAllFloors()
        {
            try
            {
                var floors = await db.tbl_floor.ToListAsync();

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
                    remarks = $"There was a fatal error: {ex}",
                    success = false
                };
            }
        }

        public async Task<ResponseModel> DeleteFloorById(string floorId)
        {
            try
            {
                var floor = await db.tbl_floor.FindAsync(Guid.Parse(floorId));

                if (floor != null)
                {
                    db.tbl_floor.Remove(floor);
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
                    remarks = $"There was a fatal error: {ex}",
                    success = false
                };
            }
        }
    }

}
