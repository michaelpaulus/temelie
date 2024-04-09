using System.Data;
using System.Data.Common;

namespace Cornerstone.Database.Providers;
public interface IDatabaseFactory
{
    IDatabaseProvider GetDatabaseProvider(DbConnection connection);
    IDatabaseProvider GetDatabaseProvider(System.Configuration.ConnectionStringSettings connectionString);
    void NotifyConnections(IDbConnection connection);
}
