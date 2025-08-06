using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TimesheetApp.Application.DTOs;

namespace TimesheetApp.Application.Interfaces
{
    public interface IDashboardService
    {
        Task<IEnumerable<ProjectHoursDto>> GetHoursPerProjectAsync(int? limit);
        Task<IEnumerable<TaskTrendDto>> GetTaskTrendsAsync();
        Task<WeeklySummaryDto> GetWeeklySummaryAsync();
        Task<IEnumerable<TimesheetEntryDto>> FilterTimesheetsAsync(TimesheetFilterDto filter);
        Task<byte[]> ExportTimesheetsToCsvAsync(TimesheetFilterDto filter);
    }
}
