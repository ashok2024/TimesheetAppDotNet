using System.Data;

namespace TimesheetApp.Infrastructure.Data
{
    public interface IDbConnectionFactory
    {
        IDbConnection CreateConnection();
    }
}