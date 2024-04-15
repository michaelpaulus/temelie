using System.Data;
using System.Data.Common;
using Temelie.Database.Extensions;
using Temelie.Database.Models;
using Temelie.Database.Services;
using Temelie.DependencyInjection;

namespace Temelie.Database.Providers;
[ExportTransient(typeof(IDatabaseFactory))]
public class DatabaseFactory : IDatabaseFactory
{

    public DatabaseFactory(IEnumerable<IDatabaseProvider> databaseProviders, IEnumerable<IConnectionCreatedNotification> connectionCreatedNotifications)
    {
        DatabaseProviders = databaseProviders;
        ConnectionCreatedNotifications = connectionCreatedNotifications;
    }

    public IEnumerable<IDatabaseProvider> DatabaseProviders { get; }
    public IEnumerable<IConnectionCreatedNotification> ConnectionCreatedNotifications { get; }

    public IDatabaseProvider GetDatabaseProvider(DbConnection connection)
    {
        return DatabaseProviders.FirstOrDefault(i => i.SupportsConnection(connection));
    }

    public IDatabaseProvider GetDatabaseProvider(ConnectionStringModel connectionString)
    {
        return DatabaseProviders.FirstOrDefault(i => i.Name.EqualsIgnoreCase(connectionString.DatabaseProviderName));
    }

    public IDatabaseProvider GetDatabaseProvider(string name)
    {
        return DatabaseProviders.FirstOrDefault(i => i.Name.EqualsIgnoreCase(name));
    }

    public void NotifyConnections(IDbConnection connection)
    {
        foreach (var notification in ConnectionCreatedNotifications)
        {
            notification.Notify(connection);
        }
    }

}
