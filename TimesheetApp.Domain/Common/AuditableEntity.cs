namespace TimesheetApp.Domain.Common
{
    public abstract class AuditableEntity
    {
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public string? CreatedBy { get; set; }

        public DateTime? UpdatedDate { get; set; }
        public string? UpdatedBy { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
