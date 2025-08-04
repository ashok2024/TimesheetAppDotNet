using Dapper;
using System.Text;
using TimesheetApp.Application.DTOs.TimesheetApp.Application.DTOs.TimesheetTask;
using TimesheetApp.Application.Interfaces;
using TimesheetApp.Domain.Entities;
using TimesheetApp.Infrastructure.Data;

public class TimesheetService : ITimesheetService
{
    private readonly IDbConnectionFactory _dbFactory;

    public TimesheetService(IDbConnectionFactory dbFactory)
    {
        _dbFactory = dbFactory;
    }

    public async Task<IEnumerable<TimesheetDto>> GetAllAsync()
    {
        using var conn = _dbFactory.CreateConnection();
        var sql = "SELECT * FROM Timesheets WHERE IsActive = 1";
        return await conn.QueryAsync<TimesheetDto>(sql);
    }

    public async Task<TimesheetDto?> GetByIdAsync(int id)
    {
        using var conn = _dbFactory.CreateConnection();
        var sql = "SELECT * FROM Timesheets WHERE Id = @Id AND IsActive = 1";
        return await conn.QueryFirstOrDefaultAsync<TimesheetDto>(sql, new { Id = id });
    }

    public async Task<IEnumerable<TimesheetDto>> GetByFilterAsync(int? projectId, int? taskId, int? userId)
    {
        using var conn = _dbFactory.CreateConnection();

        var sql = new StringBuilder("SELECT * FROM Timesheets WHERE IsActive = 1");
        var parameters = new DynamicParameters();

        if (projectId.HasValue)
        {
            sql.Append(" AND ProjectId = @ProjectId");
            parameters.Add("ProjectId", projectId.Value);
        }

        if (taskId.HasValue)
        {
            sql.Append(" AND TaskId = @TaskId");
            parameters.Add("TaskId", taskId.Value);
        }

        if (userId.HasValue)
        {
            sql.Append(" AND UserId = @UserId");
            parameters.Add("UserId", userId.Value);
        }

        return await conn.QueryAsync<TimesheetDto>(sql.ToString(), parameters);
    }

    public async Task<TimesheetDto> CreateAsync(CreateTimesheetDto dto, string createdBy)
    {
        using var conn = _dbFactory.CreateConnection();
        conn.Open();
        using var transaction = conn.BeginTransaction();

        try
        {
            var entity = new Timesheet
            {
                UserId = dto.UserId,
                ProjectId = dto.ProjectId,
                TaskId = dto.TaskId,
                WorkDate = dto.WorkDate,
                HoursWorked = dto.HoursWorked,
                Description = dto.Description,
                CreatedDate = DateTime.UtcNow,
                CreatedBy = createdBy,
                IsActive = true
            };

            // 1. Insert timesheet
            var insertSql = @"
            INSERT INTO Timesheets 
            (UserId, ProjectId, TaskId, WorkDate, HoursWorked, Description, CreatedDate, CreatedBy, IsActive)
            VALUES 
            (@UserId, @ProjectId, @TaskId, @WorkDate, @HoursWorked, @Description, @CreatedDate, @CreatedBy, @IsActive)";

            await conn.ExecuteAsync(insertSql, entity, transaction);

            // 2. Calculate total hours spent for this project
            var sumSql = @"
            SELECT ISNULL(SUM(HoursWorked), 0)
            FROM Timesheets
            WHERE ProjectId = @ProjectId AND IsActive = 1";

            var totalHours = await conn.ExecuteScalarAsync<decimal>(sumSql, new { ProjectId = dto.ProjectId }, transaction);

            // 3. Update the Project with the new total hours
            var updateSql = @"
            UPDATE Projects
            SET TotalHourSpent = @TotalHoursSpent
            WHERE Id = @ProjectId";

            await conn.ExecuteAsync(updateSql, new { TotalHoursSpent = totalHours, ProjectId = dto.ProjectId }, transaction);

            transaction.Commit();

            // 4. Return the timesheet DTO
            return new TimesheetDto
            {
                Id = entity.Id,
                UserId = entity.UserId,
                ProjectId = entity.ProjectId,
                TaskId = entity.TaskId,
                WorkDate = entity.WorkDate,
                HoursWorked = entity.HoursWorked,
                Description = entity.Description
            };
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }


    public async Task<bool> UpdateAsync(int id, UpdateTimesheetDto dto, string updatedBy)
    {
        using var conn = _dbFactory.CreateConnection();
        conn.Open();
        using var transaction = conn.BeginTransaction();

        try
        {
            // 1. Update timesheet
            var updateSql = @"UPDATE Timesheets SET 
                            UserId = @UserId,
                            ProjectId = @ProjectId,
                            TaskId = @TaskId,
                            WorkDate = @WorkDate,
                            HoursWorked = @HoursWorked,
                            Description = @Description,
                            UpdatedDate = @UpdatedDate,
                            UpdatedBy = @UpdatedBy
                          WHERE Id = @Id AND IsActive = 1";

            var affected = await conn.ExecuteAsync(updateSql, new
            {
                Id = id,
                dto.UserId,
                dto.ProjectId,
                dto.TaskId,
                dto.WorkDate,
                dto.HoursWorked,
                dto.Description,
                UpdatedDate = DateTime.UtcNow,
                UpdatedBy = updatedBy
            }, transaction);

            if (affected == 0)
            {
                transaction.Rollback();
                return false;
            }

            // 2. Recalculate total hours for the project
            var sumSql = @"SELECT ISNULL(SUM(HoursWorked), 0)
                       FROM Timesheets
                       WHERE ProjectId = @ProjectId AND IsActive = 1";

            var totalHours = await conn.ExecuteScalarAsync<decimal>(sumSql, new { ProjectId = dto.ProjectId }, transaction);

            // 3. Update the project with new total hours
            var updateProjectSql = @"UPDATE Projects
                                 SET TotalHourSpent = @TotalHoursSpent
                                 WHERE Id = @ProjectId";

            await conn.ExecuteAsync(updateProjectSql, new
            {
                TotalHoursSpent = totalHours,
                ProjectId = dto.ProjectId
            }, transaction);

            transaction.Commit();
            return true;
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }


    public async Task<bool> DeleteAsync(int id)
    {
        using var conn = _dbFactory.CreateConnection();
        var sql = "UPDATE Timesheets SET IsActive = 0 WHERE Id = @Id";
        var affected = await conn.ExecuteAsync(sql, new { Id = id });
        return affected > 0;
    }
    public async Task<IEnumerable<TimesheetDto>> GetByTaskAndUserAsync(int taskId)
    {
        using var conn = _dbFactory.CreateConnection();
        var sql = @"
        SELECT * FROM Timesheets 
        WHERE TaskId = @TaskId  AND IsActive = 1";

        return await conn.QueryAsync<TimesheetDto>(sql, new { TaskId = taskId });
    }

}
