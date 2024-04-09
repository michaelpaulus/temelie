using System.Data;
using System.Data.Common;
using Cornerstone.Database.Models;
using Cornerstone.Database.Services;

namespace Cornerstone.Database.Providers;
public class DatabaseFactory : IDatabaseFactory
{

    public DatabaseFactory(IEnumerable<IDatabaseProvider> databaseProviders, IEnumerable<IConnectionCreatedNotification> connectionCreatedNotifications)
    {
        DatabaseProviders = databaseProviders;
        ConnectionCreatedNotifications = connectionCreatedNotifications;
    }

    public IEnumerable<IDatabaseProvider> DatabaseProviders { get; }
    public IEnumerable<IConnectionCreatedNotification> ConnectionCreatedNotifications { get; }

    public IDatabaseProvider GetDatabaseProvider(DatabaseType databaseType, bool throwOnNotFound = false)
    {
        var provider = (from i in DatabaseProviders where i.ForDatabaseType == databaseType select i).FirstOrDefault();
        if (throwOnNotFound &&
            provider == null)
        {
            throw new ArgumentOutOfRangeException("databaseType", $"DatabaseType '{databaseType.ToString()}' has no provider.");
        }
        return provider;
    }

    public Models.DatabaseType GetDatabaseType(DbConnection connection)
    {
        if (connection.GetType().FullName.StartsWith("System.Data.Odbc.OdbcConnection"))
        {
            return Models.DatabaseType.Odbc;
        }
        else if (connection.GetType().FullName.StartsWith("System.Data.Oracle", StringComparison.InvariantCultureIgnoreCase))
        {
            return Models.DatabaseType.Oracle;
        }
        else if (connection.GetType().FullName.StartsWith("System.Data.SqlServerCe", StringComparison.InvariantCultureIgnoreCase))
        {
            return Models.DatabaseType.MicrosoftSQLServerCompact;
        }
        else if (connection.GetType().FullName.StartsWith("System.Data.Ole", StringComparison.InvariantCultureIgnoreCase))
        {
            if (connection.ConnectionString.Contains("Microsoft.ACE"))
            {
                return Models.DatabaseType.AccessOLE;
            }
            return Models.DatabaseType.OLE;
        }
        else if (connection.GetType().FullName.StartsWith("MySql", StringComparison.InvariantCultureIgnoreCase))
        {
            return Models.DatabaseType.MySql;
        }
        return Models.DatabaseType.MicrosoftSQLServer;
    }

    public Models.DatabaseType GetDatabaseType(System.Configuration.ConnectionStringSettings connectionString)
    {
        switch (GetProviderName(connectionString).ToLower())
        {
            case "system.data.odbc":
                return Models.DatabaseType.Odbc;
            case "system.data.oracle":
                return Models.DatabaseType.Oracle;
            case "system.data.sqlserverce.3.5":
                return Models.DatabaseType.MicrosoftSQLServerCompact;
            case "system.data.sqlclient":
                return Models.DatabaseType.MicrosoftSQLServer;
            case "mysql.data.mysqlclient":
                return Models.DatabaseType.MySql;
            case "system.data.oledb":
                if (connectionString.ConnectionString.Contains("Microsoft.ACE"))
                {
                    return Models.DatabaseType.AccessOLE;
                }
                return Models.DatabaseType.OLE;
        }
        return Models.DatabaseType.MicrosoftSQLServer;
    }

    public string GetProviderName(DbConnection connection)
    {
        switch (GetDatabaseType(connection))
        {
            case Models.DatabaseType.Oracle:
                return "System.Data.Oracle";
            case Models.DatabaseType.Odbc:
                return "System.Data.Odbc";
            case Models.DatabaseType.MicrosoftSQLServerCompact:
                return "System.Data.SqlServerCe.3.5";
            case Models.DatabaseType.OLE:
            case Models.DatabaseType.AccessOLE:
                return "System.Data.OleDb";
            case Models.DatabaseType.MySql:
                return "MySql.Data.MySqlClient";
        }
        return "System.Data.SqlClient";
    }

    public string GetProviderName(System.Configuration.ConnectionStringSettings connectionString)
    {
        return connectionString.ProviderName;
    }

    public void NotifyConnections(IDbConnection connection)
    {
        foreach (var notification in ConnectionCreatedNotifications)
        {
            notification.Notify(connection);
        }
    }

}
