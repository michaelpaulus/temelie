using System.Data;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.Json;
using System.Transactions;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Temelie.Database.Extensions;
using Temelie.Database.Models;
using Temelie.Database.Models.ChangeTracking;
using Temelie.Database.Services;

namespace Temelie.Database.Providers;

public abstract class MssqlChangeTrackingProviderBase : IChangeTrackingProvider
{
    private readonly IDatabaseExecutionService _databaseExecutionService;
    private readonly IDatabaseFactory _databaseFactory;
    private readonly ITableConverterService _tableConverterService;
    private readonly IDatabaseModelService _databaseModelService;
    private readonly ILogger<MssqlChangeTrackingProviderBase> _logger;

    public MssqlChangeTrackingProviderBase(IConfiguration configuration,
        IDatabaseExecutionService databaseExecutionService,
        IDatabaseFactory databaseFactory,
        ITableConverterService tableConverterService,
        IDatabaseModelService databaseModelService,
        ILogger<MssqlChangeTrackingProviderBase> logger)
    {
        _databaseExecutionService = databaseExecutionService;
        _databaseFactory = databaseFactory;
        _tableConverterService = tableConverterService;
        _databaseModelService = databaseModelService;
        _logger = logger;
    }

    public string Provider => nameof(SqlConnection);

    public virtual Task DetectChangesAsync(ConnectionStringModel sourceConnectionString)
    {
        return Task.CompletedTask;
    }

    public abstract Task<IEnumerable<ChangeTrackingTable>> GetTrackedTablesAsync(ConnectionStringModel sourceConnectionString);

    public abstract Task<IEnumerable<ChangeTrackingMapping>> GetMappingsAsync(string source, ConnectionStringModel targetConnectionString);

    public abstract Task<byte[]> GetCurrentVersionAsync(ConnectionStringModel sourceConnectionString, ChangeTrackingTable table);

    public abstract Task<int> GetTrackedTableChangesCountAsync(ConnectionStringModel sourceConnectionString, ChangeTrackingTable table, TableModel tableModel, byte[] currentVersion, ChangeTrackingMapping mapping);

    public abstract IAsyncEnumerable<ChangeTrackingRow> GetTrackedTableChangesAsync(ConnectionStringModel sourceConnectionString, ChangeTrackingTable table, TableModel tableModel, byte[] currentVersion, ChangeTrackingMapping mapping);

    public async Task ApplyChangesAsync(ConnectionStringModel targetConnectionString, ChangeTrackingTable table, TableModel tableModel, ChangeTrackingMapping mapping, IAsyncEnumerable<ChangeTrackingRow> changes, int count)
    {
        if (mapping.IsNextSyncFull)
        {
            await ApplyChangesFullAsync(targetConnectionString, table, tableModel, mapping, changes, count).ConfigureAwait(false);
        }
        else
        {
            await ApplyChangesPartialAsync(targetConnectionString, table, tableModel, mapping, changes, count).ConfigureAwait(false);
        }
    }

    public async Task ApplyChangesFullAsync(ConnectionStringModel targetConnectionString, ChangeTrackingTable table, TableModel tableModel, ChangeTrackingMapping mapping, IAsyncEnumerable<ChangeTrackingRow> changes, int count)
    {
        var tempTable = CreateTargetTableModel(tableModel, mapping, $"__{mapping.TargetTableName}");
        var targetTable = CreateTargetTableModel(tableModel, mapping);

        var columns = targetTable.Columns.Where(i => !i.IsComputed).Select(i => i.ColumnName).ToArray();
        var pkColumns = targetTable.Columns.Where(i => i.IsPrimaryKey).Select(i => i.ColumnName).ToArray();

        using (var conn = _databaseExecutionService.CreateDbConnection(targetConnectionString))
        {
            using (var cmd = _databaseExecutionService.CreateDbCommand(conn))
            {
                cmd.CommandText = @$"
DROP TABLE IF EXISTS [{tempTable.SchemaName}].[{tempTable.TableName}]
";
                await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
            }

            _logger.LogInformation($"Creating temporary table {tempTable.SchemaName}.{tempTable.TableName}");

            await CreateTableAsync(tempTable, conn).ConfigureAwait(false);

            _logger.LogInformation($"Creating target table {targetTable.SchemaName}.{targetTable.TableName}");

            await CreateTableAsync(targetTable, conn).ConfigureAwait(false);

            _logger.LogInformation($"Inserting {count} temp rows");

            _tableConverterService.ConvertBulk((progress) =>
                {
                    _logger.LogInformation($"Inserting {progress.ProgressPercentage}% temp rows");
                },
               tempTable,
               new SourceDataReader(tempTable, changes),
               count,
               tempTable,
               conn,
               false,
               10000,
               false,
               true
               );

            _logger.LogInformation($"Merging into target table {targetTable.SchemaName}.{targetTable.TableName}");

            using (var cmd = _databaseExecutionService.CreateDbCommand(conn))
            {
                cmd.CommandText = @$"
DROP TABLE IF EXISTS [{targetTable.SchemaName}].[{targetTable.TableName}]
";
                await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
            }

            using (var cmd = _databaseExecutionService.CreateDbCommand(conn))
            {
                cmd.CommandText = @$"
EXEC sp_rename '{tempTable.SchemaName}.{tempTable.TableName}', '{targetTable.TableName}'
";
                await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
            }

        }
    }

    public async Task ApplyChangesPartialAsync(ConnectionStringModel targetConnectionString, ChangeTrackingTable table, TableModel tableModel, ChangeTrackingMapping mapping, IAsyncEnumerable<ChangeTrackingRow> changes, int count)
    {
        var tempTable = CreateTargetTempTableModel(tableModel, mapping);
        var targetTable = CreateTargetTableModel(tableModel, mapping);

        var columns = targetTable.Columns.Where(i => !i.IsComputed).Select(i => i.ColumnName).ToArray();
        var pkColumns = targetTable.Columns.Where(i => i.IsPrimaryKey).Select(i => i.ColumnName).ToArray();

        using (var conn = _databaseExecutionService.CreateDbConnection(targetConnectionString))
        {
            _logger.LogInformation($"Creating temporary table {tempTable.SchemaName}.{tempTable.TableName}");

            await CreateTableAsync(tempTable, conn).ConfigureAwait(false);

            _logger.LogInformation($"Creating target table {targetTable.SchemaName}.{targetTable.TableName}");

            await CreateTableAsync(targetTable, conn).ConfigureAwait(false);

            _logger.LogInformation($"Inserting {count} temp rows");

            _tableConverterService.ConvertBulk((progress) =>
            {
                _logger.LogInformation($"Inserting {progress.ProgressPercentage}% temp rows");
            },
               tempTable,
               new SourceDataReader(tempTable, changes),
               count,
               tempTable,
               conn,
               false,
               10000,
               false,
               true
               );

            _logger.LogInformation($"Merging into target table {targetTable.SchemaName}.{targetTable.TableName}");

            using (var cmd = _databaseExecutionService.CreateDbCommand(conn))
            {
                cmd.CommandText = @$"
MERGE INTO [{targetTable.SchemaName}].[{targetTable.TableName}]
USING (
    SELECT
        {string.Join(", ", columns.Select(i => $"[{i}]"))}
    FROM
        [{tempTable.SchemaName}].[{tempTable.TableName}] as source
    WHERE
        source.SYS_CHANGE_OPERATION <> 'D'
) AS source
ON {string.Join(" AND ", pkColumns.Select(c => $"[{targetTable.TableName}].[{c}] = source.[{c}]"))}
WHEN MATCHED THEN
    UPDATE
SET
    {string.Join(", ", columns.Select(c => $"[{targetTable.TableName}].[{c}] = source.[{c}]"))}
WHEN NOT MATCHED THEN
    INSERT ({string.Join(", ", columns)})
    VALUES ({string.Join(", ", columns.Select(c => $"source.[{c}]"))});
";
                await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
            }

            _logger.LogInformation($"Deleting from target table {targetTable.SchemaName}.{targetTable.TableName}");

            using (var cmd = _databaseExecutionService.CreateDbCommand(conn))
            {
                cmd.CommandText = @$"
DELETE FROM
    [{targetTable.SchemaName}].[{targetTable.TableName}]
WHERE
    EXISTS
    (
        SELECT
            1
        FROM
            [{tempTable.SchemaName}].[{tempTable.TableName}] AS source
        WHERE
            source.SYS_CHANGE_OPERATION = 'D' AND
            {string.Join(" AND ", pkColumns.Select(c => $"[{targetTable.TableName}].[{c}] = source.[{c}]"))}
    );
";
                await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
            }

        }
    }

    private Task CreateTableAsync(TableModel tableModel, DbConnection dbConnection)
    {
        var provider = _databaseFactory.GetDatabaseProvider(dbConnection);

        var script = provider.GetScript(tableModel);

        if (string.IsNullOrEmpty(script.CreateScript))
        {
            throw new Exception("Create table script is null");
        }

        _databaseExecutionService.ExecuteFile(dbConnection, script.CreateScript);

        var columns = _databaseModelService.GetTableColumns(dbConnection);
        var tables = _databaseModelService.GetTables(dbConnection, new DatabaseModelOptions() { ExcludeDoubleUnderscoreObjects = false }, columns);
        var currentTable = tables.Where(i => i.TableName.EqualsIgnoreCase(tableModel.TableName) && i.SchemaName.EqualsIgnoreCase(tableModel.SchemaName)).FirstOrDefault();

        if (currentTable is not null)
        {
            var newColumns = tableModel.Columns.Where(i => !currentTable.Columns.Any(i2 => i.ColumnName == i2.ColumnName)).ToList();
            var removedColumns = currentTable.Columns.Where(i => !tableModel.Columns.Any(i2 => i.ColumnName == i2.ColumnName)).ToList();

            if (newColumns.Count > 0 || removedColumns.Count > 0)
            {
                var sb = new StringBuilder();

                foreach (var newColumn in newColumns)
                {
                    var columnScript = provider.GetColumnScript(newColumn);
                    if (columnScript is not null && !string.IsNullOrEmpty(columnScript.CreateScript))
                    {
                        sb.AppendLine(columnScript.CreateScript);
                    }
                }

                foreach (var removedColumn in removedColumns)
                {
                    var columnScript = provider.GetColumnScript(removedColumn);
                    if (columnScript is not null && !string.IsNullOrEmpty(columnScript.DropScript))
                    {
                        sb.AppendLine(columnScript.DropScript);
                    }
                }

                _databaseExecutionService.ExecuteFile(dbConnection, sb.ToString());
            }
        }

        return Task.CompletedTask;
    }

    protected async Task<IEnumerable<T>> GetRecordsAsync<T>(ConnectionStringModel connectionString, string query, params SqlParameter[] parameters)
    {
        var results = new List<T>();
        using (var conn = _databaseExecutionService.CreateDbConnection(connectionString))
        using (var cmd = _databaseExecutionService.CreateDbCommand(conn))
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

    public async Task UpdateSyncedVersionAsync(ConnectionStringModel targetConnectionString, int changeTrackingMappingId, byte[] version)
    {
        using (var ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        using (var conn = _databaseExecutionService.CreateDbConnection(targetConnectionString))
        using (var cmd = _databaseExecutionService.CreateDbCommand(conn))
        {
            cmd.CommandText = @"
UPDATE
    ChangeTrackingMappings
SET
    LastSyncedVersion = @LastSyncedVersion,
    ModifiedDate = GETUTCDATE()
WHERE
    ChangeTrackingMappingId = @ChangeTrackingMappingId
";
            cmd.Parameters.Add(new SqlParameter("@LastSyncedVersion", version));
            cmd.Parameters.Add(new SqlParameter("@ChangeTrackingMappingId", changeTrackingMappingId));
            await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
            ts.Complete();
        }
    }

    public async Task FlagSyncingAsync(ConnectionStringModel targetConnectionString, int changeTrackingMappingId, bool isSyncing)
    {
        using (var ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        using (var conn = _databaseExecutionService.CreateDbConnection(targetConnectionString))
        {

            if (!isSyncing)
            {
                using (var cmd = _databaseExecutionService.CreateDbCommand(conn))
                {
                    cmd.CommandText = @"
UPDATE
    ChangeTrackingMappings
SET
    IsNextSyncFull = @IsNextSyncFull,
    ModifiedDate = GETUTCDATE()
WHERE
    ChangeTrackingMappingId = @ChangeTrackingMappingId
";
                    cmd.Parameters.Add(new SqlParameter("@IsNextSyncFull", false));
                    cmd.Parameters.Add(new SqlParameter("@ChangeTrackingMappingId", changeTrackingMappingId));
                    await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                }
            }

            using (var cmd = _databaseExecutionService.CreateDbCommand(conn))
            {
                cmd.CommandText = @"
UPDATE
    ChangeTrackingMappings
SET
    IsSyncing = @IsSyncing,
    ModifiedDate = GETUTCDATE()
WHERE
    ChangeTrackingMappingId = @ChangeTrackingMappingId
";
                cmd.Parameters.Add(new SqlParameter("@IsSyncing", isSyncing));
                cmd.Parameters.Add(new SqlParameter("@ChangeTrackingMappingId", changeTrackingMappingId));
                await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
            }

            ts.Complete();
        }
    }

    private TableModel CreateTargetTempTableModel(TableModel tableModel, ChangeTrackingMapping mapping)
    {
        var tempTable = JsonSerializer.Deserialize<TableModel>(JsonSerializer.Serialize(tableModel))!;

        tempTable.TableName = "#" + mapping.TargetTableName;
        tempTable.SchemaName = mapping.TargetSchemaName;

        tempTable.Columns.Add(new ColumnModel()
        {
            ColumnName = "SYS_CHANGE_OPERATION",
            ColumnType = "CHAR",
            Precision = 1,
            IsNullable = false
        });

        tempTable.ExtendedProperties = new Dictionary<string, string>();
        tempTable.PartitionSchemeName = null;
        tempTable.PartitionSchemeColumns = null;

        var index = 0;

        foreach (var column in tempTable.Columns)
        {
            column.SchemaName = tempTable.SchemaName;
            column.TableName = tempTable.TableName;
            column.IsNullable = true;
            column.ColumnDefault = null;
            column.ExtendedProperties = new Dictionary<string, string>();
            column.ColumnId = ++index;
        }

        return tempTable;
    }

    private TableModel CreateTargetTableModel(TableModel tableModel, ChangeTrackingMapping mapping, string tableName = null)
    {
        var tempTable = JsonSerializer.Deserialize<TableModel>(JsonSerializer.Serialize(tableModel))!;

        tempTable.TableName = string.IsNullOrEmpty(tableName) ? mapping.TargetTableName : tableName;
        tempTable.SchemaName = mapping.TargetSchemaName;

        tempTable.ExtendedProperties = new Dictionary<string, string>();

        foreach (var column in tempTable.Columns)
        {
            column.SchemaName = tempTable.SchemaName;
            column.TableName = tempTable.TableName;
            column.ExtendedProperties = new Dictionary<string, string>();
        }

        return tempTable;
    }

    protected class SourceDataReader : IDataReader
    {
        private readonly TableModel _tableModel;
        private readonly IEnumerator<ChangeTrackingRow> _changes;

        public SourceDataReader(TableModel tableModel, IAsyncEnumerable<ChangeTrackingRow> changes)
        {
            _tableModel = tableModel;
            _changes = changes.ToBlockingEnumerable().GetEnumerator();

        }

        public object this[int i] => GetValue(i);
        public object this[string name] => GetValue(GetOrdinal(name));

        public int Depth { get; }
        public bool IsClosed { get; }
        public int RecordsAffected { get; }
        public int FieldCount => _tableModel.Columns.Count + 2;

        public void Close()
        {

        }

        public void Dispose()
        {

        }

        public bool GetBoolean(int i)
        {
            throw new NotImplementedException();
        }

        public byte GetByte(int i)
        {
            throw new NotImplementedException();
        }

        public long GetBytes(int i, long fieldOffset, byte[]? buffer, int bufferoffset, int length)
        {
            throw new NotImplementedException();
        }

        public char GetChar(int i)
        {
            throw new NotImplementedException();
        }

        public long GetChars(int i, long fieldoffset, char[]? buffer, int bufferoffset, int length)
        {
            throw new NotImplementedException();
        }

        public IDataReader GetData(int i)
        {
            throw new NotImplementedException();
        }

        public string GetDataTypeName(int i)
        {
            throw new NotImplementedException();
        }

        public DateTime GetDateTime(int i)
        {
            throw new NotImplementedException();
        }

        public decimal GetDecimal(int i)
        {
            throw new NotImplementedException();
        }

        public double GetDouble(int i)
        {
            throw new NotImplementedException();
        }

        [return: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicFields | DynamicallyAccessedMemberTypes.PublicProperties)]
        public Type GetFieldType(int i)
        {
            throw new NotImplementedException();
        }

        public float GetFloat(int i)
        {
            throw new NotImplementedException();
        }

        public Guid GetGuid(int i)
        {
            throw new NotImplementedException();
        }

        public short GetInt16(int i)
        {
            throw new NotImplementedException();
        }

        public int GetInt32(int i)
        {
            throw new NotImplementedException();
        }

        public long GetInt64(int i)
        {
            throw new NotImplementedException();
        }

        public string GetName(int i)
        {
            throw new NotImplementedException();
        }

        public int GetOrdinal(string name)
        {
            var column = _tableModel.Columns.FirstOrDefault(c => c.ColumnName == name);
            return _tableModel.Columns.IndexOf(column);
        }

        public DataTable? GetSchemaTable()
        {
            throw new NotImplementedException();
        }

        public string GetString(int i)
        {
            throw new NotImplementedException();
        }

        public object GetValue(int i)
        {
            var column = _tableModel.Columns[i];
            if (column.ColumnName == "SYS_CHANGE_VERSION")
            {
                return _changes.Current.ChangeVersion;
            }
            else if (column.ColumnName == "SYS_CHANGE_OPERATION")
            {
                return _changes.Current.ChangeOperation;
            }
            return _changes.Current.ColumnValues[column.ColumnName] ?? DBNull.Value;
        }

        public int GetValues(object[] values)
        {
            throw new NotImplementedException();
        }

        public bool IsDBNull(int i)
        {
            throw new NotImplementedException();
        }

        public bool NextResult()
        {
            throw new NotImplementedException();
        }

        public bool Read()
        {
            return _changes.MoveNext();
        }
    }

}
