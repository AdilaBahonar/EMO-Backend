using AutoMapper;
using Microsoft.EntityFrameworkCore;
using EMO.Models.DBModels;
using EMO.Models.DTOs.ResponseDTO;
using EMO.Models.DBModels.DBTables;
using EMO.Models.DTOs.GenderDTOs;

namespace EMO.Repositories.GenderServicesRepo
{
    public class GenderServices : IGenderServices
    {
        private readonly DBUserManagementContext db;
        private readonly IMapper mapper;

        public GenderServices(DBUserManagementContext context, IMapper mapper)
        {
            this.db = context;
            this.mapper = mapper;
        }
        public async Task<ResponseModel<GenderResponseDTO>> AddGender(AddGenderDTO requestDto)
        {
            try
            {
                // Check for duplicate (case-insensitive)
                bool exists = await db.tbl_gender
                    .AnyAsync(g => g.gender_name.ToLower() == requestDto.genderName.ToLower());

                if (exists)
                    return new ResponseModel<GenderResponseDTO>
                    {
                        remarks = "Gender already exists",
                        success = false
                    };

                var newGender = mapper.Map<tbl_gender>(requestDto);
                await db.tbl_gender.AddAsync(newGender);
                await db.SaveChangesAsync();

                return new ResponseModel<GenderResponseDTO>
                {
                    remarks = "Success",
                    success = true
                };
            }
            catch (Exception ex)
            {
                return new ResponseModel<GenderResponseDTO>
                {
                    remarks = $"Fatal error: Internal Server Error",
                    success = false
                };
            }
        }
        public async Task<ResponseModel<GenderResponseDTO>> UpdateGender(UpdateGenderDTO requestDto)
        {
            try
            {
                var existingGender = await db.tbl_gender
                    .FirstOrDefaultAsync(g => g.gender_id == Guid.Parse(requestDto.genderId));

                if (existingGender == null)
                    return new ResponseModel<GenderResponseDTO>
                    {
                        remarks = "No record found",
                        success = false
                    };

                existingGender.gender_name = requestDto.genderName;
                await db.SaveChangesAsync();

                return new ResponseModel<GenderResponseDTO>
                {
                    remarks = "Updated successfully",
                    success = true
                };
            }
            catch (Exception ex)
            {
                return new ResponseModel<GenderResponseDTO>
                {
                    remarks = $"Fatal error: Internal Server Error",
                    success = false
                };
            }
        }
        public async Task<ResponseModel<List<GenderResponseDTO>>> GetAllGenders()
        {
            try
            {
                var all = await db.tbl_gender
                    .OrderBy(g => g.gender_name)
                    .ToListAsync();

                if (!all.Any())
                    return new ResponseModel<List<GenderResponseDTO>>
                    {
                        remarks = "No genders found",
                        success = false
                    };

                return new ResponseModel<List<GenderResponseDTO>>
                {
                    data = mapper.Map<List<GenderResponseDTO>>(all),
                    remarks = "Success",
                    success = true
                };
            }
            catch (Exception ex)
            {
                return new ResponseModel<List<GenderResponseDTO>>
                {
                    remarks = $"Fatal error: Internal Server Error",
                    success = false
                };
            }
        }
        public async Task<ResponseModel<GenderResponseDTO>> GetGenderById(string id)
        {
            try
            {
                var gender = await db.tbl_gender
                    .FirstOrDefaultAsync(g => g.gender_id == Guid.Parse(id));

                if (gender == null)
                    return new ResponseModel<GenderResponseDTO>
                    {
                        remarks = "No record found",
                        success = false
                    };

                return new ResponseModel<GenderResponseDTO>
                {
                    data = mapper.Map<GenderResponseDTO>(gender),
                    remarks = "Success",
                    success = true
                };
            }
            catch (Exception ex)
            {
                return new ResponseModel<GenderResponseDTO>
                {
                    remarks = $"Fatal error: Internal Server Error",
                    success = false
                };
            }
        }
        public async Task<ResponseModel<string>> DeleteGender(string id)
        {
            try
            {
                var gender = await db.tbl_gender
                    .FirstOrDefaultAsync(g => g.gender_id == Guid.Parse(id));

                if (gender == null)
                    return new ResponseModel<string>
                    {
                        remarks = "No record found",
                        success = false
                    };

                db.tbl_gender.Remove(gender);
                await db.SaveChangesAsync();

                return new ResponseModel<string>
                {
                    data = id,
                    remarks = "Deleted successfully",
                    success = true
                };
            }
            catch (Exception ex)
            {
                return new ResponseModel<string>
                {
                    remarks = $"Fatal error: Internal server Error",
                    success = false
                };
            }
        }
    }
}
