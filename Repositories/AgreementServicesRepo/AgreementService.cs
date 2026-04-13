using AutoMapper;
using EMO.Models.DBModels;
using EMO.Models.DBModels.DBTables;
using EMO.Models.DTOs.AgreementDTOs;
using EMO.Models.DTOs.ResponseDTO;
using Microsoft.EntityFrameworkCore;
using ZXing;

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

        //public async Task<ResponseModel<List<AgreementResponseDTO>>> GetAgreementByBusinessId(string businessId)
        //{
        //    try
        //    {
        //        var businessGuid = Guid.Parse(businessId);

        //        // STEP 1: Get agreements of this business
        //        var agreements = await db.tbl_agreement
        //            .Where(x => x.fk_business == businessGuid)
        //            .ToListAsync();

        //        // STEP 2: Check and update expired agreements
        //        var now = DateTime.Now;

        //        bool isAnyUpdated = false;

        //        foreach (var agreement in agreements)
        //        {
        //            if (agreement.agreement_end_date < now && agreement.is_active)
        //            {
        //                agreement.is_active = false;
        //                agreement.updated_at = now;

        //                var offices = await db.tbl_office_agreement.Where(x => x.fk_agreement == agreement.agreement_id).Include(x => x.office).Select(x => x.office).ToListAsync();
        //                foreach (var item in offices)
        //                {
        //                    item.is_occupied = false;
        //                }
        //                isAnyUpdated = true;
        //            }
        //        }

        //        // STEP 3: Save changes if any update happened
        //        if (isAnyUpdated)
        //        {
        //            await db.SaveChangesAsync();
        //        }

        //        // STEP 4: Fetch again with includes (or reuse if you want optimization)
        //        var result = await db.tbl_agreement
        //            .Include(x => x.tenant)
        //            .Where(x => x.fk_business == businessGuid)
        //            .ToListAsync();

        //        if (result.Any())
        //        {
        //            return new ResponseModel<List<AgreementResponseDTO>>()
        //            {
        //                data = mapper.Map<List<AgreementResponseDTO>>(result),
        //                remarks = "Success",
        //                success = true
        //            };
        //        }
        //        else
        //        {
        //            return new ResponseModel<List<AgreementResponseDTO>>()
        //            {
        //                remarks = "No record found",
        //                success = false
        //            };
        //        }
        //    }
        //    catch (Exception)
        //    {
        //        return new ResponseModel<List<AgreementResponseDTO>>()
        //        {
        //            remarks = $"There was a fatal error",
        //            success = false
        //        };
        //    }
        //}


        public async Task<ResponseModel<List<AgreementResponseDTO>>> GetAgreementByBusinessId(string businessId)
        {
            try
            {
                var businessGuid = Guid.Parse(businessId);
                var now = DateTime.Now;

                var agreements = await db.tbl_agreement
                    .Where(x => x.fk_business == businessGuid)
                    .Include(x => x.tenant)
                    .ToListAsync();

                if (agreements.Any())
                {
                    return new ResponseModel<List<AgreementResponseDTO>>()
                    {
                        remarks = "No record found",
                        success = false
                    };
                }
                var expiredAgreements = agreements
                    .Where(a => a.agreement_end_date < now && a.is_active)
                    .ToList();

                if (expiredAgreements.Any())
                {
                    var expiredIds = expiredAgreements.Select(x => x.agreement_id).ToList();

                    foreach (var agreement in expiredAgreements)
                    {
                        agreement.is_active = false;
                        agreement.updated_at = now;
                    }

                    var offices = await db.tbl_office_agreement
                        .Where(x => expiredIds.Contains(x.fk_agreement))
                        .Include(x => x.office)
                        .Select(x => x.office)
                        .Distinct()
                        .ToListAsync();

                    foreach (var office in offices)
                    {
                        office.is_occupied = false;
                    }

                    await db.SaveChangesAsync();
                }

                var dto = mapper.Map<List<AgreementResponseDTO>>(agreements);

                return new ResponseModel<List<AgreementResponseDTO>>
                {
                    data = dto,
                    success = true,
                    remarks = "Success"
                };
            }
            catch (Exception ex)
            {
                return new ResponseModel<List<AgreementResponseDTO>>
                {
                    success = false,
                    remarks = ex.Message
                };
            }
        }
        public async Task<ResponseModel<List<AgreementResponseDTO>>> GetAllAgreements()
        {
            try
            {
                var agreements = await db.tbl_agreement
                    .Include(x => x.tenant)
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
