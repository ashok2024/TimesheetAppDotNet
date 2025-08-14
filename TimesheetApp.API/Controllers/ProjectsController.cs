using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using TimesheetApp.Application.DTOs;
using TimesheetApp.Application.Interfaces;
using TimesheetApp.Domain.Entities;
using TimesheetApp.Infrastructure.Repositories;
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

    [HttpGet("paginated")]
    public async Task<IActionResult> GetPaginated([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        var result = await _projectService.GetPagedAsync(page, pageSize);
        return Ok(result);
    }
    [HttpGet("filtered-paginated")]
    public async Task<IActionResult> GetFilteredPaginatedProjects([FromQuery] int page = 1,
                                                               [FromQuery] int pageSize = 10,
                                                               [FromQuery] string? name = null,
                                                               [FromQuery] string? startDate = null,
                                                               [FromQuery] string? endDate = null)
    {
        var result = await _projectService.GetPagedAsyncFilter(page, pageSize, name, startDate, endDate);
        return Ok(result);
    }
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
    [HttpGet("export-csv")]
    public async Task<IActionResult> ExportProjectsCsv([FromQuery] string? name, [FromQuery] string? startDate, [FromQuery] string? endDate)
    {
        var projects = await _projectService.GetFilteredProjectsForExportAsync(name, startDate, endDate);

        var csvBuilder = new StringBuilder();
        csvBuilder.AppendLine("Project Name,Description,Start Date,End Date,Total Hour Spent,Assigned Users");

        foreach (var project in projects)
        {
            var userNames = project.Users != null ? string.Join(" | ", project.Users.Select(u => u.FullName)) : "";
            csvBuilder.AppendLine($"\"{project.Name}\",\"{project.Description}\",\"{project.StartDate:yyyy-MM-dd}\",\"{project.EndDate:yyyy-MM-dd}\",{project.TotalHourSpent},\"{userNames}\"");
        }

        var bytes = Encoding.UTF8.GetBytes(csvBuilder.ToString());
        return File(bytes, "text/csv", "projects_export.csv");
    }
    [HttpGet("by-user/{userId}")]
    public async Task<IActionResult> GetProjectsByUserId(int userId)
    {
        var projects = await _projectService.GetProjectsByUserIdAsync(userId);

        if (projects == null || projects.Count == 0)
            return NotFound();

        return Ok(projects);
    }

}
