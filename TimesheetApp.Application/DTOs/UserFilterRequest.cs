using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TimesheetApp.Application.DTOs
{
    public class UserFilterRequest
    {
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;

        public Guid? ProjectId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }

}
