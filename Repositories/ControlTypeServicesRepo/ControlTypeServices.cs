using EMO.Models.DBModels.DBTables;
using EMO.Models.DTOs.BusinessDTOs;
using EMO.Models.DTOs.ControlTypeDTOs;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using EMO.Models.DBModels;
using EMO.Models.DTOs.ResponseDTO;

namespace EMO.Repositories.BusinessServicesRepo
{
    public class ControlTypeServices : IControlTypeServices
    {
        private readonly DBUserManagementContext db;
        private readonly IMapper mapper;

        public ControlTypeServices(DBUserManagementContext db, IMapper mapper)
        {
            this.db = db;
            this.mapper = mapper;
        }

        public async Task<ResponseModel<ControlTypeResponseDTO>> AddControlType(AddControlTypeDTO requestDto)
        {
            try
            {
                var existingControlType = await db.tbl_control_type
                    .Where(b => b.control_type_name.ToLower() == requestDto.controlTypeName.ToLower())
                    .FirstOrDefaultAsync();

                if (existingControlType == null)
                {
                    var newControlType = mapper.Map<tbl_control_type>(requestDto);
                    await db.tbl_control_type.AddAsync(newControlType);
                    await db.SaveChangesAsync();

                    var response = mapper.Map<ControlTypeResponseDTO>(newControlType);

                    return new ResponseModel<ControlTypeResponseDTO>()
                    {
                        data = response,
                        remarks = "Success",
                        success = true,
                    };
                }
                else
                {
                    return new ResponseModel<ControlTypeResponseDTO>()
                    {
                        remarks = "Control Type Already Exists",
                        success = false,
                    };
                }
            }
            catch (Exception ex)
            {
                return new ResponseModel<ControlTypeResponseDTO>()
                {
                    remarks = $"There was a fatal error: {ex}",
                    success = false,
                };
            }
        }

        public async Task<ResponseModel<ControlTypeResponseDTO>> UpdateControlType(UpdateControlTypeDTO requestDto)
        {
            try
            {
                var existingControlType = await db.tbl_control_type
                    .Where(b => b.control_type_id == Guid.Parse(requestDto.controlTypeId))
                    .FirstOrDefaultAsync();

                if (existingControlType != null)
                {
                    mapper.Map(requestDto, existingControlType);

                    await db.SaveChangesAsync();

                    var response = mapper.Map<ControlTypeResponseDTO>(existingControlType);

                    return new ResponseModel<ControlTypeResponseDTO>()
                    {
                        data = response,
                        remarks = "Success",
                        success = true,
                    };
                }
                else
                {
                    return new ResponseModel<ControlTypeResponseDTO>()
                    {
                        remarks = "No Record Found",
                        success = false,
                    };
                }
            }
            catch (Exception ex)
            {
                return new ResponseModel<ControlTypeResponseDTO>()
                {
                    remarks = $"There was a fatal error: {ex}",
                    success = false,
                };
            }
        }

        public async Task<ResponseModel<ControlTypeResponseDTO>> GetControlTypeById(string controltypeId)
        {
            try
            {
                var controltype = await db.tbl_control_type
                    .Where(b => b.control_type_id == Guid.Parse(controltypeId))
                    .FirstOrDefaultAsync();

                if (controltype != null)
                {
                    var response = mapper.Map<ControlTypeResponseDTO>(controltype);

                    return new ResponseModel<ControlTypeResponseDTO>()
                    {
                        data = response,
                        remarks = "Success",
                        success = true
                    };
                }
                else
                {
                    return new ResponseModel<ControlTypeResponseDTO>()
                    {
                        remarks = "Control Type not found",
                        success = false,
                    };
                }
            }
            catch (Exception ex)
            {
                return new ResponseModel<ControlTypeResponseDTO>()
                {
                    remarks = $"There was a fatal error {ex}",
                    success = false,
                };
            }
        }

        public async Task<ResponseModel<List<ControlTypeResponseDTO>>> GetAllControlTypes()
        {
            try
            {
                var controltype = await db.tbl_control_type.ToListAsync();

                if (controltype.Any())
                {
                    var responseList = mapper.Map<List<ControlTypeResponseDTO>>(controltype);

                    return new ResponseModel<List<ControlTypeResponseDTO>>()
                    {
                        data = responseList,
                        remarks = "Success",
                        success = true
                    };
                }
                else
                {
                    return new ResponseModel<List<ControlTypeResponseDTO>>()
                    {
                        remarks = "No Control Type found",
                        success = false,
                    };
                }
            }
            catch (Exception ex)
            {
                return new ResponseModel<List<ControlTypeResponseDTO>>()
                {
                    remarks = $"There was a fatal error {ex}",
                    success = false,
                };
            }
        }

        public async Task<ResponseModel> DeleteControlTypeById(string controltypeId)
        {
            try
            {
                var controltype = await db.tbl_control_type.FindAsync(Guid.Parse(controltypeId));

                if (controltype != null)
                {
                    db.tbl_control_type.Remove(controltype);
                    await db.SaveChangesAsync();

                    return new ResponseModel()
                    {
                        remarks = "Control Type deleted successfully",
                        success = true,
                    };
                }
                else
                {
                    return new ResponseModel()
                    {
                        remarks = "Control Type not found",
                        success = false,
                    };
                }
            }
            catch (Exception ex)
            {
                return new ResponseModel()
                {
                    remarks = $"There was a fatal error {ex}",
                    success = false,
                };
            }
        }
    }

}
