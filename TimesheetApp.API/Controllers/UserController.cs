using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text;
using TimesheetApp.Application.DTOs;
using TimesheetApp.Application.Interfaces;
using TimesheetApp.Infrastructure.Repositories;

namespace TimesheetApp.API.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize(Roles = "Admin")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;

    public UserController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet("GetAllUsers")]
    public async Task<IActionResult> GetAllUsers()
    {
        var users = await _userService.GetAllUsersAsync();
        return Ok(users);
    }
    [HttpGet("paginated")]
    public async Task<IActionResult> GetPaginatedUsers(int page = 1, int pageSize = 10)
    {
        var (users, totalCount) = await _userService.GetPaginatedUsersAsync(page, pageSize);
        return Ok(new { data = users, total = totalCount });
    }
    [HttpGet("{id}")]
    public async Task<IActionResult> GetUserById(int id)
    {
        var user = await _userService.GetUserByIdAsync(id);
        return user != null ? Ok(user) : NotFound();
    }

    [HttpPost("AddUser")]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserDto dto)
    {
        var result = await _userService.CreateUserAsync(dto);
        return CreatedAtAction(nameof(GetUserById), new { id = result.Id }, result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserDto dto)
    {
        var result = await _userService.UpdateUserAsync(id, dto);
        return result ? NoContent() : NotFound();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(int id)
    {
        var result = await _userService.DeleteUserAsync(id);
        return result ? NoContent() : NotFound();
    }
    [HttpPost("filter-paginated")]
    public async Task<IActionResult> GetFilteredPaginatedUsers([FromBody] UserFilterRequest filter)
    {
        var result = await _userService.GetFilteredPaginatedAsync(filter);
        return Ok(new { data = result.Data, total = result.Total });
    }

    [HttpGet("ExportCsv")]
    public async Task<IActionResult> ExportCsv()
    {
        var users = await _userService.GetAllUsersAsync(); // Same as your existing GetAllUsers

        if (users == null || !users.Any())
            return NotFound("No users found to export.");

        var csvBuilder = new StringBuilder();

        // Add CSV header
        csvBuilder.AppendLine("Full Name,Emp ID,Email,Phone Number,Department,Role,Date Of Joining,Is Active");

        // Add CSV rows
        foreach (var user in users)
        {
            csvBuilder.AppendLine($"{Escape(user.FullName)},{Escape(user.EmpId)},{Escape(user.Email)},{Escape(user.PhoneNumber)},{Escape(user.Department)},{Escape(user.Role)},{user.DateOfJoining:yyyy-MM-dd},{user.IsActive}");
        }

        var fileName = $"users_export_{DateTime.Now:yyyyMMddHHmmss}.csv";
        var fileContent = Encoding.UTF8.GetBytes(csvBuilder.ToString());

        return File(fileContent, "text/csv", fileName);
    }

    // Helper to escape special characters
    private string Escape(string? input)
    {
        if (string.IsNullOrWhiteSpace(input)) return "";
        return $"\"{input.Replace("\"", "\"\"")}\"";
    }
    [HttpGet("by-project/{projectId}")]
    public async Task<IActionResult> GetUsersByProjectId(int projectId)
    {
        try
        {
            var users = await _userService.GetUsersByProjectIdAsync(projectId);
            return Ok(users);
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Internal server error");
        }
    }

}
