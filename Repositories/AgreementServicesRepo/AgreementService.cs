using AutoMapper;
using EMO.Models.DBModels;
using EMO.Models.DBModels.DBTables;
using EMO.Models.DTOs.AgreementDTOs;
using EMO.Models.DTOs.OfficeDTOs;
using EMO.Models.DTOs.ResponseDTO;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
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
                    .Where(x => x.agreement_name.ToLower() == requestDto.agreementName.ToLower() && !x.is_deleted)
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
                    remarks = $"There was a fatal error",
                    success = false
                };
            }
        }

        public async Task<ResponseModel<AgreementResponseDTO>> UpdateAgreement(UpdateAgreementDTO requestDto)
        {
            try
            {
                var existingAgreement = await db.tbl_agreement
                    .Where(x => x.agreement_id == Guid.Parse(requestDto.agreementId) && !x.is_deleted)
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
                    remarks = $"There was a fatal error",
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
                    .Where(x => x.agreement_id == Guid.Parse(agreementId) && !x.is_deleted)
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
                    remarks = $"There was a fatal error",
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
                var agreements = await db.tbl_agreement
                    .AsNoTracking()
                    .Where(x => x.fk_business == businessGuid && !x.is_deleted)
                    .Include(x => x.tenant)
                    .OrderByDescending(x => x.agreement_start_date)
                    .ToListAsync();

                return new ResponseModel<List<AgreementResponseDTO>>
                {
                    data = mapper.Map<List<AgreementResponseDTO>>(agreements),
                    success = true,
                    remarks = agreements.Any() ? "Success" : "No record found"
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
                    .Where(x => !x.is_deleted)
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
                    remarks = $"There was a fatal error",
                    success = false
                };
            }
        }

        public async Task<ResponseModel<List<OfficeResponseDTO>>> GetOfficeByAgreementId(string agreementId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(agreementId))
                {
                    return new ResponseModel<List<OfficeResponseDTO>>()
                    {
                        remarks = "Invalid Id.",
                        success = false
                    };
                }

                if (!Guid.TryParse(agreementId, out Guid parsedAgreementId))
                {
                    return new ResponseModel<List<OfficeResponseDTO>>()
                    {
                        remarks = "Invalid Guid format.",
                        success = false
                    };
                }

                var offices = await db.tbl_office_agreement
                  .Where(x => x.fk_agreement == parsedAgreementId && !x.is_deleted)
                  .Include(x => x.office)
                      .ThenInclude(o => o.section)
                  .Select(x => x.office)
                  .ToListAsync();

                return new ResponseModel<List<OfficeResponseDTO>>()
                {
                    data = mapper.Map<List<OfficeResponseDTO>>(offices),
                    remarks = offices.Any() ? "Success" : "No record found.",
                    success = true
                };
            }
            catch (Exception ex)
            {
                return new ResponseModel<List<OfficeResponseDTO>>()
                {
                    remarks = $"There was a fatal error:",
                    success = false
                };
            }
        }



        public async Task<ResponseModel<List<OfficeResponseDTO>>> GetOfficeByTenantId(string tenantId)
        {
            try
            {
                if (!Guid.TryParse(tenantId, out var parsedTenantId))
                {
                    return new ResponseModel<List<OfficeResponseDTO>>
                    {
                        remarks = "Invalid tenant id.",
                        success = false
                    };
                }

                var today = DateTime.Today;
                var offices = await (
                    from agreement in db.tbl_agreement.AsNoTracking()
                    join officeAgreement in db.tbl_office_agreement.AsNoTracking()
                        on agreement.agreement_id equals officeAgreement.fk_agreement
                    join office in db.tbl_office.AsNoTracking()
                        on officeAgreement.fk_office equals office.office_id
                    where agreement.fk_tenant == parsedTenantId
                          && agreement.is_active
                          && !agreement.is_deleted
                          && agreement.agreement_start_date.Date <= today
                          && agreement.agreement_end_date.Date >= today
                          && !officeAgreement.is_deleted
                          && office.is_active
                          && !office.is_deleted
                          && office.business.is_active
                          && !office.business.is_deleted
                    select office)
                    .Include(x => x.section)
                    .Distinct()
                    .OrderBy(x => x.office_name)
                    .ToListAsync();

                return new ResponseModel<List<OfficeResponseDTO>>
                {
                    data = mapper.Map<List<OfficeResponseDTO>>(offices),
                    remarks = offices.Any() ? "Success" : "No active agreement offices found.",
                    success = true
                };
            }
            catch (Exception ex)
            {
                return new ResponseModel<List<OfficeResponseDTO>>
                {
                    remarks = $"There was a fatal error: {ex.Message}",
                    success = false
                };
            }
        }
        public async Task<ResponseModel> DeleteAgreementById(string agreementId)
        {
            try
            {
                var agreement = await db.tbl_agreement.FirstOrDefaultAsync(x => x.agreement_id == Guid.Parse(agreementId) && !x.is_deleted);

                if (agreement != null)
                {
                    agreement.is_deleted = true;
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
                    remarks = $"There was a fatal error",
                    success = false
                };
            }
        }

        public async Task<ResponseModel> RemoveOfficeFromAgreement(RemoveOfficeFromAgreementRequestDTO requestDTO)
        {
            try
            {
                var agreementId = Guid.Parse(requestDTO.agreementId);
                var officeId = Guid.Parse(requestDTO.officeId);
                var assignment = await db.tbl_office_agreement
                    .FirstOrDefaultAsync(x => x.fk_office == officeId
                                              && x.fk_agreement == agreementId
                                              && !x.is_deleted);
                if (assignment is null)
                    return new ResponseModel { remarks = "Office assignment was not found.", success = false };

                assignment.is_deleted = true;

                var today = DateTime.Today;
                var occupiedByAnotherAgreement = await (
                    from agreement in db.tbl_agreement.AsNoTracking()
                    join officeAgreement in db.tbl_office_agreement.AsNoTracking()
                        on agreement.agreement_id equals officeAgreement.fk_agreement
                    where officeAgreement.fk_office == officeId
                          && officeAgreement.office_agreement_id != assignment.office_agreement_id
                          && !officeAgreement.is_deleted
                          && agreement.is_active
                          && !agreement.is_deleted
                          && agreement.agreement_start_date.Date <= today
                          && agreement.agreement_end_date.Date >= today
                    select agreement.agreement_id)
                    .AnyAsync();

                var office = await db.tbl_office.FirstOrDefaultAsync(x => x.office_id == officeId);
                if (office is not null)
                {
                    office.is_occupied = occupiedByAnotherAgreement;
                    office.updated_at = DateTime.Now;
                }

                await db.SaveChangesAsync();
                return new ResponseModel { remarks = "Office removed successfully.", success = true };
            }
            catch (Exception ex)
            {
                return new ResponseModel { remarks = $"There was a fatal error: {ex.Message}", success = false };
            }
        }
    }

}
