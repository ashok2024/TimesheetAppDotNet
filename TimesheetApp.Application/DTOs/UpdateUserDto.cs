using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimesheetApp.Application.DTOs
{
    public class UpdateUserDto
    {
        public string FullName { get; set; }
        public string Role { get; set; }
        public string EmpId { get; set; }
        public string PhoneNumber { get; set; }
        public string Department { get; set; }
        public DateTime DateOfJoining { get; set; }
    }
}
