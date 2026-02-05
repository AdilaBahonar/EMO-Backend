using EMO.Models.DBModels.DBTables;
using EMO.Models.DTOs.SingalPhaseDTOs;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using EMO.Models.DBModels;
using EMO.Repositories.SingalPhaseDataServicesRepo;
using EMO.Models.DTOs.ResponseDTO;

namespace EMO.Repositories.SingalPhaseDataRepo
{
    public class SingalPhaseDataService : ISingalPhaseDataService
    {
        private readonly DBUserManagementContext db;
        private readonly IMapper mapper;

        public SingalPhaseDataService(DBUserManagementContext db, IMapper mapper)
        {
            this.db = db;
            this.mapper = mapper;
        }

        // Add New Singal Phase Data
        public async Task<ResponseModel<SingalPhaseDataResponseDTO>> AddSingalPhaseData(AddSingalPhaseDataDTO requestDto)
        {
            try
            {
                var newData = mapper.Map<tbl_singal_phase_data>(requestDto);
                await db.tbl_singal_phase_data.AddAsync(newData);
                await db.SaveChangesAsync();

                return new ResponseModel<SingalPhaseDataResponseDTO>()
                {
                    data = mapper.Map<SingalPhaseDataResponseDTO>(newData),
                    remarks = "Success",
                    success = true
                };
            }
            catch (Exception ex)
            {
                return new ResponseModel<SingalPhaseDataResponseDTO>()
                {
                    remarks = $"Error: {ex.Message}",
                    success = false
                };
            }
        }

        // Update Existing Singal Phase Data
        public async Task<ResponseModel<SingalPhaseDataResponseDTO>> UpdateSingalPhaseData(UpdateSingalPhaseDataDTO requestDto)
        {
            try
            {
                var existingData = await db.tbl_singal_phase_data
                    .Where(x => x.singal_phase_data_id == Guid.Parse(requestDto.singalPhaseDataId))
                    .FirstOrDefaultAsync();

                if (existingData == null)
                {
                    return new ResponseModel<SingalPhaseDataResponseDTO>()
                    {
                        remarks = "No record found",
                        success = false
                    };
                }

                mapper.Map(requestDto, existingData);
                await db.SaveChangesAsync();

                return new ResponseModel<SingalPhaseDataResponseDTO>()
                {
                    data = mapper.Map<SingalPhaseDataResponseDTO>(existingData),
                    remarks = "Updated successfully",
                    success = true
                };
            }
            catch (Exception ex)
            {
                return new ResponseModel<SingalPhaseDataResponseDTO>()
                {
                    remarks = $"Error: {ex.Message}",
                    success = false
                };
            }
        }

        // Get Single Record By ID
        public async Task<ResponseModel<SingalPhaseDataResponseDTO>> GetSingalPhaseDataById(Guid singalPhaseDataId)
        {
            try
            {
                var data = await db.tbl_singal_phase_data
                    .FirstOrDefaultAsync(x => x.singal_phase_data_id == singalPhaseDataId);

                if (data == null)
                {
                    return new ResponseModel<SingalPhaseDataResponseDTO>()
                    {
                        remarks = "Record not found",
                        success = false
                    };
                }

                return new ResponseModel<SingalPhaseDataResponseDTO>()
                {
                    data = mapper.Map<SingalPhaseDataResponseDTO>(data),
                    remarks = "Success",
                    success = true
                };
            }
            catch (Exception ex)
            {
                return new ResponseModel<SingalPhaseDataResponseDTO>()
                {
                    remarks = $"Error: {ex.Message}",
                    success = false
                };
            }
        }

        // Get All Records
        public async Task<ResponseModel<List<SingalPhaseDataResponseDTO>>> GetAllSingalPhaseData()
        {
            try
            {
                var list = await db.tbl_singal_phase_data.ToListAsync();

                if (!list.Any())
                {
                    return new ResponseModel<List<SingalPhaseDataResponseDTO>>()
                    {
                        remarks = "No records found",
                        success = false
                    };
                }

                return new ResponseModel<List<SingalPhaseDataResponseDTO>>()
                {
                    data = mapper.Map<List<SingalPhaseDataResponseDTO>>(list),
                    remarks = "Success",
                    success = true
                };
            }
            catch (Exception ex)
            {
                return new ResponseModel<List<SingalPhaseDataResponseDTO>>()
                {
                    remarks = $"Error: {ex.Message}",
                    success = false
                };
            }
        }

        // Delete Record by ID
        public async Task<ResponseModel> DeleteSingalPhaseDataById(Guid singalPhaseDataId)
        {
            try
            {
                var data = await db.tbl_singal_phase_data.FindAsync(singalPhaseDataId);
                if (data == null)
                {
                    return new ResponseModel()
                    {
                        remarks = "Record not found",
                        success = false
                    };
                }

                db.tbl_singal_phase_data.Remove(data);
                await db.SaveChangesAsync();

                return new ResponseModel()
                {
                    remarks = "Deleted successfully",
                    success = true
                };
            }
            catch (Exception ex)
            {
                return new ResponseModel()
                {
                    remarks = $"Error: {ex.Message}",
                    success = false
                };
            }
        }
    }
}
