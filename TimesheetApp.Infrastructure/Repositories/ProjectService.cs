using Dapper;
using TimesheetApp.Application.DTOs;
using TimesheetApp.Application.Interfaces;
using TimesheetApp.Domain.Entities;
using TimesheetApp.Infrastructure.Data;

public class ProjectService : IProjectService
{
    private readonly IDbConnectionFactory _dbFactory;

    public ProjectService(IDbConnectionFactory dbFactory)
    {
        _dbFactory = dbFactory;
    }

    public async Task<PagedResult<ProjectDto>> GetPagedAsync(int page, int pageSize)
    {
        using var conn = _dbFactory.CreateConnection();

        // 1. Get total count
        var countSql = "SELECT COUNT(*) FROM Projects WHERE IsActive = 1";
        var totalCount = await conn.ExecuteScalarAsync<int>(countSql);

        // 2. Get paged data with users
        var sql = @"
        SELECT 
            p.Id, p.Name, p.Description, p.StartDate, p.EndDate, p.TotalHourSpent,
            u.Id, u.FullName, u.EmpId, u.Email, u.PhoneNumber, u.Department, u.Role
        FROM Projects p
        LEFT JOIN ProjectUsers pu ON pu.ProjectId = p.Id
        LEFT JOIN Users u ON u.Id = pu.UserId
        WHERE p.IsActive = 1
        ORDER BY p.Id
        OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

        var projectMap = new Dictionary<int, ProjectDto>();

        var result = await conn.QueryAsync<ProjectDto, UserDto, ProjectDto>(
            sql,
            (project, user) =>
            {
                if (!projectMap.TryGetValue(project.Id, out var proj))
                {
                    proj = new ProjectDto
                    {
                        Id = project.Id,
                        Name = project.Name,
                        Description = project.Description,
                        StartDate = project.StartDate,
                        EndDate = project.EndDate,
                        Users = new List<UserDto>(),
                        TotalHourSpent = project.TotalHourSpent
                    };
                    projectMap[proj.Id] = proj;
                }

                if (user != null && user.Id != 0 && !proj.Users.Any(u => u.Id == user.Id))
                {
                    proj.Users.Add(user);
                }

                return proj;
            },
            new { Offset = (page - 1) * pageSize, PageSize = pageSize },
            splitOn: "Id"
        );

        return new PagedResult<ProjectDto>
        {
            Data = projectMap.Values.ToList(),
            Total = totalCount
        };
    }



    public async Task<PagedResult<ProjectDto>> GetPagedAsyncFilter(int page, int pageSize, string? name, string? startDate, string? endDate)
    {
        using var conn = _dbFactory.CreateConnection();

        var filters = new List<string> { "p.IsActive = 1" };

        if (!string.IsNullOrWhiteSpace(name))
            filters.Add("p.Name LIKE @Name");

        if (!string.IsNullOrWhiteSpace(startDate))
            filters.Add("p.StartDate >= @StartDate");

        if (!string.IsNullOrWhiteSpace(endDate))
            filters.Add("p.EndDate <= @EndDate");

        var whereClause = filters.Count > 0 ? $"WHERE {string.Join(" AND ", filters)}" : "";

        var countSql = $"SELECT COUNT(*) FROM Projects p {whereClause}";
        var totalCount = await conn.ExecuteScalarAsync<int>(countSql, new { Name = $"%{name}%", StartDate = startDate, EndDate = endDate });

        var sql = $@"
    SELECT 
        p.Id, p.Name, p.Description, p.StartDate, p.EndDate, p.TotalHourSpent,
        u.Id, u.FullName, u.EmpId, u.Email, u.PhoneNumber, u.Department, u.Role
    FROM Projects p
    LEFT JOIN ProjectUsers pu ON pu.ProjectId = p.Id
    LEFT JOIN Users u ON u.Id = pu.UserId
    {whereClause}
    ORDER BY p.Id
    OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

        var projectMap = new Dictionary<int, ProjectDto>();

        var result = await conn.QueryAsync<ProjectDto, UserDto, ProjectDto>(
            sql,
            (project, user) =>
            {
                if (!projectMap.TryGetValue(project.Id, out var proj))
                {
                    proj = new ProjectDto
                    {
                        Id = project.Id,
                        Name = project.Name,
                        Description = project.Description,
                        StartDate = project.StartDate,
                        EndDate = project.EndDate,
                        Users = new List<UserDto>(),
                        TotalHourSpent = project.TotalHourSpent
                    };
                    projectMap[proj.Id] = proj;
                }

                if (user != null && user.Id != 0 && !proj.Users.Any(u => u.Id == user.Id))
                {
                    proj.Users.Add(user);
                }

                return proj;
            },
            new
            {
                Offset = (page - 1) * pageSize,
                PageSize = pageSize,
                Name = $"%{name}%",
                StartDate = startDate,
                EndDate = endDate
            },
            splitOn: "Id"
        );

        return new PagedResult<ProjectDto>
        {
            Data = projectMap.Values.ToList(),
            Total = totalCount
        };
    }


    public async Task<ProjectDto?> GetByIdAsync(int id)
    {
        using var conn = _dbFactory.CreateConnection();

        var sql = @"
                      SELECT 
                          p.Id AS ProjectId,
                          p.Name,
                          p.Description,
                          p.StartDate,
                          p.EndDate,
                          p.TotalHourSpent,
                          u.Id,
                          u.FullName,
                          u.EmpId,
                          u.Email,
                          u.PhoneNumber,
                          u.Department,
                          u.Role
                      FROM Projects p
                      LEFT JOIN ProjectUsers pu ON pu.ProjectId = p.Id
                      LEFT JOIN Users u ON u.Id = pu.UserId
                      WHERE p.IsActive = 1 AND p.Id = @Id";

        ProjectDto? project = null;

        var result = await conn.QueryAsync<ProjectDto, UserDto, ProjectDto>(
            sql,
            (p, user) =>
            {
                if (project == null)
                {
                    project = new ProjectDto
                    {
                        Id = p.Id,
                        Name = p.Name,
                        Description = p.Description,
                        StartDate = p.StartDate,
                        EndDate = p.EndDate,
                        Users = new List<UserDto>(),
                        TotalHourSpent= p.TotalHourSpent
                    };
                }

                if (user != null && !project.Users.Any(u => u.Id == user.Id))
                {
                    project.Users.Add(user);
                }

                return project;
            },
            new { Id = id },
            splitOn: "Id"
        );

        return project;
    }

    public async Task<List<ProjectDto>> GetFilteredProjectsForExportAsync(string? name, string? startDate, string? endDate)
    {
        using var conn = _dbFactory.CreateConnection();

        var filters = new List<string> { "p.IsActive = 1" };
        if (!string.IsNullOrWhiteSpace(name))
            filters.Add("p.Name LIKE @Name");
        if (!string.IsNullOrWhiteSpace(startDate))
            filters.Add("p.StartDate >= @StartDate");
        if (!string.IsNullOrWhiteSpace(endDate))
            filters.Add("p.EndDate <= @EndDate");

        var whereClause = filters.Count > 0 ? $"WHERE {string.Join(" AND ", filters)}" : "";

        var sql = $@"
        SELECT 
            p.Id, p.Name, p.Description, p.StartDate, p.EndDate, p.TotalHourSpent,
            u.Id, u.FullName, u.EmpId, u.Email, u.PhoneNumber, u.Department, u.Role
        FROM Projects p
        LEFT JOIN ProjectUsers pu ON pu.ProjectId = p.Id
        LEFT JOIN Users u ON u.Id = pu.UserId
        {whereClause}
        ORDER BY p.Id";

        var projectMap = new Dictionary<int, ProjectDto>();

        var result = await conn.QueryAsync<ProjectDto, UserDto, ProjectDto>(
            sql,
            (project, user) =>
            {
                if (!projectMap.TryGetValue(project.Id, out var proj))
                {
                    proj = new ProjectDto
                    {
                        Id = project.Id,
                        Name = project.Name,
                        Description = project.Description,
                        StartDate = project.StartDate,
                        EndDate = project.EndDate,
                        Users = new List<UserDto>(),
                        TotalHourSpent = project.TotalHourSpent
                    };
                    projectMap[proj.Id] = proj;
                }

                if (user != null && user.Id != 0 && !proj.Users.Any(u => u.Id == user.Id))
                {
                    proj.Users.Add(user);
                }

                return proj;
            },
            new { Name = $"%{name}%", StartDate = startDate, EndDate = endDate },
            splitOn: "Id"
        );

        return projectMap.Values.ToList();
    }

    public async Task<ProjectDto> CreateAsync(CreateProjectDto dto, string createdBy)
    {
        using var conn = _dbFactory.CreateConnection();
        conn.Open();
        using var tran = conn.BeginTransaction();

        try
        {
            var insertProjectSql = @"
            INSERT INTO Projects 
            (Name, Description, StartDate, EndDate, CreatedDate, CreatedBy, IsActive) 
            VALUES 
            (@Name, @Description, @StartDate, @EndDate, @CreatedDate, @CreatedBy, @IsActive);
            SELECT CAST(SCOPE_IDENTITY() as int);";

            // 1. Insert project and get back the generated Id
            var projectId = await conn.ExecuteScalarAsync<int>(insertProjectSql, new
            {
                dto.Name,
                dto.Description,
                dto.StartDate,
                dto.EndDate,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = createdBy,
                IsActive = true
            }, tran);

            // 2. Insert users into ProjectUsers using projectId
            if (dto.UserId != null && dto.UserId.Count > 0)
            {
                var insertProjectUserSql = "INSERT INTO ProjectUsers (ProjectId, UserId) VALUES (@ProjectId, @UserId)";
                foreach (var userId in dto.UserId)
                {
                    await conn.ExecuteAsync(insertProjectUserSql, new { ProjectId = projectId, UserId = userId }, tran);
                }
            }

            tran.Commit();

            // 3. Return the created project
            return new ProjectDto
            {
                Id = projectId,
                Name = dto.Name,
                Description = dto.Description,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate
            };
        }
        catch
        {
            tran.Rollback();
            throw;
        }
    }


    public async Task<bool> UpdateAsync(int id, UpdateProjectDto dto, string updatedBy)
    {
        using var conn = _dbFactory.CreateConnection();
        conn.Open();
        using var transaction = conn.BeginTransaction();

        try
        {
            // 1. Update project basic info
            var updateSql = @"
            UPDATE Projects 
            SET Name = @Name, Description = @Description, StartDate = @StartDate, EndDate = @EndDate, 
                UpdatedDate = @UpdatedDate, UpdatedBy = @UpdatedBy 
            WHERE Id = @Id AND IsActive = 1";

            var result = await conn.ExecuteAsync(updateSql, new
            {
                Id = id,
                dto.Name,
                dto.Description,
                dto.StartDate,
                dto.EndDate,
                UpdatedDate = DateTime.UtcNow,
                UpdatedBy = updatedBy
            }, transaction);

            // 2. Remove existing user mappings
            var deleteSql = "DELETE FROM ProjectUsers WHERE ProjectId = @ProjectId";
            await conn.ExecuteAsync(deleteSql, new { ProjectId = id }, transaction);

            // 3. Re-insert new user mappings
            var insertSql = "INSERT INTO ProjectUsers (ProjectId, UserId) VALUES (@ProjectId, @UserId)";

            foreach (var userId in dto.UserId.Distinct())
            {
                await conn.ExecuteAsync(insertSql, new { ProjectId = id, UserId = userId }, transaction);
            }

            transaction.Commit();
            return result > 0;
        }
        catch (Exception ex)
        {
            transaction.Rollback();
            Console.Error.WriteLine($"Error updating project: {ex.Message}");
            throw;
        }
    }


    public async Task<bool> DeleteAsync(int id)
    {
        using var conn = _dbFactory.CreateConnection();
        var sql = "UPDATE Projects SET IsActive = 0 WHERE Id = @Id";
        var result = await conn.ExecuteAsync(sql, new { Id = id });
        return result > 0;
    }
}
