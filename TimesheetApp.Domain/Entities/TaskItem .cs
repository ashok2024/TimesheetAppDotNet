using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TimesheetApp.Domain.Common;

namespace TimesheetApp.Domain.Entities
{
    public class TaskItem : AuditableEntity
    {
        public int Id { get; set; }
        public int ProjectId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public Guid? AssignedTo { get; set; }
        public DateTime? DueDate { get; set; }
        public string? Status { get; set; }
        public string? FilePath { get; set; }
    }
}
