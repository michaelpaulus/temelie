using Temelie.Database.Providers;
using System.Data.Common;
using System.Data;
using System.Transactions;
using Temelie.Database.Models;
using Temelie.DependencyInjection;

namespace Temelie.Database.Services;

[ExportTransient(typeof(IDatabaseExecutionService))]
public class DatabaseExecutionService : IDatabaseExecutionService
{
    public const int DefaultCommandTimeout = 0;

    private readonly IDatabaseFactory _databaseFactory;

    public DatabaseExecutionService(IDatabaseFactory databaseFactory)
    {
        _databaseFactory = databaseFactory;
    }

    public DbCommand CreateDbCommand(DbConnection dbConnection)
    {
        DbCommand command = dbConnection.CreateCommand();
        command.Connection = dbConnection;

        command.CommandTimeout = DefaultCommandTimeout;

        return command;
    }

    public DbConnection CreateDbConnection(ConnectionStringModel connectionString)
    {
        var databaseProvider = _databaseFactory.GetDatabaseProvider(connectionString);
        DbConnection connection = databaseProvider.CreateConnection();
        connection.ConnectionString = connectionString.ConnectionString;
        connection.ConnectionString = databaseProvider.TransformConnectionString(connection.ConnectionString);
        _databaseFactory.NotifyConnections(connection);

        if (connection.State != ConnectionState.Open)
        {
          connection.Open();
        }

        return connection;
    }

    public System.Data.DataSet Execute(DbConnection connection, string sqlCommand)
    {
        var ds = new System.Data.DataSet();

        using (var command = CreateDbCommand(connection))
        {
            command.CommandText = sqlCommand;

            using (var reader = command.ExecuteReader())
            {
                var dt = new DataTable();
                dt.Load(reader, LoadOption.PreserveChanges);
                ds.Tables.Add(dt);
                var count = dt.Rows.Count;
            }
        }

        return ds;
    }

    public void ExecuteFile(DbConnection connection, string sqlCommand)
    {
        if (!(string.IsNullOrEmpty(sqlCommand)))
        {
            System.Text.RegularExpressions.Regex regEx = new System.Text.RegularExpressions.Regex("^[\\s]*GO[^a-zA-Z0-9]", System.Text.RegularExpressions.RegexOptions.IgnoreCase | System.Text.RegularExpressions.RegexOptions.Multiline);
            foreach (string commandText in regEx.Split(sqlCommand))
            {
                if (!(string.IsNullOrEmpty(commandText.Trim())))
                {
                    if (commandText.ToUpper().Contains("ALTER DATABASE") && System.Transactions.Transaction.Current is not null)
                    {
                        using (var scope = new TransactionScope(TransactionScopeAsyncFlowOption.Suppress))
                        {
                            ExecuteNonQuery(connection, commandText);
                            scope.Complete();
                        }
                    }
                    else
                    {
                        ExecuteNonQuery(connection, commandText);
                    }
                }
            }
        }
    }

    public void ExecuteFile(ConnectionStringModel connectionString, string sqlCommand)
    {
        if (!(string.IsNullOrEmpty(sqlCommand)))
        {
            using (DbConnection connection = CreateDbConnection(connectionString))
            {
                ExecuteFile(connection, sqlCommand);
            }
        }
    }

    public void ExecuteNonQuery(DbConnection connection, string sqlCommand)
    {
        if (!(string.IsNullOrEmpty(sqlCommand)))
        {
            using (DbCommand command = CreateDbCommand(connection))
            {
                command.CommandText = sqlCommand;
                command.ExecuteNonQuery();
            }
        }
    }

    public async Task<IEnumerable<T>> GetRecordsAsync<T>(ConnectionStringModel connectionString, string query, params DbParameter[] parameters)
    {
        var results = new List<T>();
        using (var conn = CreateDbConnection(connectionString))
        using (var cmd = CreateDbCommand(conn))
        {
            cmd.CommandText = query;
            cmd.Parameters.AddRange(parameters);
            using (var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false))
            {

                while (await reader.ReadAsync().ConfigureAwait(false))
                {
                    var item = Activator.CreateInstance<T>();
                    foreach (var prop in typeof(T).GetProperties())
                    {
                        if (!await reader.IsDBNullAsync(reader.GetOrdinal(prop.Name)).ConfigureAwait(false))
                        {
                            prop.SetValue(item, await reader.GetFieldValueAsync<object>(reader.GetOrdinal(prop.Name)).ConfigureAwait(false));
                        }
                    }
                    results.Add(item);
                }

            }
        }

        return results;
    }

}
