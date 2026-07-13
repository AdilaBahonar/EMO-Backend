using EMO.Models.DBModels;
using Microsoft.EntityFrameworkCore;

namespace EMO.Repositories.UserAccessRepo;

public sealed class UserAccessScope
{
    public Guid UserId { get; init; }
    public string UserName { get; init; } = string.Empty;
    public string StoredTokenHash { get; init; } = string.Empty;
    public string UserTypeName { get; init; } = string.Empty;
    public int UserTypeLevel { get; init; }
    public bool IsLoginAllowed { get; init; }
    public string DenialReason { get; init; } = string.Empty;
    public bool HasGlobalAccess { get; init; }
    public bool IsBusinessAdmin => UserTypeLevel == 1;
    public bool IsTenant => UserTypeLevel == 2;
    public List<Guid> BusinessIds { get; init; } = new();
    public List<Guid> FacilityIds { get; init; } = new();
    public List<Guid> BuildingIds { get; init; } = new();
    public List<Guid> FloorIds { get; init; } = new();
    public List<Guid> SectionIds { get; init; } = new();
    public List<Guid> OfficeIds { get; init; } = new();
    public List<Guid> DeviceIds { get; init; } = new();
    public List<Guid> SensorIds { get; init; } = new();
}

public interface IUserAccessService
{
    Task<UserAccessScope?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);
    Task<UserAccessScope?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<List<Guid>> GetActiveTenantOfficeIdsAsync(Guid tenantId, Guid? businessId = null, CancellationToken cancellationToken = default);
}

public sealed class UserAccessService : IUserAccessService
{
    private readonly DBUserManagementContext _db;

    public UserAccessService(DBUserManagementContext db)
    {
        _db = db;
    }

    public async Task<UserAccessScope?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        var userId = await _db.tbl_user.AsNoTracking()
            .Where(x => x.user_name == username && !x.is_deleted)
            .Select(x => (Guid?)x.user_id)
            .FirstOrDefaultAsync(cancellationToken);
        return userId.HasValue ? await GetByUserIdAsync(userId.Value, cancellationToken) : null;
    }

    public async Task<UserAccessScope?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _db.tbl_user.AsNoTracking()
            .Where(x => x.user_id == userId && !x.is_deleted)
            .Select(x => new
            {
                x.user_id,
                x.user_name,
                x.user_token,
                x.is_active,
                x.fk_business,
                SubTypeActive = x.sub_user_type.is_active && !x.sub_user_type.is_deleted,
                UserTypeActive = x.sub_user_type.user_type.is_active && !x.sub_user_type.user_type.is_deleted,
                TypeName = x.sub_user_type.user_type.user_type_name,
                TypeLevel = x.sub_user_type.user_type.user_type_level
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (user is null) return null;

        var baseDenied = !user.is_active || !user.SubTypeActive || !user.UserTypeActive;
        if (baseDenied)
        {
            return new UserAccessScope
            {
                UserId = user.user_id,
                UserName = user.user_name,
                StoredTokenHash = user.user_token,
                UserTypeName = user.TypeName,
                UserTypeLevel = user.TypeLevel,
                IsLoginAllowed = false,
                DenialReason = "User account is inactive."
            };
        }

        if (user.TypeLevel == 0)
        {
            return new UserAccessScope
            {
                UserId = user.user_id,
                UserName = user.user_name,
                StoredTokenHash = user.user_token,
                UserTypeName = user.TypeName,
                UserTypeLevel = user.TypeLevel,
                IsLoginAllowed = true,
                HasGlobalAccess = true
            };
        }

        List<Guid> businessIds;
        List<Guid> officeIds;

        if (user.TypeLevel == 1)
        {
            if (!user.fk_business.HasValue)
                return Denied(user.user_id, user.user_name, user.user_token, user.TypeName, user.TypeLevel, "No business is assigned to this user.");

            var businessActive = await _db.tbl_business.AsNoTracking()
                .AnyAsync(x => x.business_id == user.fk_business.Value && x.is_active && !x.is_deleted, cancellationToken);
            if (!businessActive)
                return Denied(user.user_id, user.user_name, user.user_token, user.TypeName, user.TypeLevel, "Business is inactive.");

            businessIds = new List<Guid> { user.fk_business.Value };
            officeIds = await _db.tbl_office.AsNoTracking()
                .Where(x => businessIds.Contains(x.fk_business) && x.is_active && !x.is_deleted)
                .Select(x => x.office_id)
                .Distinct()
                .ToListAsync(cancellationToken);
        }
        else if (user.TypeLevel == 2)
        {
            officeIds = await GetActiveTenantOfficeIdsAsync(user.user_id, null, cancellationToken);
            if (officeIds.Count == 0)
                return Denied(user.user_id, user.user_name, user.user_token, user.TypeName, user.TypeLevel,
                    "No active agreement is available. Your agreement may be expired or not started yet.");

            businessIds = await _db.tbl_office.AsNoTracking()
                .Where(x => officeIds.Contains(x.office_id) && x.is_active && !x.is_deleted && x.business.is_active && !x.business.is_deleted)
                .Select(x => x.fk_business)
                .Distinct()
                .ToListAsync(cancellationToken);
        }
        else
        {
            businessIds = user.fk_business.HasValue ? new List<Guid> { user.fk_business.Value } : new List<Guid>();
            officeIds = user.fk_business.HasValue
                ? await _db.tbl_office.AsNoTracking()
                    .Where(x => x.fk_business == user.fk_business.Value && x.is_active && !x.is_deleted)
                    .Select(x => x.office_id).ToListAsync(cancellationToken)
                : new List<Guid>();
        }

        var hierarchy = await _db.tbl_office.AsNoTracking()
            .Where(x => officeIds.Contains(x.office_id) && x.is_active && !x.is_deleted)
            .Select(x => new
            {
                OfficeId = x.office_id,
                SectionId = x.fk_section,
                FloorId = x.section.fk_floor,
                BuildingId = x.section.floor.fk_building,
                FacilityId = x.section.floor.building.fk_facility
            })
            .ToListAsync(cancellationToken);

        var deviceIds = await _db.tbl_device.AsNoTracking()
            .Where(x => officeIds.Contains(x.fk_office) && x.is_active && !x.is_deleted)
            .Select(x => x.device_id)
            .Distinct()
            .ToListAsync(cancellationToken);

        var sensorIds = deviceIds.Count == 0
            ? new List<Guid>()
            : await _db.tbl_sensor.AsNoTracking()
                .Where(x => deviceIds.Contains(x.fk_device) && !x.is_deleted)
                .Select(x => x.sensor_id)
                .Distinct()
                .ToListAsync(cancellationToken);

        return new UserAccessScope
        {
            UserId = user.user_id,
            UserName = user.user_name,
            StoredTokenHash = user.user_token,
            UserTypeName = user.TypeName,
            UserTypeLevel = user.TypeLevel,
            IsLoginAllowed = true,
            BusinessIds = businessIds.Distinct().ToList(),
            FacilityIds = hierarchy.Select(x => x.FacilityId).Distinct().ToList(),
            BuildingIds = hierarchy.Select(x => x.BuildingId).Distinct().ToList(),
            FloorIds = hierarchy.Select(x => x.FloorId).Distinct().ToList(),
            SectionIds = hierarchy.Select(x => x.SectionId).Distinct().ToList(),
            OfficeIds = hierarchy.Select(x => x.OfficeId).Distinct().ToList(),
            DeviceIds = deviceIds,
            SensorIds = sensorIds
        };
    }

    public async Task<List<Guid>> GetActiveTenantOfficeIdsAsync(
        Guid tenantId,
        Guid? businessId = null,
        CancellationToken cancellationToken = default)
    {
        var today = DateTime.Today;
        var query =
            from agreement in _db.tbl_agreement.AsNoTracking()
            join officeAgreement in _db.tbl_office_agreement.AsNoTracking()
                on agreement.agreement_id equals officeAgreement.fk_agreement
            join office in _db.tbl_office.AsNoTracking()
                on officeAgreement.fk_office equals office.office_id
            join business in _db.tbl_business.AsNoTracking()
                on agreement.fk_business equals business.business_id
            where agreement.fk_tenant == tenantId
                  && agreement.is_active
                  && !agreement.is_deleted
                  && agreement.agreement_start_date.Date <= today
                  && agreement.agreement_end_date.Date >= today
                  && !officeAgreement.is_deleted
                  && office.is_active
                  && !office.is_deleted
                  && business.is_active
                  && !business.is_deleted
                  && (!businessId.HasValue || agreement.fk_business == businessId.Value)
            select office.office_id;

        return await query.Distinct().ToListAsync(cancellationToken);
    }

    private static UserAccessScope Denied(Guid userId, string username, string token, string typeName, int typeLevel, string reason) => new()
    {
        UserId = userId,
        UserName = username,
        StoredTokenHash = token,
        UserTypeName = typeName,
        UserTypeLevel = typeLevel,
        IsLoginAllowed = false,
        DenialReason = reason
    };
}
