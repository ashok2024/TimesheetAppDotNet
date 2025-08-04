using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TimesheetApp.Application.DTOs;
using TimesheetApp.Application.Interfaces;
[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ProjectsController : ControllerBase
{
    private readonly IProjectService _projectService;

    public ProjectsController(IProjectService projectService)
    {
        _projectService = projectService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await _projectService.GetAllAsync());

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var project = await _projectService.GetByIdAsync(id);
        return project == null ? NotFound() : Ok(project);
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateProjectDto dto)
    {
        var createdBy = "admin"; // Replace with actual user from context
        var result = await _projectService.CreateAsync(dto, createdBy);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, UpdateProjectDto dto)
    {
        var updatedBy = "admin"; // Replace with actual user from context
        var success = await _projectService.UpdateAsync(id, dto, updatedBy);
        return success ? NoContent() : NotFound();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var success = await _projectService.DeleteAsync(id);
        return success ? NoContent() : NotFound();
    }
}
