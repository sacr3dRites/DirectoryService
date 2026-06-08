using System.Data;

namespace DirectoryService.Application.Database;

public interface INpgSqlConnectionFactory
{
    Task<IDbConnection> CreateConnectionAsync();
}