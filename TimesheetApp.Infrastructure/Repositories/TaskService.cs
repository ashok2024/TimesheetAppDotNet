using Dapper;
using TimesheetApp.Application.DTOs;
using TimesheetApp.Application.Interfaces;
using TimesheetApp.Domain.Entities;
using TimesheetApp.Infrastructure.Data;

public class TaskService : ITaskService
{
    private readonly IDbConnectionFactory _dbFactory;

    public TaskService(IDbConnectionFactory dbFactory)
    {
        _dbFactory = dbFactory;
    }

    public async Task<IEnumerable<TaskItemDto>> GetAllTasksAsync()
    {
        using var conn = _dbFactory.CreateConnection();

        var sql = @"
    SELECT 
        t.Id AS TaskId, t.Name, t.Description, t.DueDate, t.Status, t.ProjectId,
        p.Id AS ProjectId, p.Name AS ProjectName, p.Description AS ProjectDescription, p.StartDate, p.EndDate,
        u.Id AS UserId, u.EmpId, u.UserName, u.Email, u.FullName, u.PhoneNumber, u.Department, u.Role, u.DateOfJoining, u.IsActive
    FROM TaskItems t
    INNER JOIN Projects p ON t.ProjectId = p.Id
    LEFT JOIN TaskUserAssignments tua ON tua.TaskId = t.Id
    LEFT JOIN Users u ON tua.UserId = u.Id
    WHERE t.IsActive = 1;
    ";

        var taskDict = new Dictionary<int, TaskItemDto>();

        var result = await conn.QueryAsync<TaskItemDto, ProjectDto, UserDto, TaskItemDto>(
            sql,
            (task, project, user) =>
            {
                if (!taskDict.TryGetValue(task.Id, out var existingTask))
                {
                    task.Project = project;
                    task.AssignedUserIds = new List<UserDto>();
                    taskDict[task.Id] = task;
                    existingTask = task;
                }

                if (user != null && user.Id != 0 && !existingTask.AssignedUserIds.Any(u => u.Id == user.Id))
                {
                    existingTask.AssignedUserIds.Add(user);
                }

                return existingTask;
            },
            splitOn: "ProjectId,UserId"
        );

        return taskDict.Values;
    }

    public async Task<PagedResult<TaskItemDto>> GetTasksByProjectIdAsync(int projectId, int page, int pageSize)
    {
        using var conn = _dbFactory.CreateConnection();

        var offset = (page - 1) * pageSize;

        var sql = @"
    -- Main paginated query
    SELECT 
        t.Id, t.Name, t.Description, t.DueDate, t.Status, t.ProjectId, t.FilePath,
        p.Id AS ProjectId, p.Name AS Name, p.Description AS Description, p.StartDate, p.EndDate,
        u.Id AS Id, u.EmpId, u.UserName, u.Email, u.FullName, u.PhoneNumber, u.Department, u.Role, u.DateOfJoining, u.IsActive,
        ISNULL(th.TotalHoursSpent, 0) AS TotalHoursSpent
    FROM Tasks t
    INNER JOIN Projects p ON t.ProjectId = p.Id
    LEFT JOIN TaskUserAssignments tua ON tua.TaskId = t.Id
    LEFT JOIN Users u ON u.Id = tua.UserId
    LEFT JOIN (
        SELECT TaskId, SUM(HoursWorked) AS TotalHoursSpent
        FROM Timesheets
        GROUP BY TaskId
    ) th ON th.TaskId = t.Id
    WHERE t.ProjectId = @ProjectId AND t.IsActive = 1
    ORDER BY t.Id
    OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;

    -- Total count query
    SELECT COUNT(*) 
    FROM Tasks 
    WHERE ProjectId = @ProjectId AND IsActive = 1;
    ";

        var taskMap = new Dictionary<int, TaskItemDto>();

        using var multi = await conn.QueryMultipleAsync(sql, new { ProjectId = projectId, Offset = offset, PageSize = pageSize });

        // Use non-async .Read with type arguments for multi-mapping
        var taskRows = multi.Read<TaskItemDto, ProjectDto, UserDto, TaskItemDto>(
            (task, project, user) =>
            {
                if (!taskMap.TryGetValue(task.Id, out var existingTask))
                {
                    task.Project = project;
                    task.AssignedUserIds = new List<UserDto>();
                    taskMap[task.Id] = task;
                }

                if (user != null && user.Id != 0 && !taskMap[task.Id].AssignedUserIds.Any(u => u.Id == user.Id))
                {
                    taskMap[task.Id].AssignedUserIds.Add(user);
                }

                return taskMap[task.Id];
            },
            splitOn: "ProjectId,Id"
        );

        var totalCount = await multi.ReadSingleAsync<int>();

        return new PagedResult<TaskItemDto>
        {
            Data = taskMap.Values.ToList(),
            Total = totalCount
        };
    }


    public async Task<TaskItemDto?> GetTaskByIdAsync(int id)
    {
        using var conn = _dbFactory.CreateConnection();

        var sql = @"
        SELECT 
            t.Id, t.Name, t.Description, t.DueDate, t.Status, t.ProjectId, t.FilePath,
            ISNULL(th.TotalHoursSpent, 0) AS TotalHoursSpent,

            p.Id AS ProjectId, p.Name AS ProjectName, p.Description AS ProjectDescription, p.StartDate, p.EndDate,

            u.Id AS UserId, u.EmpId, u.UserName, u.Email, u.PasswordHash, u.FullName, 
            u.PhoneNumber, u.Department, u.Role, u.DateOfJoining

        FROM Tasks t
        INNER JOIN Projects p ON t.ProjectId = p.Id
        LEFT JOIN TaskUserAssignments tua ON tua.TaskId = t.Id
        LEFT JOIN Users u ON tua.UserId = u.Id

        LEFT JOIN (
            SELECT TaskId, SUM(HoursWorked) AS TotalHoursSpent
            FROM Timesheets
            GROUP BY TaskId
        ) th ON th.TaskId = t.Id

        WHERE t.Id = @Id AND t.IsActive = 1;
    ";

        TaskItemDto? taskResult = null;
        var taskMap = new Dictionary<int, TaskItemDto>();

        await conn.QueryAsync<TaskItemDto, ProjectDto, UserDto, TaskItemDto>(
            sql,
            (task, project, user) =>
            {
                if (!taskMap.TryGetValue(task.Id, out var existingTask))
                {
                    task.Project = project;
                    task.AssignedUserIds = new List<UserDto>();
                    taskMap[task.Id] = task;
                    taskResult = task;
                    existingTask = task;
                }

                if (user != null && user.Id != 0 && !existingTask.AssignedUserIds.Any(u => u.Id == user.Id))
                {
                    // Note: Map UserId from SQL to Id in DTO
                    user.Id = user.Id;
                    existingTask.AssignedUserIds.Add(user);
                }

                return existingTask;
            },
            new { Id = id },
            splitOn: "ProjectId,UserId"
        );

        return taskResult;
    }




    public async Task<TaskItemDto> CreateTaskAsync(CreateTaskItemDto dto, string createdBy)
    {
        using var conn = _dbFactory.CreateConnection();
        conn.Open();
        using var transaction = conn.BeginTransaction();

        try
        {
            string? savedFilePath = null;

            if (dto.File != null && dto.File.Length > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "tasks");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var uniqueFileName = $"{Guid.NewGuid()}_{dto.File.FileName}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await dto.File.CopyToAsync(stream);
                }

                savedFilePath = $"uploads/tasks/{uniqueFileName}";
            }

            var task = new TaskItem
            {
                Name = dto.Name,
                Description = dto.Description,
                DueDate = dto.DueDate,
                Status = dto.Status,
                ProjectId = dto.ProjectId,
                FilePath = savedFilePath, // ✅ Set file path
                CreatedBy = createdBy,
                CreatedDate = DateTime.UtcNow,
                IsActive = true
            };

            var insertTaskSql = @"
        INSERT INTO Tasks (Name, Description, DueDate, Status, ProjectId, CreatedDate, CreatedBy, IsActive, FilePath)
        VALUES (@Name, @Description, @DueDate, @Status, @ProjectId, @CreatedDate, @CreatedBy, @IsActive, @FilePath);
        SELECT CAST(SCOPE_IDENTITY() as int);";

            task.Id = await conn.QuerySingleAsync<int>(insertTaskSql, task, transaction);

            var insertAssignmentSql = @"
        INSERT INTO TaskUserAssignments (TaskId, UserId)
        VALUES (@TaskId, @UserId);";

            foreach (var userId in dto.UserIds)
            {
                await conn.ExecuteAsync(insertAssignmentSql, new { TaskId = task.Id, UserId = userId }, transaction);
            }

            transaction.Commit();

            return new TaskItemDto
            {
                Id = task.Id,
                Name = task.Name,
                Description = task.Description,
                DueDate = task.DueDate,
                Status = task.Status,
                ProjectId = task.ProjectId,
                FilePath = task.FilePath
            };
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }


    public async Task<bool> UpdateTaskAsync(int id, UpdateTaskItemDto dto, string updatedBy)
    {
        using var conn = _dbFactory.CreateConnection();
        conn.Open();
        using var transaction = conn.BeginTransaction();

        try
        {
            string? savedFilePath = null;

            if (dto.File != null && dto.File.Length > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "tasks");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var uniqueFileName = $"{Guid.NewGuid()}_{dto.File.FileName}";
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await dto.File.CopyToAsync(stream);
                }

                savedFilePath = $"uploads/tasks/{uniqueFileName}";
            }

            var updateTaskSql = @"
        UPDATE Tasks SET 
            Name = @Name, 
            Description = @Description, 
            DueDate = @DueDate, 
            Status = @Status, 
            ProjectId = @ProjectId,
            UpdatedDate = @UpdatedDate,
            UpdatedBy = @UpdatedBy,
            FilePath = COALESCE(@FilePath, FilePath)
        WHERE Id = @Id;";

            var affected = await conn.ExecuteAsync(updateTaskSql, new
            {
                Id = id,
                dto.Name,
                dto.Description,
                dto.DueDate,
                dto.Status,
                dto.ProjectId,
                FilePath = savedFilePath,
                UpdatedDate = DateTime.UtcNow,
                UpdatedBy = updatedBy
            }, transaction);

            var deleteAssignmentsSql = "DELETE FROM TaskUserAssignments WHERE TaskId = @TaskId;";
            await conn.ExecuteAsync(deleteAssignmentsSql, new { TaskId = id }, transaction);

            var insertAssignmentSql = @"
        INSERT INTO TaskUserAssignments (TaskId, UserId)
        VALUES (@TaskId, @UserId);";

            foreach (var userId in dto.UserIds)
            {
                await conn.ExecuteAsync(insertAssignmentSql, new { TaskId = id, UserId = userId }, transaction);
            }

            transaction.Commit();
            return affected > 0;
        }
        catch
        {
            transaction.Rollback();
            throw;
        }
    }



    public async Task<bool> DeleteTaskAsync(int id)
    {
        using var conn = _dbFactory.CreateConnection();
        var sql = "UPDATE Tasks SET IsActive = 0 WHERE Id = @Id";
        var affected = await conn.ExecuteAsync(sql, new { Id = id });
        return affected > 0;
    }
    



}
