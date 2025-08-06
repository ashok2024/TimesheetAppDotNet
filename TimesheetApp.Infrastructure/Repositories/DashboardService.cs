using Dapper;
using System.Text;
using TimesheetApp.Application.DTOs;
using TimesheetApp.Application.Interfaces;
using TimesheetApp.Infrastructure.Data;

public class DashboardService : IDashboardService
{
    private readonly IDbConnectionFactory _dbFactory;

    public DashboardService(IDbConnectionFactory dbFactory)
    {
        _dbFactory = dbFactory;
    }

    public async Task<IEnumerable<ProjectHoursDto>> GetHoursPerProjectAsync(int? limit)
    {
        using var conn = _dbFactory.CreateConnection();
        var sql = @"
        SELECT TOP (@Limit) 
            p.Name AS ProjectName, 
            ISNULL(SUM(t.HoursWorked), 0) AS TotalHours
        FROM Projects p
        LEFT JOIN Timesheets t ON t.ProjectId = p.Id
        GROUP BY p.Name
        ORDER BY TotalHours DESC";

        return await conn.QueryAsync<ProjectHoursDto>(sql, new { Limit = limit ?? int.MaxValue });
    }


    public async Task<IEnumerable<TaskTrendDto>> GetTaskTrendsAsync()
    {
        using var conn = _dbFactory.CreateConnection();
        var sql = @"
            SELECT 
                CAST(t.WorkDate AS DATE) AS Date,
                COUNT(*) AS TaskCount
            FROM Timesheets t
            GROUP BY CAST(t.WorkDate AS DATE)
            ORDER BY Date";

        return await conn.QueryAsync<TaskTrendDto>(sql);
    }

    public async Task<WeeklySummaryDto> GetWeeklySummaryAsync()
    {
        using var conn = _dbFactory.CreateConnection();
        var sql = @"
            SELECT 
                DATENAME(WEEKDAY, t.WorkDate) AS DayOfWeek,
                SUM(t.HoursWorked) AS TotalHours
            FROM Timesheets t
            WHERE t.WorkDate >= DATEADD(DAY, -7, GETDATE())
            GROUP BY DATENAME(WEEKDAY, t.WorkDate)";

        var result = await conn.QueryAsync<DaySummaryDto>(sql);

        return new WeeklySummaryDto
        {
            Days = result.ToList()
        };
    }

    public async Task<IEnumerable<TimesheetEntryDto>> FilterTimesheetsAsync(TimesheetFilterDto filter)
    {
        using var conn = _dbFactory.CreateConnection();
        var sql = @"
            SELECT t.Id, t.UserId, u.FullName, t.ProjectId, p.Name AS ProjectName, 
                   t.EntryDate, t.HoursWorked, t.Description
            FROM Timesheets t
            JOIN Users u ON t.UserId = u.Id
            JOIN Projects p ON t.ProjectId = p.Id
            WHERE (@UserId IS NULL OR t.UserId = @UserId)
              AND (@ProjectId IS NULL OR t.ProjectId = @ProjectId)
              AND (@StartDate IS NULL OR t.EntryDate >= @StartDate)
              AND (@EndDate IS NULL OR t.EntryDate <= @EndDate)
            ORDER BY t.EntryDate DESC";

        return await conn.QueryAsync<TimesheetEntryDto>(sql, filter);
    }

    public async Task<byte[]> ExportTimesheetsToCsvAsync(TimesheetFilterDto filter)
    {
        var timesheets = await FilterTimesheetsAsync(filter);
        var csv = new StringBuilder();
        csv.AppendLine("Date,User,Project,Hours,Description");

        foreach (var ts in timesheets)
        {
            csv.AppendLine($"{ts.EntryDate:yyyy-MM-dd},{ts.UserFullName},{ts.ProjectName},{ts.HoursWorked},{ts.Description}");
        }

        return Encoding.UTF8.GetBytes(csv.ToString());
    }
}
