using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimesheetApp.Application.DTOs
{
    namespace TimesheetApp.Application.DTOs.TimesheetTask
    {
        public class CreateTimesheetTaskRequest
        {
            public string ProjectName { get; set; } = string.Empty;
            public string TaskDescription { get; set; } = string.Empty;
            public double HoursSpent { get; set; }
            public DateTime TaskDate { get; set; }
            public string? Notes { get; set; }
        }
    }

}
