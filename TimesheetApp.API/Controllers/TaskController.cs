using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TimesheetApp.Application.DTOs;
using TimesheetApp.Application.Interfaces;
using TimesheetApp.Domain.Entities;
[Authorize]
[ApiController]
[Route("api/[controller]")]
public class TaskController : ControllerBase
{
    private readonly ITaskService _taskService;

    public TaskController(ITaskService taskService)
    {
        _taskService = taskService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await _taskService.GetAllTasksAsync());

    [HttpGet("project/{projectId}/paginated")]
    public async Task<IActionResult> GetPaginatedTasksByProjectId(int projectId, int page = 1, int pageSize = 10)
    {
        var result = await _taskService.GetTasksByProjectIdAsync(projectId, page, pageSize);
        return Ok(result);
    }
    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var task = await _taskService.GetTaskByIdAsync(id);
        return task is not null ? Ok(task) : NotFound();
    }

    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Create([FromForm] CreateTaskItemDto dto)
    {
        var createdBy = User?.Identity?.Name ?? "system";
        var task = await _taskService.CreateTaskAsync(dto, createdBy);
        return CreatedAtAction(nameof(Get), new { id = task.Id }, task);
    }

    [HttpPut("{id}")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Update(int id, [FromForm] UpdateTaskItemDto dto)
    {
        var updatedBy = User?.Identity?.Name ?? "system";
        return await _taskService.UpdateTaskAsync(id, dto, updatedBy)
            ? NoContent()
            : NotFound();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        return await _taskService.DeleteTaskAsync(id)
            ? NoContent()
            : NotFound();
    }
    //[HttpGet("project/{projectId}")]
    //public async Task<IActionResult> GetTasksByProjectId(int projectId)
    //{
    //    var tasks = await _taskService.GetTasksByProjectIdAsync(projectId);
    //    return Ok(tasks);
    //}
}
