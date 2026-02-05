using AutoMapper;
using EMO.Models.DBModels;
using EMO.Models.DBModels.DBTables;
using EMO.Models.DTOs.AgreementDTOs;
using EMO.Models.DTOs.ResponseDTO;
using Microsoft.EntityFrameworkCore;

namespace EMO.Repositories.AgreementServicesRepo
{
    public class AgreementServices : IAgreementServices
    {
        private readonly DBUserManagementContext db;
        private readonly IMapper mapper;

        public AgreementServices(DBUserManagementContext db, IMapper mapper)
        {
            this.db = db;
            this.mapper = mapper;
        }

        public async Task<ResponseModel<AgreementResponseDTO>> AddAgreement(AddAgreementDTO requestDto)
        {
            try
            {
                var existingAgreement = await db.tbl_agreement
                    .Where(x => x.agreement_name.ToLower() == requestDto.agreementName.ToLower())
                    .FirstOrDefaultAsync();

                if (existingAgreement == null)
                {
                    var newAgreement = mapper.Map<tbl_agreement>(requestDto);
                    await db.tbl_agreement.AddAsync(newAgreement);
                    await db.SaveChangesAsync();

                    return new ResponseModel<AgreementResponseDTO>()
                    {
                        data = mapper.Map<AgreementResponseDTO>(newAgreement),
                        remarks = "Success",
                        success = true
                    };
                }
                else
                {
                    return new ResponseModel<AgreementResponseDTO>()
                    {
                        remarks = "Agreement Already Exists",
                        success = false
                    };
                }
            }
            catch (Exception ex)
            {
                return new ResponseModel<AgreementResponseDTO>()
                {
                    remarks = $"There was a fatal error: {ex}",
                    success = false
                };
            }
        }

        public async Task<ResponseModel<AgreementResponseDTO>> UpdateAgreement(UpdateAgreementDTO requestDto)
        {
            try
            {
                var existingAgreement = await db.tbl_agreement
                    .Where(x => x.agreement_id == Guid.Parse(requestDto.agreementId))
                    .FirstOrDefaultAsync();

                if (existingAgreement != null)
                {
                    mapper.Map(requestDto, existingAgreement);
                    await db.SaveChangesAsync();

                    return new ResponseModel<AgreementResponseDTO>()
                    {
                        data = mapper.Map<AgreementResponseDTO>(existingAgreement),
                        remarks = "Success",
                        success = true
                    };
                }
                else
                {
                    return new ResponseModel<AgreementResponseDTO>()
                    {
                        remarks = "No Record Found",
                        success = false
                    };
                }
            }
            catch (Exception ex)
            {
                return new ResponseModel<AgreementResponseDTO>()
                {
                    remarks = $"There was a fatal error: {ex}",
                    success = false
                };
            }
        }

        public async Task<ResponseModel<AgreementResponseDTO>> GetAgreementById(string agreementId)
        {
            try
            {
                var agreement = await db.tbl_agreement
                    .Include(x => x.tenant)
                    .Include(x => x.office)
                    .Where(x => x.agreement_id == Guid.Parse(agreementId))
                    .FirstOrDefaultAsync();

                if (agreement != null)
                {
                    return new ResponseModel<AgreementResponseDTO>()
                    {
                        data = mapper.Map<AgreementResponseDTO>(agreement),
                        remarks = "Success",
                        success = true
                    };
                }
                else
                {
                    return new ResponseModel<AgreementResponseDTO>()
                    {
                        remarks = "Agreement not found",
                        success = false
                    };
                }
            }
            catch (Exception ex)
            {
                return new ResponseModel<AgreementResponseDTO>()
                {
                    remarks = $"There was a fatal error: {ex}",
                    success = false
                };
            }
        }

        public async Task<ResponseModel<List<AgreementResponseDTO>>> GetAllAgreements()
        {
            try
            {
                var agreements = await db.tbl_agreement
                    .Include(x => x.tenant)
                    .Include(x => x.office)
                    .ToListAsync();

                if (agreements.Any())
                {
                    return new ResponseModel<List<AgreementResponseDTO>>()
                    {
                        data = mapper.Map<List<AgreementResponseDTO>>(agreements),
                        remarks = "Success",
                        success = true
                    };
                }
                else
                {
                    return new ResponseModel<List<AgreementResponseDTO>>()
                    {
                        remarks = "No Agreements found",
                        success = false
                    };
                }
            }
            catch (Exception ex)
            {
                return new ResponseModel<List<AgreementResponseDTO>>()
                {
                    remarks = $"There was a fatal error: {ex}",
                    success = false
                };
            }
        }

        public async Task<ResponseModel> DeleteAgreementById(string agreementId)
        {
            try
            {
                var agreement = await db.tbl_agreement.FindAsync(Guid.Parse(agreementId));

                if (agreement != null)
                {
                    db.tbl_agreement.Remove(agreement);
                    await db.SaveChangesAsync();

                    return new ResponseModel()
                    {
                        remarks = "Agreement deleted successfully",
                        success = true
                    };
                }
                else
                {
                    return new ResponseModel()
                    {
                        remarks = "Agreement not found",
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
