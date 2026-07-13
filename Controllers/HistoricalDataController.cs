using EMO.Extensions.MiddleWare;
using EMO.Models.DTOs.HistoricalDataDTOs;
using EMO.Models.DTOs.UserDTOs;
using EMO.Repositories.HistoricalDataRepo;
using EMO.Repositories.UserAccessRepo;
using Microsoft.AspNetCore.Mvc;

namespace EMO.Controllers;

[ApiController]
[ApiKey]
[Route("api/historical-data")]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public sealed class HistoricalDataController : ControllerBase
{
    private readonly IHistoricalDataService _service;
    private readonly IUserAccessService _userAccess;

    public HistoricalDataController(
        IHistoricalDataService service,
        IUserAccessService userAccess)
    {
        _service = service;
        _userAccess = userAccess;
    }

    [HttpGet("{level}/{id:guid}")]
    public async Task<IActionResult> Get(
        string level,
        Guid id,
        [FromQuery] HistoricalDataQueryDto query,
        CancellationToken cancellationToken)
    {
        try
        {
            var access = await CurrentAccessAsync(cancellationToken);
            if (access is not null && (!access.IsLoginAllowed || access.IsTenant || !IsAllowed(access, level, id)))
                return Forbid();

            var result = await _service.GetAsync(level, id, query, cancellationToken);
            return result is null ? NotFound() : Ok(result);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }

    [HttpGet("{level}/{id:guid}/export")]
    public async Task<IActionResult> Export(
        string level,
        Guid id,
        [FromQuery] HistoricalDataQueryDto query,
        CancellationToken cancellationToken)
    {
        try
        {
            var access = await CurrentAccessAsync(cancellationToken);
            if (access is not null && (!access.IsLoginAllowed || access.IsTenant || !IsAllowed(access, level, id)))
                return Forbid();

            var result = await _service.ExportCsvAsync(level, id, query, cancellationToken);
            return File(result.Content, "text/csv; charset=utf-8", result.FileName);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    [HttpGet("tenant/{level}/{id:guid}")]
    public async Task<IActionResult> GetTenant(
        string level,
        Guid id,
        [FromQuery] HistoricalDataQueryDto query,
        CancellationToken cancellationToken)
    {
        try
        {
            var access = await CurrentAccessAsync(cancellationToken);
            if (access is null || !access.IsLoginAllowed || !access.IsTenant) return Unauthorized();
            if (!IsAllowed(access, level, id)) return Forbid();

            var result = await _service.GetTenantAsync(
                level, id, query, access.OfficeIds, cancellationToken);
            return result is null ? NotFound() : Ok(result);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
    }

    [HttpGet("tenant/{level}/{id:guid}/export")]
    public async Task<IActionResult> ExportTenant(
        string level,
        Guid id,
        [FromQuery] HistoricalDataQueryDto query,
        CancellationToken cancellationToken)
    {
        try
        {
            var access = await CurrentAccessAsync(cancellationToken);
            if (access is null || !access.IsLoginAllowed || !access.IsTenant) return Unauthorized();
            if (!IsAllowed(access, level, id)) return Forbid();

            var result = await _service.ExportTenantCsvAsync(
                level, id, query, access.OfficeIds, cancellationToken);
            return File(result.Content, "text/csv; charset=utf-8", result.FileName);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new { message = exception.Message });
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
    }

    private async Task<UserAccessScope?> CurrentAccessAsync(CancellationToken cancellationToken)
    {
        var currentUser = HttpContext.Items["User"] as UserResponseDTO;
        return currentUser is not null && Guid.TryParse(currentUser.userId, out var userId)
            ? await _userAccess.GetByUserIdAsync(userId, cancellationToken)
            : null;
    }

    private static bool IsAllowed(UserAccessScope access, string level, Guid id)
    {
        if (access.HasGlobalAccess) return true;
        return level.Trim().ToLowerInvariant() switch
        {
            "business" => access.BusinessIds.Contains(id),
            "facility" => access.FacilityIds.Contains(id),
            "building" => access.BuildingIds.Contains(id),
            "floor" => access.FloorIds.Contains(id),
            "section" => access.SectionIds.Contains(id),
            "office" => access.OfficeIds.Contains(id),
            "device" => access.DeviceIds.Contains(id),
            "sensor" => access.SensorIds.Contains(id),
            _ => false
        };
    }
}
