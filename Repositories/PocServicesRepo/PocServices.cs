using EMO.Models.DBModels.DBTables;
using EMO.Models.DTOs.OfficeDTOs;
using EMO.Models.DTOs.PocDTOs;
using EMO.Repositories.PocServicesRepo;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using EMO.Models.DBModels;
using EMO.Models.DTOs.ResponseDTO;

namespace EMO.Repositories.OfficeServicesRepo
{
    public class PocServices : IPocServices
    {
        private readonly DBUserManagementContext db;
        private readonly IMapper mapper;

        public PocServices(DBUserManagementContext db, IMapper mapper)
        {
            this.db = db;
            this.mapper = mapper;
        }

        public async Task<ResponseModel<PocResponseDTO>> AddPoc(AddPocDTO requestDto)
        {
            try
            {
                var existingPoc = await db.tbl_poc
                    .Where(o => o.poc_name.ToLower() == requestDto.pocName.ToLower())
                    .FirstOrDefaultAsync();

                if (existingPoc == null)
                {
                    var newPoc = mapper.Map<tbl_poc>(requestDto);
                    await db.tbl_poc.AddAsync(newPoc);
                    await db.SaveChangesAsync();

                    return new ResponseModel<PocResponseDTO>()
                    {
                        data = mapper.Map<PocResponseDTO>(newPoc),
                        remarks = "Success",
                        success = true
                    };
                }
                else
                {
                    return new ResponseModel<PocResponseDTO>()
                    {
                        remarks = "Poc Already Exists",
                        success = false
                    };
                }
            }
            catch (Exception ex)
            {
                return new ResponseModel<PocResponseDTO>()
                {
                    remarks = $"There was a fatal error: {ex}",
                    success = false
                };
            }
        }

        public async Task<ResponseModel<PocResponseDTO>> UpdatePoc(UpdatePocDTO requestDto)
        {
            try
            {
                var existingPoc = await db.tbl_poc
                    .Where(o => o.poc_id == Guid.Parse(requestDto.pocId))
                    .FirstOrDefaultAsync();

                if (existingPoc != null)
                {
                    mapper.Map(requestDto, existingPoc);
                    await db.SaveChangesAsync();

                    return new ResponseModel<PocResponseDTO>()
                    {
                        data = mapper.Map<PocResponseDTO>(existingPoc),
                        remarks = "Success",
                        success = true
                    };
                }
                else
                {
                    return new ResponseModel<PocResponseDTO>()
                    {
                        remarks = "No Record Found",
                        success = false
                    };
                }
            }
            catch (Exception ex)
            {
                return new ResponseModel<PocResponseDTO>()
                {
                    remarks = $"There was a fatal error: {ex}",
                    success = false
                };
            }
        }

        public async Task<ResponseModel<PocResponseDTO>> GetPocById(string pocId)
        {
            try
            {
                var poc = await db.tbl_poc
                    .Where(o => o.poc_id == Guid.Parse(pocId))
                    .FirstOrDefaultAsync();

                if (poc != null)
                {
                    return new ResponseModel<PocResponseDTO>()
                    {
                        data = mapper.Map<PocResponseDTO>(poc),
                        remarks = "Success",
                        success = true
                    };
                }
                else
                {
                    return new ResponseModel<PocResponseDTO>()
                    {
                        remarks = "Poc not found",
                        success = false
                    };
                }
            }
            catch (Exception ex)
            {
                return new ResponseModel<PocResponseDTO>()
                {
                    remarks = $"There was a fatal error: {ex}",
                    success = false
                };
            }
        }

        public async Task<ResponseModel<List<PocResponseDTO>>> GetAllPoces()
        {
            try
            {
                var poces = await db.tbl_office.ToListAsync();

                if (poces.Any())
                {
                    return new ResponseModel<List<PocResponseDTO>>()
                    {
                        data = mapper.Map<List<PocResponseDTO>>(poces),
                        remarks = "Success",
                        success = true
                    };
                }
                else
                {
                    return new ResponseModel<List<PocResponseDTO>>()
                    {
                        remarks = "No poc found",
                        success = false
                    };
                }
            }
            catch (Exception ex)
            {
                return new ResponseModel<List<PocResponseDTO>>()
                {
                    remarks = $"There was a fatal error: {ex}",
                    success = false
                };
            }
        }

        public async Task<ResponseModel> DeletePocById(string pocId)
        {
            try
            {
                var poc = await db.tbl_poc.FindAsync(Guid.Parse(pocId));

                if (poc != null)
                {
                    db.tbl_poc.Remove(poc);
                    await db.SaveChangesAsync();

                    return new ResponseModel()
                    {
                        remarks = "Poc deleted successfully",
                        success = true
                    };
                }
                else
                {
                    return new ResponseModel()
                    {
                        remarks = "Poc not found",
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
