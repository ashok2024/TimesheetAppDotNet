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
        Task<IEnumerable<TimesheetDto>> GetAllAsync();
        Task<TimesheetDto?> GetByIdAsync(int id);
        Task<TimesheetDto> CreateAsync(CreateTimesheetDto dto, string createdBy);
        Task<bool> UpdateAsync(int id, UpdateTimesheetDto dto, string updatedBy);
        Task<bool> DeleteAsync(int id);
        Task<IEnumerable<TimesheetDto>> GetByFilterAsync(int? projectId, int? taskId, int? userId);
        Task<IEnumerable<TimesheetDto>> GetByTaskAndUserAsync(int taskId);

    }
}
