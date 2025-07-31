using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TimesheetApp.Application.DTOs.TimesheetApp.Application.DTOs.TimesheetTask;
using TimesheetApp.Application.Interfaces;
using TimesheetApp.Domain.Entities;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TimesheetController : ControllerBase
{
    private readonly ITimesheetService _timesheetService;

    public TimesheetController(ITimesheetService timesheetService)
    {
        _timesheetService = timesheetService;
    }

    [HttpPost]
    [RequestSizeLimit(5 * 1024 * 1024)]
    public async Task<IActionResult> Create([FromForm] CreateTimesheetTaskRequest request, IFormFile? attachment)
    {
        var userIdStr = User.Claims.FirstOrDefault(c => c.Type == "id")?.Value;
        if (!int.TryParse(userIdStr, out var userId)) return Unauthorized();

        await _timesheetService.AddTaskAsync(request, attachment, userId);
        return Ok("Task created.");
    }

    [HttpGet("user")]
    public async Task<IActionResult> GetUserTasks()
    {
        var userIdStr = User.Claims.FirstOrDefault(c => c.Type == "id")?.Value;
        if (!int.TryParse(userIdStr, out var userId)) return Unauthorized();

        var tasks = await _timesheetService.GetTasksByUserAsync(userId);
        return Ok(tasks);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var userIdStr = User.Claims.FirstOrDefault(c => c.Type == "id")?.Value;
        if (!int.TryParse(userIdStr, out var userId)) return Unauthorized();

        var deleted = await _timesheetService.DeleteTaskAsync(id, userId);
        if (!deleted) return NotFound();

        return Ok("Task deleted.");
    }
}
