using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TimesheetApp.Application.DTOs.TimesheetApp.Application.DTOs.TimesheetTask;
using TimesheetApp.Application.Interfaces;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class TimesheetController : ControllerBase
{
    private readonly ITimesheetService _timesheetService;

    public TimesheetController(ITimesheetService timesheetService)
    {
        _timesheetService = timesheetService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(await _timesheetService.GetAllAsync());

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var item = await _timesheetService.GetByIdAsync(id);
        return item is not null ? Ok(item) : NotFound();
    }

    // ✅ New filtered GET endpoint
    [HttpGet("filter")]
    public async Task<IActionResult> GetByFilter(
        [FromQuery] int? projectId,
        [FromQuery] int? taskId,
        [FromQuery] int? userId)
    {
        var items = await _timesheetService.GetByFilterAsync(projectId, taskId, userId);
        return Ok(items);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateTimesheetDto dto)
    {
        var createdBy = User?.Identity?.Name ?? "system";
        var item = await _timesheetService.CreateAsync(dto, createdBy);
        return CreatedAtAction(nameof(Get), new { id = item.Id }, item);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, UpdateTimesheetDto dto)
    {
        var updatedBy = User?.Identity?.Name ?? "system";
        return await _timesheetService.UpdateAsync(id, dto, updatedBy)
            ? NoContent()
            : NotFound();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        return await _timesheetService.DeleteAsync(id)
            ? NoContent()
            : NotFound();
    }
    [HttpGet("by-task-user")]
    public async Task<IActionResult> GetByTaskAndUser([FromQuery] int taskId)
    {
        try
        {
            var result = await _timesheetService.GetByTaskAndUserAsync(taskId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            // Optionally log the error
            return StatusCode(500, $"An error occurred: {ex.Message}");
        }
    }
}
    