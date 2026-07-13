using EMO.Extensions.MiddleWare;
using EMO.Models.DTOs.HistoricalDataDTOs;
using EMO.Repositories.HistoricalDataRepo;
using Microsoft.AspNetCore.Mvc;

namespace EMO.Controllers;

[ApiController]
[ApiKey]
[Route("api/historical-data")]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public sealed class HistoricalDataController : ControllerBase
{
    private readonly IHistoricalDataService _service;

    public HistoricalDataController(IHistoricalDataService service)
    {
        _service = service;
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
}
