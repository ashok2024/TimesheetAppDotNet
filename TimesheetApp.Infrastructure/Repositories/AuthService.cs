// Infrastructure > Services > AuthService.cs
using Dapper;
using System.Data;
using TimesheetApp.Application.DTOs;
using TimesheetApp.Application.Services;
using TimesheetApp.Domain.Entities;
using TimesheetApp.Infrastructure.Data;

public class AuthService : IAuthService
{
    private readonly IDbConnectionFactory _dbFactory;

    public AuthService(IDbConnectionFactory dbFactory)
    {
        _dbFactory = dbFactory;
    }

    public async Task<bool> UsernameExistsAsync(string username)
    {
        using var conn = _dbFactory.CreateConnection();
        var sql = "SELECT COUNT(1) FROM Users WHERE Username = @Username";
        var count = await conn.ExecuteScalarAsync<int>(sql, new { Username = username });
        return count > 0;
    }

    public async Task<User> RegisterAsync(RegisterRequest request)
    {
        var user = new User
        {
            UserName = request.Username,
            PasswordHash = PasswordHasher.Hash(request.Password),
            Role = request.Role
        };

        using var conn = _dbFactory.CreateConnection();
        var sql = @"INSERT INTO Users (Username, PasswordHash, Role)
                    VALUES ( @Username, @PasswordHash, @Role)";
        await conn.ExecuteAsync(sql, user);

        return user;
    }

    public async Task<User?> GetUserByUsernameAsync(string username)
    {
        using var conn = _dbFactory.CreateConnection();
        var sql = "SELECT * FROM Users WHERE Username = @Username";
        return await conn.QueryFirstOrDefaultAsync<User>(sql, new { Username = username });
    }
}
