using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TimesheetApp.Application.DTOs;
using TimesheetApp.Application.Interfaces;

namespace TimesheetApp.API.Controllers;

[Route("api/dashboard")]
[ApiController]
[Authorize]
public class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboardService;

    public DashboardController(IDashboardService dashboardService)
    {
        _dashboardService = dashboardService;
    }

    [HttpGet("hours-per-project")]
    public async Task<IActionResult> GetHoursPerProject([FromQuery] int? limit = null)
    {
        var result = await _dashboardService.GetHoursPerProjectAsync(limit);
        return Ok(result);
    }

    [HttpGet("task-trends")]
    public async Task<IActionResult> GetTaskTrends()
    {
        var result = await _dashboardService.GetTaskTrendsAsync();
        return Ok(result);
    }

    [HttpGet("weekly-summary")]
    public async Task<IActionResult> GetWeeklySummary()
    {
        var result = await _dashboardService.GetWeeklySummaryAsync();
        return Ok(result);
    }

    [HttpPost("timesheets/filter")]
    public async Task<IActionResult> FilterTimesheets([FromBody] TimesheetFilterDto filter)
    {
        var result = await _dashboardService.FilterTimesheetsAsync(filter);
        return Ok(result);
    }

    [HttpPost("timesheets/export")]
    public async Task<IActionResult> ExportTimesheets([FromBody] TimesheetFilterDto filter)
    {
        var fileBytes = await _dashboardService.ExportTimesheetsToCsvAsync(filter);
        return File(fileBytes, "text/csv", "TimesheetsExport.csv");
    }
}
