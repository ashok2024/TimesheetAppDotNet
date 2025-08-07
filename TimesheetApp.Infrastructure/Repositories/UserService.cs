using Dapper;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TimesheetApp.Application.DTOs;
using TimesheetApp.Application.Interfaces;
using TimesheetApp.Domain.Entities;
using TimesheetApp.Infrastructure.Data;

namespace TimesheetApp.Infrastructure.Repositories
{
    public class UserService : IUserService
    {
        private readonly IDbConnectionFactory _dbFactory;
        private readonly IPasswordHasher<User> _passwordHasher;

        public UserService(IDbConnectionFactory dbFactory, IPasswordHasher<User> passwordHasher)
        {
            _dbFactory = dbFactory;
            _passwordHasher = passwordHasher;
        }

        public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
        {
            using var conn = _dbFactory.CreateConnection();

            var sql = @"
                        SELECT 
                            Id, 
                            EmpId,   
                            UserName, 
                            Email, 
                            FullName, 
                            PhoneNumber, 
                            Department, 
                            Role, 
                            DateOfJoining, 
                            IsActive
                        FROM Users 
                        WHERE IsActive = 1";

            return await conn.QueryAsync<UserDto>(sql);
        }

        public async Task<(IEnumerable<UserDto> Users, int TotalCount)> GetPaginatedUsersAsync(int page, int pageSize)
        {
            using var conn = _dbFactory.CreateConnection();

            var offset = (page - 1) * pageSize;

            var sql = @"
        SELECT 
            Id, 
            EmpId,   
            UserName, 
            Email, 
            FullName, 
            PhoneNumber, 
            Department, 
            Role, 
            DateOfJoining, 
            IsActive
        FROM Users
        WHERE IsActive = 1
        ORDER BY Id
        OFFSET @Offset ROWS
        FETCH NEXT @PageSize ROWS ONLY;

        SELECT COUNT(*) FROM Users WHERE IsActive = 1;
    ";

            using var multi = await conn.QueryMultipleAsync(sql, new { Offset = offset, PageSize = pageSize });
            var users = await multi.ReadAsync<UserDto>();
            var totalCount = await multi.ReadSingleAsync<int>();

            return (users, totalCount);
        }

        public async Task<UserDto?> GetUserByIdAsync(int id)
        {
            using var conn = _dbFactory.CreateConnection();
            var sql = "SELECT * FROM Users WHERE Id = @Id AND IsActive = 1";
            return await conn.QueryFirstOrDefaultAsync<UserDto>(sql, new { Id = id });
        }

        public async Task<UserDto> CreateUserAsync(CreateUserDto dto)
        {
            var user = new User
            {
                FullName = dto.FullName,
                EmpId = dto.EmpId,
                Email = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                Department = dto.Department,
                Role = dto.Role,
                DateOfJoining = dto.DateOfJoining,
                PasswordHash = _passwordHasher.HashPassword(null!, dto.Password),
                CreatedDate = DateTime.UtcNow,
                CreatedBy = "System", // Replace with logged-in user later
                IsActive = true,
                UserName = dto.Email
            };

            using var conn = _dbFactory.CreateConnection();
            var sql = @"
                 INSERT INTO Users 
                 (FullName, EmpId, Email, UserName, PhoneNumber, Department, Role, DateOfJoining, PasswordHash, CreatedDate, CreatedBy, IsActive)
                 VALUES 
                 (@FullName, @EmpId, @Email, @UserName, @PhoneNumber, @Department, @Role, @DateOfJoining, @PasswordHash, @CreatedDate, @CreatedBy, @IsActive)";

            await conn.ExecuteAsync(sql, user);

            return new UserDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role
            };
        }


        public async Task<bool> UpdateUserAsync(int id, UpdateUserDto dto)
        {
            using var conn = _dbFactory.CreateConnection();
            var sql = @"
                          UPDATE Users 
                          SET 
                              FullName = @FullName, 
                              Role = @Role,
                              EmpId = @EmpId,
                              PhoneNumber = @PhoneNumber,
                              Department = @Department,
                              DateOfJoining = @DateOfJoining,
                              UpdatedDate = @UpdatedDate,
                              UpdatedBy = @UpdatedBy 
                          WHERE Id = @Id AND IsActive = 1";

            var affected = await conn.ExecuteAsync(sql, new
            {
                Id = id,
                dto.FullName,
                dto.Role,
                dto.EmpId,
                dto.PhoneNumber,
                dto.Department,
                dto.DateOfJoining,
                UpdatedDate = DateTime.UtcNow,
                UpdatedBy = "System"
            });

            return affected > 0;
        }


        public async Task<bool> DeleteUserAsync(int id)
        {
            using var conn = _dbFactory.CreateConnection();
            var sql = @"UPDATE Users 
                        SET IsActive = 0, UpdatedDate = @UpdatedDate, UpdatedBy = @UpdatedBy 
                        WHERE Id = @Id";
            var affected = await conn.ExecuteAsync(sql, new
            {
                Id = id,
                UpdatedDate = DateTime.UtcNow,
                UpdatedBy = "System"
            });
            return affected > 0;
        }

        public async Task<PagedResult<UserDto>> GetFilteredPaginatedAsync(UserFilterRequest filter)
        {
            using var conn = _dbFactory.CreateConnection();

            var conditions = new List<string> { "u.IsActive = 1" };
            var parameters = new DynamicParameters();
            parameters.Add("Offset", (filter.Page - 1) * filter.PageSize);
            parameters.Add("PageSize", filter.PageSize);

            if (filter.ProjectId.HasValue)
            {
                conditions.Add("pu.ProjectId = @ProjectId");
                parameters.Add("ProjectId", filter.ProjectId.Value);
            }

            if (filter.FromDate.HasValue)
            {
                conditions.Add("u.DateOfJoining >= @FromDate");
                parameters.Add("FromDate", filter.FromDate.Value);
            }

            if (filter.ToDate.HasValue)
            {
                conditions.Add("u.DateOfJoining <= @ToDate");
                parameters.Add("ToDate", filter.ToDate.Value);
            }

            var whereClause = string.Join(" AND ", conditions);

            var countSql = $@"
        SELECT COUNT(DISTINCT u.Id)
        FROM Users u
        LEFT JOIN ProjectUsers pu ON pu.UserId = u.Id
        WHERE {whereClause}";

            var totalCount = await conn.ExecuteScalarAsync<int>(countSql, parameters);

            var sql = $@"
        SELECT DISTINCT 
            u.Id, u.FullName, u.EmpId, u.Email, u.PhoneNumber, u.Department, u.Role, u.DateOfJoining, u.IsActive
        FROM Users u
        LEFT JOIN ProjectUsers pu ON pu.UserId = u.Id
        WHERE {whereClause}
        ORDER BY u.Id
        OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

            var users = await conn.QueryAsync<UserDto>(sql, parameters);

            return new PagedResult<UserDto>
            {
                Data = users.ToList(),
                Total = totalCount
            };
        }

    }
}
