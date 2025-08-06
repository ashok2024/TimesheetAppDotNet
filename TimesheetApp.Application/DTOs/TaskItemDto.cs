using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TimesheetApp.Domain.Entities;

namespace TimesheetApp.Application.DTOs
{
    public class TaskItemDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime? DueDate { get; set; }
        public string? Status { get; set; }
        public int ProjectId { get; set; }
        public ProjectDto? Project { get; set; }
        public List<UserDto> AssignedUserIds { get; set; } = new();
        public string? FilePath { get; set; }
        public decimal TotalHoursSpent { get; set; }
    }

    public class CreateTaskItemDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime? DueDate { get; set; }
        public string? Status { get; set; }
        public int ProjectId { get; set; }
        public List<int>? UserIds { get; set; }
        public IFormFile? File { get; set; }
    }

    public class UpdateTaskItemDto : CreateTaskItemDto { }
    public class PaginatedResult<T>
    {
        public IEnumerable<T> Data { get; set; } = [];
        public int TotalCount { get; set; }
    }

}
