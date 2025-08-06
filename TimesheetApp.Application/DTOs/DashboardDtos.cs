using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimesheetApp.Application.DTOs
{
    public class ProjectHoursDto
    {
        public string ProjectName { get; set; }
        public double TotalHours { get; set; }
    }

    public class TaskTrendDto
    {
        public DateTime Date { get; set; }
        public int TaskCount { get; set; }
    }

    public class DaySummaryDto
    {
        public string DayOfWeek { get; set; }
        public double TotalHours { get; set; }
    }

    public class WeeklySummaryDto
    {
        public List<DaySummaryDto> Days { get; set; }
    }

    public class TimesheetEntryDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserFullName { get; set; }
        public int ProjectId { get; set; }
        public string ProjectName { get; set; }
        public DateTime EntryDate { get; set; }
        public double HoursWorked { get; set; }
        public string Description { get; set; }
    }

    public class TimesheetFilterDto
    {
        public int? UserId { get; set; }
        public int? ProjectId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }

}
