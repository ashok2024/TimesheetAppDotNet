using TimesheetApp.Domain.Common;

namespace TimesheetApp.Domain.Entities
{
    public class User : AuditableEntity
    {
        public int Id { get; set; }
        public string EmpId { get; set; }
        public string UserName { get; set; } = default!;
        public string Email { get; set; } = default!;
        public string PasswordHash { get; set; } = default!;
        public string? FullName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Department { get; set; }
        public string? Role { get; set; }
        public DateTime? DateOfJoining { get; set; }
    }
}
