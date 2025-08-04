using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimesheetApp.Application.DTOs
{
    public class UserDto
    {
        public int Id { get; set; }
        public string EmpId { get; set; }
        public string UserName { get; set; } = default!;
        public string Email { get; set; } = default!;
        public string FullName { get; set; } = default!;
        public string? PhoneNumber { get; set; }
        public string? Department { get; set; }
        public string Role { get; set; } = default!;
        public DateTime? DateOfJoining { get; set; }
        public bool IsActive { get; set; }
    }
}
