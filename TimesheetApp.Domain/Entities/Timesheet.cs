using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimesheetApp.Domain.Entities
{
    public class Timesheet
    {
        public int Id { get; set; }

        public int UserId { get; set; }

        public string ProjectName { get; set; } = string.Empty;

        public string TaskDescription { get; set; } = string.Empty;

        public double HoursSpent { get; set; }

        public DateTime TaskDate { get; set; }

        public string? Notes { get; set; }

        public string? AttachmentPath { get; set; }  // for uploaded file
    }
}
