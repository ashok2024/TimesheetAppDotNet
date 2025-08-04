using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimesheetApp.Application.DTOs
{
    // CreateUserDto.cs
    public class CreateUserDto
    {
        public string FullName { get; set; }
        public string EmpId { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Department { get; set; }
        public string Role { get; set; } // "Admin" or "User"
        public DateTime DateOfJoining { get; set; }
        public string Password { get; set; }
    }


}
