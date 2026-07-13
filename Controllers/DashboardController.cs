using EMO.Extensions.MiddleWare;
using EMO.Models.DTOs.DeepDiveDTOs;
using EMO.Repositories.DashboardServicesRepo;
using EMO.Repositories.DeepDiveRepo;
using EnergyMonitor.DTOs;
using EnergyMonitor.Services;
using Microsoft.AspNetCore.Mvc;

namespace EnergyMonitor.Controllers;

/// <summary>
/// Drill-down energy dashboard API.
/// Every endpoint accepts ?range=24h|7d|30d|90d|1y (or explicit ?from=&to= ISO timestamps).
///
/// Drill-down path:
///   /business/{id}  →  /facility/{id}  →  /building/{id}
///   →  /floor/{id}  →  /section/{id}   →  /office/{id}  →  /device/{id}  →  /sensor/{id}
/// </summary>
[ApiController]
[ApiKey]
[Route("api/dashboard")]
[Produces("application/json")]
public class DashboardController : ControllerBase
{
    private readonly DashboardService _svc;
    private readonly IDeepDiveService _deepDive;

    public DashboardController(DashboardService svc, IDeepDiveService deepDive)
    {
        _svc = svc;
        _deepDive = deepDive;
    }

    // ─── Analysis snapshot ───────────────────────────────────────────────────

    /// <summary>
    /// Returns the persisted hierarchy analysis snapshot used by the Analysis tab.
    /// The absolute route is retained so the existing Angular service does not change.
    /// </summary>
    [HttpGet("~/api/deep-dive/{level}/{id:guid}")]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public async Task<IActionResult> GetDeepDive(
        string level,
        Guid id,
        [FromQuery] DeepDiveQueryDto query)
    {
        var allowed = new[]
        {
            "business", "facility", "building", "floor",
            "section", "office", "device", "sensor"
        };

        if (!allowed.Contains(level.ToLowerInvariant()))
        {
            return BadRequest(
                "Unknown level. Use business|facility|building|floor|section|office|device|sensor.");
        }

        var result = await _deepDive.GetAsync(level, id, query);
        return result is null ? NotFound() : Ok(result);
    }

    // ─── Business ─────────────────────────────────────────────────────────────

    /// <summary>Top-level view: all facilities under a business with KPIs.</summary>
    [HttpGet("business/{businessId:guid}")]
    [ProducesResponseType(typeof(BusinessDashboardDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetBusiness(
        Guid businessId,
        [FromQuery] DashboardQueryParams q)
    {
        var result = await _svc.GetBusinessAsync(businessId, q);
        return result is null ? NotFound() : Ok(result);
    }
    // ─── Facility ─────────────────────────────────────────────────────────────

    /// <summary>Facility view: all buildings with energy KPIs.</summary>
    [HttpGet("facility/{facilityId:guid}")]
    [ProducesResponseType(typeof(FacilityDashboardDto), 200)]
    public async Task<IActionResult> GetFacility(
        Guid facilityId,
        [FromQuery] DashboardQueryParams q)
        => Ok(await _svc.GetFacilityAsync(facilityId, q));

    // ─── Building ─────────────────────────────────────────────────────────────

    /// <summary>Building view: all floors with energy KPIs.</summary>
    [HttpGet("building/{buildingId:guid}")]
    [ProducesResponseType(typeof(BuildingDashboardDto), 200)]
    public async Task<IActionResult> GetBuilding(
        Guid buildingId,
        [FromQuery] DashboardQueryParams q)
        => Ok(await _svc.GetBuildingAsync(buildingId, q));

    // ─── Floor ────────────────────────────────────────────────────────────────

    /// <summary>Floor view: all sections with energy KPIs.</summary>
    [HttpGet("floor/{floorId:guid}")]
    [ProducesResponseType(typeof(FloorDashboardDto), 200)]
    public async Task<IActionResult> GetFloor(
        Guid floorId,
        [FromQuery] DashboardQueryParams q)
        => Ok(await _svc.GetFloorAsync(floorId, q));

    // ─── Section ──────────────────────────────────────────────────────────────

    /// <summary>Section / zone view: all offices with energy KPIs.</summary>
    [HttpGet("section/{sectionId:guid}")]
    [ProducesResponseType(typeof(SectionDashboardDto), 200)]
    public async Task<IActionResult> GetSection(
        Guid sectionId,
        [FromQuery] DashboardQueryParams q)
        => Ok(await _svc.GetSectionAsync(sectionId, q));

    // ─── Office ───────────────────────────────────────────────────────────────

    /// <summary>Office view: all sensors in this office with live readings.</summary>
    [HttpGet("office/{officeId:guid}")]
    [ProducesResponseType(typeof(OfficeDashboardDto), 200)]
    public async Task<IActionResult> GetOffice(
        Guid officeId,
        [FromQuery] DashboardQueryParams q)
        => Ok(await _svc.GetOfficeAsync(officeId, q));


    // ─── Device ────────────────────────────────────────────────────────────────

    /// <summary>Device view: all sensors assigned to this device.</summary>
    [HttpGet("device/{deviceId:guid}")]
    [ProducesResponseType(typeof(DeviceDashboardDto), 200)]
    public async Task<IActionResult> GetDevice(
        Guid deviceId,
        [FromQuery] DashboardQueryParams q)
        => Ok(await _svc.GetDeviceAsync(deviceId, q));

    // ─── Sensor ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Full sensor detail: all time-series, PF distribution, hourly demand,
    /// raw readings table, and anomaly alerts.
    /// </summary>
    [HttpGet("sensor/{sensorId:guid}")]
    [ProducesResponseType(typeof(SensorDashboardDto), 200)]
    public async Task<IActionResult> GetSensor(
        Guid sensorId,
        [FromQuery] DashboardQueryParams q)
        => Ok(await _svc.GetSensorAsync(sensorId, q));

    // ─── Breadcrumb helper ────────────────────────────────────────────────────

    /// <summary>
    /// Returns the full breadcrumb trail for any entity.
    /// Used by the frontend to build the navigation bar.
    /// GET /api/dashboard/breadcrumb/sensor/{id}
    /// GET /api/dashboard/breadcrumb/office/{id}  … etc.
    /// </summary>
    /// 

    [HttpGet("breadcrumb/{level}/{id:guid}")]
    [ProducesResponseType(typeof(List<BreadcrumbDto>), 200)]
    public async Task<IActionResult> GetBreadcrumb(string level, Guid id)
    {
        // Resolve full chain by following FK navigation properties upward.
        // Each service call is a single DB round-trip via EF Core.
        var crumbs = new List<BreadcrumbDto>();

        switch (level.ToLowerInvariant())
        {
            case "device":
                var device = await _svc.ResolveDeviceChainAsync(id);
                crumbs = device;
                break;
            case "sensor":
                var sensor = await _svc.ResolveSensorChainAsync(id);
                crumbs = sensor;
                break;
            case "office":
                var office = await _svc.ResolveOfficeChainAsync(id);
                crumbs = office;
                break;
            case "section":
                var section = await _svc.ResolveSectionChainAsync(id);
                crumbs = section;
                break;
            case "floor":
                var floor = await _svc.ResolveFloorChainAsync(id);
                crumbs = floor;
                break;
            case "building":
                var building = await _svc.ResolveBuildingChainAsync(id);
                crumbs = building;
                break;
            case "facility":
                var facility = await _svc.ResolveFacilityChainAsync(id);
                crumbs = facility;
                break;
            default:
                return BadRequest("Unknown level. Use: sensor|device|office|section|floor|building|facility");
        }

        return Ok(crumbs);
    }
}
