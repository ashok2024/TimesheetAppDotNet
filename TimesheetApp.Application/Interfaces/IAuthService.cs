using TimesheetApp.Application.DTOs;
using TimesheetApp.Domain.Entities;

public interface IAuthService
{
    Task<bool> UsernameExistsAsync(string username);
    Task<User> RegisterAsync(RegisterRequest request);
    Task<User?> GetUserByUsernameAsync(string username);
}