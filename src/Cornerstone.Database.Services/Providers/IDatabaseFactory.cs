using System.Configuration;
using System.Data;
using System.Data.Common;
using Cornerstone.Database.Models;

namespace Cornerstone.Database.Providers;
public interface IDatabaseFactory
{
    IDatabaseProvider GetDatabaseProvider(DatabaseType databaseType, bool throwOnNotFound = false);
    DatabaseType GetDatabaseType(ConnectionStringSettings connectionString);
    DatabaseType GetDatabaseType(DbConnection connection);
    string GetProviderName(ConnectionStringSettings connectionString);
    string GetProviderName(DbConnection connection);
    void NotifyConnections(IDbConnection connection);
}
