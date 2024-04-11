using System.Data;
using System.Data.Common;
using Cornerstone.Database.Models;

namespace Cornerstone.Database.Providers;
public interface IDatabaseFactory
{
    IDatabaseProvider GetDatabaseProvider(DbConnection connection);
    IDatabaseProvider GetDatabaseProvider(ConnectionStringModel connectionString);
    void NotifyConnections(IDbConnection connection);
}
