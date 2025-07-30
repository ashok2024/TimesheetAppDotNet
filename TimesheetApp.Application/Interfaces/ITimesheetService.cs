using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TimesheetApp.Application.DTOs.TimesheetApp.Application.DTOs.TimesheetTask;

namespace TimesheetApp.Application.Interfaces
{
    public interface ITimesheetService
    {
        Task AddTaskAsync(CreateTimesheetTaskRequest request, IFormFile? attachment, int userId);
        Task<IEnumerable<TimesheetTaskDto>> GetTasksByUserAsync(int userId);
        Task<bool> DeleteTaskAsync(int id, int userId);
    }
}
