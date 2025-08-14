using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TimesheetApp.Application.DTOs;

namespace TimesheetApp.Application.Interfaces
{
    public interface ITaskService
    {
        Task<IEnumerable<TaskItemDto>> GetAllTasksAsync();
        Task<TaskItemDto?> GetTaskByIdAsync(int id);
        Task<TaskItemDto> CreateTaskAsync(CreateTaskItemDto dto, string createdBy);
        Task<bool> UpdateTaskAsync(int id, UpdateTaskItemDto dto, string updatedBy);
        Task<bool> DeleteTaskAsync(int id);
        Task<PagedResult<TaskItemDto>> GetTasksByProjectIdAsync(int projectId, int page, int pageSize);
        Task<List<TaskItemDto>> GetTasksByProjectIdAsync(int projectId);

    }

}
