using System.Data;
using System.Data.Common;
using Temelie.Database.Models;
using Temelie.Database.Services;
using Temelie.DependencyInjection;
using System.Text;
using MySqlConnector;

namespace Temelie.Database.Providers.MySql;
[ExportProvider(typeof(IDatabaseProvider))]
public partial class DatabaseProvider : DatabaseProviderBase
{

    private readonly IEnumerable<IConnectionCreatedNotification> _connectionCreatedNotifications;
    private readonly IDatabaseExecutionService _database;

    public DatabaseProvider(IEnumerable<IConnectionCreatedNotification> connectionCreatedNotifications,
        IEnumerable<IDatabaseModelProvider> databaseModelProviders) : base(databaseModelProviders)
    {
        _connectionCreatedNotifications = connectionCreatedNotifications;
        var factory = new DatabaseFactory([this], _connectionCreatedNotifications);
        _database = new DatabaseExecutionService(factory);
    }

    public static string ProviderName = nameof(MySqlConnection);

    public override string QuoteCharacterStart => "`";
    public override string QuoteCharacterEnd => "`";

    public override string Name => ProviderName;

    public override string DefaultConnectionString => "Data Source=localhost;Database=database;User Id=;Password=";

    public DbProviderFactory CreateProvider()
    {
        return new MySqlDbProviderFactory();
    }

    public override bool TryHandleColumnValueLoadException(Exception ex, Models.ColumnModel column, out object value)
    {
        var mysqlException = ex as MySqlConversionException;

        if (mysqlException != null)
        {
            if (column.DbType == System.Data.DbType.DateTime2 ||
                column.DbType == System.Data.DbType.Date ||
                column.DbType == System.Data.DbType.DateTime ||
                column.DbType == System.Data.DbType.DateTimeOffset)
            {
                value = DateTime.MinValue;
                return true;
            }
        }

        value = null;

        return false;
    }

    public override Models.ColumnTypeModel GetColumnType(Models.ColumnTypeModel sourceColumnType)
    {

        var targetColumnType = new Models.ColumnTypeModel();

        targetColumnType.ColumnType = sourceColumnType.ColumnType.ToUpper().Replace("UNSIGNED", "").Trim();

        if (targetColumnType.ColumnType.Contains("(") &&
        targetColumnType.ColumnType.EndsWith(")"))
        {
            if (targetColumnType.ColumnType == "TINYINT(1)")
            {
                targetColumnType.ColumnType = "BIT";
            }
            else
            {
                targetColumnType.ColumnType = targetColumnType.ColumnType.Substring(0, targetColumnType.ColumnType.IndexOf("("));
            }
        }

        targetColumnType.Precision = sourceColumnType.Precision;
        targetColumnType.Scale = sourceColumnType.Scale;

        switch (targetColumnType.ColumnType)
        {
            case "LONG VARCHAR":
            case "TEXT":
            case "MEDIUMTEXT":
            case "LONGTEXT":
                targetColumnType.ColumnType = "NVARCHAR";
                if (targetColumnType.Precision.GetValueOrDefault() < 4000)
                {
                    targetColumnType.Precision = int.MaxValue;
                }
                break;
            case "VARCHAR":
            case "STRING":
                targetColumnType.ColumnType = "NVARCHAR";
                break;
            case "INTEGER":
            case "INT32":
            case "MEDIUMINT":
                targetColumnType.ColumnType = "INT";
                break;
            case "INT16":
            case "TINYINT":
                targetColumnType.ColumnType = "SMALLINT";
                break;
            case "NUMERIC":
            case "DOUBLE":
            case "SINGLE":
            case "DEC":
                targetColumnType.ColumnType = "DECIMAL";
                break;
            case "TIMESTAMP":
                targetColumnType.ColumnType = "DATETIME2";
                break;
            case "DATETIME":
                targetColumnType.ColumnType = "DATETIME2";
                break;
            case "CHAR":
                targetColumnType.ColumnType = "NCHAR";
                break;
            case "BOOLEAN":
                targetColumnType.ColumnType = "BIT";
                break;
            case "BYTE[]":
            case "BLOB":
            case "LONGBLOB":
                targetColumnType.ColumnType = "VARBINARY";
                break;
        }

        switch (targetColumnType.ColumnType)
        {
            case "NVARCHAR":
            case "VARBINARY":
                if (targetColumnType.Precision.GetValueOrDefault() > 4000)
                {
                    targetColumnType.Precision = int.MaxValue;
                }
                break;
        }

        return targetColumnType;

    }

    public override string TransformConnectionString(string connectionString)
    {
        var csb = new MySqlConnectionStringBuilder(connectionString);
        csb.DefaultCommandTimeout = Temelie.Database.Services.DatabaseExecutionService.DefaultCommandTimeout;
        return csb.ConnectionString;
    }

    public override void UpdateParameter(DbParameter parameter, ColumnModel column)
    {
        switch (parameter.DbType)
        {
            case System.Data.DbType.StringFixedLength:
            case System.Data.DbType.String:
                parameter.Size = column.Precision;
                break;
            case DbType.DateTime2:
                if (parameter is MySqlParameter parameter1)
                {
                    parameter1.MySqlDbType = MySqlDbType.DateTime;
                }
                break;
        }
    }

    protected void UpdateSchemaColumns(DataTable table, DataTable dataTypes)
    {

        if (table.Columns.Contains("ordinal_position"))
        {
            table.Columns["ordinal_position"].ColumnName = "column_id";
        }

        if (table.Columns.Contains("type_name") && !(table.Columns.Contains("column_type")))
        {
            table.Columns["type_name"].ColumnName = "column_type";
        }

        if (!table.Columns.Contains("is_primary_key"))
        {
            table.Columns.Add("is_primary_key", typeof(bool));

            foreach (var row in table.Rows.OfType<System.Data.DataRow>())
            {
                row["is_primary_key"] = false;

                if (table.Columns.Contains("COLUMN_KEY"))
                {
                    string value = Convert.ToString(row["COLUMN_KEY"]);
                    if (value != null &&
                        value.Equals("PRI", StringComparison.OrdinalIgnoreCase))
                    {
                        row["is_primary_key"] = true;
                    }
                }
            }
        }

        if (!table.Columns.Contains("is_identity"))
        {
            table.Columns.Add("is_identity", typeof(bool));
            foreach (var row in table.Rows.OfType<System.Data.DataRow>())
            {
                row["is_identity"] = false;

                if (table.Columns.Contains("EXTRA"))
                {
                    string value = Convert.ToString(row["EXTRA"]);
                    if (value != null &&
                        value.Equals("auto_increment", StringComparison.OrdinalIgnoreCase))
                    {
                        row["is_identity"] = true;
                    }
                }

            }
        }

        if (table.Columns.Contains("data_type") && !(table.Columns.Contains("column_type")))
        {
            table.Columns.Add("column_type", typeof(string));

            foreach (var row in table.Rows.OfType<System.Data.DataRow>())
            {
                string strColumnType = Convert.ToString(row["data_type"]);

                if (!(string.IsNullOrEmpty(strColumnType)) && strColumnType.ToUpper().EndsWith(" IDENTITY"))
                {
                    strColumnType = strColumnType.Substring(0, strColumnType.Length - " IDENTITY".Length);
                    row["is_identity"] = true;
                }

                var dataTypeRows = (
                    from i in dataTypes.Rows.OfType<System.Data.DataRow>()
                    where string.Equals(Convert.ToString(i["ProviderDbType"]), strColumnType)
                    select i).ToList();

                if (dataTypeRows.Count == 0 && dataTypes.Columns.Contains("SqlType"))
                {
                    dataTypeRows = (
                        from i in dataTypes.Rows.OfType<System.Data.DataRow>()
                        where string.Equals(Convert.ToString(i["SqlType"]), strColumnType)
                        select i).ToList();
                }

                var dataTypeRow = dataTypeRows.FirstOrDefault();

                if (dataTypeRow != null)
                {
                    strColumnType = Convert.ToString(dataTypeRow["DataType"]);
                    if (strColumnType.Contains('.'.ToString()))
                    {
                        strColumnType = strColumnType.Split('.')[strColumnType.Split('.').Length - 1];
                    }
                }

                row["column_type"] = strColumnType;
            }

            table.Columns.Remove("data_type");
        }

        if (table.Columns.Contains("numeric_scale"))
        {
            table.Columns["numeric_scale"].ColumnName = "scale";
        }

        if (table.Columns.Contains("decimal_digits"))
        {
            table.Columns["decimal_digits"].ColumnName = "scale";
        }

        if (table.Columns.Contains("num_prec_radix"))
        {
            table.Columns["num_prec_radix"].ColumnName = "numeric_precision";
        }

        if (table.Columns.Contains("column_size"))
        {
            table.Columns["column_size"].ColumnName = "character_maximum_length";
        }

        foreach (var row in table.Rows.OfType<DataRow>())
        {

            string columnDefault = GetStringValue(row, "COLUMN_DEFAULT");

            if (!string.IsNullOrEmpty(columnDefault))
            {
                if (columnDefault == "NULL" ||
                    columnDefault == "(NULL)")
                {
                    columnDefault = "";
                }
                else if (columnDefault == "0000-00-00 00:00:00")
                {
                    columnDefault = "";
                }
                else if (columnDefault == "b'0'")
                {
                    columnDefault = "0";
                }

                if (columnDefault.StartsWith("_utf8mb4\\'") && columnDefault.EndsWith("\\'"))
                {
                    columnDefault = columnDefault.Replace("_utf8mb4\\'", "").Replace("\\'", "");
                }

            }

            row["COLUMN_DEFAULT"] = columnDefault;

        }

        if (table.Columns.Contains("numeric_precision"))
        {
            if (table.Columns.Contains("character_maximum_length"))
            {
                table.Columns.Add("precision", typeof(long));
                foreach (System.Data.DataRow row in table.Rows)
                {
                    if (row.IsNull("numeric_precision"))
                    {
                        if (!row.IsNull("character_maximum_length"))
                        {
                            try
                            {
                                row["precision"] = row["character_maximum_length"];
                            }
#pragma warning disable CS0168 // Variable is declared but never used
                            catch (ArgumentException ex)
#pragma warning restore CS0168 // Variable is declared but never used
                            {
                                row["precision"] = long.MaxValue;
                            }
                        }
                    }
                    else
                    {
                        row["precision"] = row["numeric_precision"];
                    }
                }
            }
            else
            {
                table.Columns["numeric_precision"].ColumnName = "precision";
            }
        }

    }

    public override void SetReadTimeout(DbCommand sourceCommand)
    {
        var command = sourceCommand as MySqlCommand;
        if (command != null)
        {
            command.CommandText = "set net_write_timeout=99999;set net_read_timeout=99999;";
            command.ExecuteNonQuery();
        }
    }

    public override string GetDatabaseName(string connectionString)
    {
        return new MySqlConnectionStringBuilder(connectionString).Database;
    }

    public override bool SupportsConnection(DbConnection connection)
    {
        return connection is MySqlConnection;
    }

    public override DbConnection CreateConnection()
    {
        return new MySqlConnection();
    }

    public override int GetRowCount(DbCommand command, string schemaName, string tableName)
    {
        command.CommandText = $"SELECT COUNT(1) FROM {tableName}";
        var rowCount = System.Convert.ToInt32(command.ExecuteScalar());
        return rowCount;
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
            sb.Append($"    `{column}`");
            first = false;
        }

        sb.AppendLine("");

        sb.AppendLine("FROM");
        sb.AppendLine($"    `{tableName}`");

        return sb.ToString();
    }
}
