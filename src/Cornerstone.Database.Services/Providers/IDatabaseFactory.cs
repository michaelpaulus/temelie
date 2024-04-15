using System.Data;
using System.Data.Common;
using Temelie.Database.Models;

namespace Temelie.Database.Providers;
public interface IDatabaseFactory
{
    IDatabaseProvider GetDatabaseProvider(string name);
    IDatabaseProvider GetDatabaseProvider(DbConnection connection);
    IDatabaseProvider GetDatabaseProvider(ConnectionStringModel connectionString);
    void NotifyConnections(IDbConnection connection);
}
