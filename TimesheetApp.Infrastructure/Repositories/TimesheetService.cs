using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TimesheetApp.Application.DTOs.TimesheetApp.Application.DTOs.TimesheetTask;
using TimesheetApp.Application.Interfaces;
using TimesheetApp.Domain.Entities;
using TimesheetApp.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;

namespace TimesheetApp.Infrastructure.Repositories
{
    public class TimesheetService : ITimesheetService
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public TimesheetService(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        public async Task AddTaskAsync(CreateTimesheetTaskRequest request, IFormFile? attachment, int userId)
        {
            string? attachmentPath = null;

            if (attachment != null && attachment.Length > 0)
            {
                var uploadsFolder = Path.Combine(_env.WebRootPath ?? "wwwroot", "uploads");
                Directory.CreateDirectory(uploadsFolder);

                var fileName = $"{Guid.NewGuid()}_{Path.GetFileName(attachment.FileName)}";
                var fullPath = Path.Combine(uploadsFolder, fileName);

                using var stream = new FileStream(fullPath, FileMode.Create);
                await attachment.CopyToAsync(stream);

                attachmentPath = $"/uploads/{fileName}";
            }

            var task = new Timesheet
            {
                UserId = userId,
                ProjectName = request.ProjectName,
                TaskDescription = request.TaskDescription,
                HoursSpent = request.HoursSpent,
                TaskDate = request.TaskDate,
                Notes = request.Notes,
                AttachmentPath = attachmentPath
            };

            await _context.Timesheets.AddAsync(task);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<TimesheetTaskDto>> GetTasksByUserAsync(int userId)
        {
            return await _context.Timesheets
                .Where(t => t.UserId == userId)
                .Select(t => new TimesheetTaskDto
                {
                    Id = t.Id,
                    ProjectName = t.ProjectName,
                    TaskDescription = t.TaskDescription,
                    HoursSpent = t.HoursSpent,
                    TaskDate = t.TaskDate,
                    Notes = t.Notes,
                    AttachmentUrl = t.AttachmentPath
                })
                .ToListAsync();
        }

        public async Task<bool> DeleteTaskAsync(int id, int userId)
        {
            var task = await _context.Timesheets.FirstOrDefaultAsync(t => t.Id == id && t.UserId == userId);
            if (task == null) return false;

            _context.Timesheets.Remove(task);
            await _context.SaveChangesAsync();
            return true;
        }
    }

}
