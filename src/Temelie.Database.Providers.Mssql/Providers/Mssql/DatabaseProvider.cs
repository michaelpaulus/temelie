using System.Data.Common;
using System.Text;
using Temelie.Database.Models;
using Temelie.Database.Services;
using Temelie.DependencyInjection;
using Microsoft.Data.SqlClient;

namespace Temelie.Database.Providers.Mssql;

[ExportProvider(typeof(IDatabaseProvider))]
public partial class DatabaseProvider : DatabaseProviderBase
{

    private readonly IEnumerable<IConnectionCreatedNotification> _connectionCreatedNotifications;
    private readonly IDatabaseExecutionService _databaseService;

    public DatabaseProvider(IEnumerable<IConnectionCreatedNotification> connectionCreatedNotifications,
        IEnumerable<IDatabaseModelProvider> databaseModelProviders) : base(databaseModelProviders)
    {
        _connectionCreatedNotifications = connectionCreatedNotifications;
        var factory = new DatabaseFactory([this], _connectionCreatedNotifications);
        _databaseService = new Services.DatabaseExecutionService(factory);
    }

    public static string ProviderName => nameof(SqlConnection);

    public override string QuoteCharacterStart => "[";
    public override string QuoteCharacterEnd => "]";

    public override string Name => ProviderName;

    public override string DefaultConnectionString => "Data Source=(local);Initial Catalog=Database;Integrated Security=True;User Id=;Password=;Encrypt=False;";

    public DbProviderFactory CreateProvider()
    {
        return SqlClientFactory.Instance;
    }

    public override ColumnTypeModel GetColumnType(ColumnTypeModel sourceColumnType)
    {

        var targetColumnType = new Models.ColumnTypeModel();

        targetColumnType.ColumnType = sourceColumnType.ColumnType.ToUpper().Trim();

        targetColumnType.Precision = sourceColumnType.Precision;
        targetColumnType.Scale = sourceColumnType.Scale;

        switch (targetColumnType.ColumnType)
        {
            case "TEXT":
                targetColumnType.ColumnType = "NVARCHAR";
                if (targetColumnType.Precision.GetValueOrDefault() < 4000)
                {
                    targetColumnType.Precision = int.MaxValue;
                }
                break;
        }

        switch (targetColumnType.ColumnType)
        {
            case "NVARCHAR":
            case "VARCHAR":
            case "VARBINARY":
                if (targetColumnType.Precision.GetValueOrDefault() > 4000 ||
                    targetColumnType.Precision.GetValueOrDefault() == 0 ||
                    targetColumnType.Precision.GetValueOrDefault() == -1)
                {
                    targetColumnType.Precision = int.MaxValue;
                }
                break;
        }

        return targetColumnType;

    }

    public override string TransformConnectionString(string connectionString)
    {
        var csb = new SqlConnectionStringBuilder(connectionString);
        return csb.ConnectionString;
    }

    public override bool TryHandleColumnValueLoadException(Exception ex, ColumnModel column, out object? value)
    {
        value = null;
        return false;
    }

    public override void UpdateParameter(DbParameter parameter, ColumnModel column)
    {
        switch (parameter.DbType)
        {
            case System.Data.DbType.StringFixedLength:
            case System.Data.DbType.String:
                parameter.Size = column.Precision;
                break;
            case System.Data.DbType.Time:
                if (parameter is SqlParameter parameter1)
                {
                    parameter1.SqlDbType = System.Data.SqlDbType.Time;
                }
                break;
        }
    }

    public override string GetDatabaseName(string connectionString)
    {
        var sqlCommandBuilder = new SqlConnectionStringBuilder(connectionString);
        return sqlCommandBuilder.InitialCatalog;
    }

    public override bool SupportsConnection(DbConnection connection)
    {
        return connection is SqlConnection;
    }

    public override DbConnection CreateConnection()
    {
        return new SqlConnection();
    }

    public override int GetRowCount(DbCommand command, string schemaName, string tableName)
    {
        try
        {
            command.CommandText = $@"SELECT 
	SUM(sysindexes.rows)
FROM 
	sys.tables INNER JOIN 
	sys.sysindexes ON 
		tables.object_id = sysindexes.id AND 
		sysindexes.indid < 2 INNER JOIN
	sys.schemas ON
		tables.schema_id = schemas.schema_id
WHERE
    schemas.name = '{schemaName}' AND
	tables.name = '{tableName}'";
            return System.Convert.ToInt32(command.ExecuteScalar()?.ToString());
        }
        catch
        {

        }
        command.CommandText = $"SELECT COUNT(1) FROM [{schemaName}].[{tableName}]";
        return System.Convert.ToInt32(command.ExecuteScalar()?.ToString());
    }

    public override string GetSelectStatement(string schemaName, string tableName, IEnumerable<string> columns)
    {
        var sb = new StringBuilder();

        sb.AppendLine("SELECT");

        var first = true;

        foreach (var column in columns)
        {
            if (!first)
            {
                sb.AppendLine(",");
            }
            sb.Append($"    [{column}]");
            first = false;
        }

        sb.AppendLine("");

        sb.AppendLine("FROM");
        sb.AppendLine($"    [{schemaName}].[{tableName}]");

        return sb.ToString();
    }
}
