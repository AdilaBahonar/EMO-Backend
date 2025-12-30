using EMO.Models.DBModels.DBTables;
using EMO.Models.DTOs.SectionDTOs;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using 
    .Models.DBModels;
using EMO.Models.DTOs.ResponseDTO;
using System.ComponentModel.Design;

namespace EMO.Repositories.SectionServicesRepo
{
    public class SectionServices : ISectionServices
    {
        private readonly DBUserManagementContext db;
        private readonly IMapper mapper;

        public SectionServices(DBUserManagementContext db, IMapper mapper)
        {
            this.db = db;
            this.mapper = mapper;
        }

        public async Task<ResponseModel<SectionResponseDTO>> AddSection(AddSectionDTO requestDto)
        {
            try
            {
                var existingSection = await db.tbl_section
                    .Where(s => s.section_name.ToLower() == requestDto.sectionName.ToLower())
                    .FirstOrDefaultAsync();

                if (existingSection == null)
                {
                    var newSection = mapper.Map<tbl_section>(requestDto);
                    await db.tbl_section.AddAsync(newSection);
                    await db.SaveChangesAsync();

                    return new ResponseModel<SectionResponseDTO>()
                    {
                        data = mapper.Map<SectionResponseDTO>(newSection),
                        remarks = "Success",
                        success = true
                    };
                }
                else
                {
                    return new ResponseModel<SectionResponseDTO>()
                    {
                        remarks = "Section Already Exists",
                        success = false
                    };
                }
            }
            catch (Exception ex)
            {
                return new ResponseModel<SectionResponseDTO>()
                {
                    remarks = $"There was a fatal error: {ex}",
                    success = false
                };
            }
        }

        public async Task<ResponseModel<SectionResponseDTO>> UpdateSection(UpdateSectionDTO requestDto)
        {
            try
            {
                var existingSection = await db.tbl_section
                    .Where(s => s.section_id == Guid.Parse(requestDto.sectionId))
                    .FirstOrDefaultAsync();

                if (existingSection != null)
                {
                    mapper.Map(requestDto, existingSection);
                    await db.SaveChangesAsync();

                    return new ResponseModel<SectionResponseDTO>()
                    {
                        data = mapper.Map<SectionResponseDTO>(existingSection),
                        remarks = "Success",
                        success = true
                    };
                }
                else
                {
                    return new ResponseModel<SectionResponseDTO>()
                    {
                        remarks = "No Record Found",
                        success = false
                    };
                }
            }
            catch (Exception ex)
            {
                return new ResponseModel<SectionResponseDTO>()
                {
                    remarks = $"There was a fatal error: {ex}",
                    success = false
                };
            }
        }

        public async Task<ResponseModel<SectionResponseDTO>> GetSectionById(string sectionId)
        {
            try
            {
                var section = await db.tbl_section
                    .Where(s => s.section_id == Guid.Parse(sectionId))
                    .FirstOrDefaultAsync();

                if (section != null)
                {
                    return new ResponseModel<SectionResponseDTO>()
                    {
                        data = mapper.Map<SectionResponseDTO>(section),
                        remarks = "Success",
                        success = true
                    };
                }
                else
                {
                    return new ResponseModel<SectionResponseDTO>()
                    {
                        remarks = "Section not found",
                        success = false
                    };
                }
            }
            catch (Exception ex)
            {
                return new ResponseModel<SectionResponseDTO>()
                {
                    remarks = $"There was a fatal error: {ex}",
                    success = false
                };
            }
        }

        public async Task<ResponseModel<List<SectionResponseDTO>>> GetAllSections()
        {
            try
            {
                var sections = await db.tbl_section.ToListAsync();

                if (sections.Any())
                {
                    return new ResponseModel<List<SectionResponseDTO>>()
                    {
                        data = mapper.Map<List<SectionResponseDTO>>(sections),
                        remarks = "Success",
                        success = true
                    };
                }
                else
                {
                    return new ResponseModel<List<SectionResponseDTO>>()
                    {
                        remarks = "No Section found",
                        success = false
                    };
                }
            }
            catch (Exception ex)
            {
                return new ResponseModel<List<SectionResponseDTO>>()
                {
                    remarks = $"There was a fatal error: {ex}",
                    success = false
                };
            }
        }

        public async Task<ResponseModel> DeleteSectionById(string sectionId)
        {
            try
            {
                var section = await db.tbl_section.FindAsync(Guid.Parse(sectionId));

                if (section != null)
                {
                    db.tbl_section.Remove(section);
                    await db.SaveChangesAsync();

                    return new ResponseModel()
                    {
                        remarks = "Section deleted successfully",
                        success = true
                    };
                }
                else
                {
                    return new ResponseModel()
                    {
                        remarks = "Section not found",
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
