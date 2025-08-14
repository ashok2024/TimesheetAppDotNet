using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimesheetApp.Application.DTOs
{
    public class TimesheetResponse
    {
        public int TimesheetId { get; set; }
        public int ProjectId { get; set; }
        public string ProjectName { get; set; }
        public int TaskId { get; set; }
        public string TaskName { get; set; }
        public int UserId { get; set; }
        public decimal Hours { get; set; }
        public DateTime LogDate { get; set; }
        public string Description { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
