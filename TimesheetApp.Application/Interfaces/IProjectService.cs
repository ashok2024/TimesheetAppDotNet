using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TimesheetApp.Application.DTOs;

namespace TimesheetApp.Application.Interfaces
{
    public interface IProjectService
    {
        Task<PagedResult<ProjectDto>> GetPagedAsync(int page, int pageSize);
        Task<ProjectDto?> GetByIdAsync(int id);
        Task<ProjectDto> CreateAsync(CreateProjectDto dto, string createdBy);
        Task<bool> UpdateAsync(int id, UpdateProjectDto dto, string updatedBy);
        Task<bool> DeleteAsync(int id);
        Task<PagedResult<ProjectDto>> GetPagedAsyncFilter(int page, int pageSize, string? name, string? startDate, string? endDate);
        Task<List<ProjectDto>> GetFilteredProjectsForExportAsync(string? name, string? startDate, string? endDate);
    }

}
