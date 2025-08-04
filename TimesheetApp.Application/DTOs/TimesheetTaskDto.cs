using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimesheetApp.Application.DTOs
{
    namespace TimesheetApp.Application.DTOs.TimesheetTask
    {
        public class TimesheetDto
        {
            public int Id { get; set; }
            public int UserId { get; set; }
            public int ProjectId { get; set; }
            public int? TaskId { get; set; }
            public DateTime WorkDate { get; set; }
            public decimal HoursWorked { get; set; }
            public string? Description { get; set; }
        }

        public class CreateTimesheetDto
        {
            public int UserId { get; set; }
            public int ProjectId { get; set; }
            public int? TaskId { get; set; }
            public DateTime WorkDate { get; set; }
            public decimal HoursWorked { get; set; }
            public string? Description { get; set; }
        }

        public class UpdateTimesheetDto : CreateTimesheetDto { }

    }

}
