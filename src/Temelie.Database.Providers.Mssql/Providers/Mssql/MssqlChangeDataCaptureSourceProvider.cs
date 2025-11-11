using System.Data;
using System.Data.Common;
using System.Text;
using Microsoft.Data.SqlClient;
using Temelie.Database.Extensions;
using Temelie.Database.Models;
using Temelie.Database.Models.ChangeTracking;
using Temelie.Database.Services;
using Temelie.DependencyInjection;

namespace Temelie.Database.Providers;

[ExportProvider(typeof(IChangeTrackingSourceProvider))]
public class MssqlChangeDataCaptureSourceProvider : IChangeTrackingSourceProvider
{
    private readonly IDatabaseExecutionService _databaseExecutionService;
    private readonly IDatabaseModelService _databaseModelService;
    private readonly IDatabaseFactory _databaseFactory;

    public MssqlChangeDataCaptureSourceProvider(IDatabaseExecutionService databaseExecutionService,
        IDatabaseModelService databaseModelService,
        IDatabaseFactory databaseFactory)
    {
        _databaseExecutionService = databaseExecutionService;
        _databaseModelService = databaseModelService;
        _databaseFactory = databaseFactory;
    }

    public static string ProviderName => $"mssql_cdc";

    public string Provider => ProviderName;

    public async Task UpdateSchemaAsync(ConnectionStringModel sourceConnectionString, ChangeTrackingTable table)
    {
        using (var conn = _databaseExecutionService.CreateDbConnection(sourceConnectionString))
        {
            var columns = _databaseModelService.GetTableColumns(conn);
            var tables = _databaseModelService.GetTables(conn, new DatabaseModelOptions() { ExcludeCdcObjects = false }, columns);
            var sourceTable = tables.Where(i => i.TableName.EqualsIgnoreCase(table.TableName) && i.SchemaName.EqualsIgnoreCase(table.SchemaName)).FirstOrDefault();
            var cdcTable = tables.Where(i => i.TableName.EqualsIgnoreCase($"{table.Instance}_CT") && i.SchemaName.EqualsIgnoreCase("cdc")).FirstOrDefault();

            if (sourceTable is null || cdcTable is null)
            {
                return;
            }

            bool changed = false;

            var sourceColumns = sourceTable.Columns.ToList();

            foreach (var cdcColumn in cdcTable.Columns)
            {
                if (cdcColumn.ColumnName == "__$start_lsn" ||
                    cdcColumn.ColumnName == "__$end_lsn" ||
                    cdcColumn.ColumnName == "__$seqval" ||
                    cdcColumn.ColumnName == "__$operation" ||
                    cdcColumn.ColumnName == "__$update_mask" ||
                    cdcColumn.ColumnName == "__$command_id")
                {
                    continue;
                }
                var sourceColumn = sourceColumns.Where(i => i.ColumnName.EqualsIgnoreCase(cdcColumn.ColumnName)).FirstOrDefault();
                if (sourceColumn is null)
                {
                    changed = true;
                }
                else
                {
                    sourceColumns.Remove(sourceColumn);
                }
            }

            if (sourceColumns.Count > 0)
            {
                changed = true;
            }

            if (changed)
            {
                using (var cmd = _databaseExecutionService.CreateDbCommand(conn))
                {
                    cmd.CommandText = $@"
DROP TABLE IF EXISTS dbo.[__{table.Instance}_CT]";
                    await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                }

                using (var cmd = _databaseExecutionService.CreateDbCommand(conn))
                {
                    cmd.CommandText = $@"
SELECT * INTO dbo.[__{table.Instance}_CT] from cdc.[{table.Instance}_CT]";
                    await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                }

                using (var cmd = _databaseExecutionService.CreateDbCommand(conn))
                {
                    cmd.CommandText = $@"
EXEC sys.sp_cdc_disable_table
@source_schema = '{table.SchemaName}',
@source_name = '{table.TableName}',
@capture_instance = '{table.Instance}';
";
                    await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                }

                using (var cmd = _databaseExecutionService.CreateDbCommand(conn))
                {
                    cmd.CommandText = $@"
EXEC sys.sp_cdc_enable_table
@source_schema = '{table.SchemaName}',
@source_name = '{table.TableName}',
@role_name = NULL,
@capture_instance = '{table.Instance}';
";
                    await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                }

                columns = _databaseModelService.GetTableColumns(conn);
                tables = _databaseModelService.GetTables(conn, new DatabaseModelOptions() { ExcludeDoubleUnderscoreObjects = false, ExcludeCdcObjects = false }, columns);
                cdcTable = tables.Where(i => i.TableName.EqualsIgnoreCase($"{table.Instance}_CT") && i.SchemaName.EqualsIgnoreCase("cdc")).FirstOrDefault();
                var backupCdcTable = tables.Where(i => i.TableName.EqualsIgnoreCase($"__{table.Instance}_CT") && i.SchemaName.EqualsIgnoreCase("dbo")).FirstOrDefault();

                if (cdcTable is null || backupCdcTable is null)
                {
                    return;
                }

                await UpgradeTableAsync(cdcTable, backupCdcTable, conn).ConfigureAwait(false);

                using (var cmd = _databaseExecutionService.CreateDbCommand(conn))
                {
                    cmd.CommandText = $@"
INSERT INTO 
    cdc.{table.Instance}_CT
    (
        {string.Join(", ", cdcTable.Columns.Select(i => $"[{i.ColumnName}]"))}
    )
    (
    SELECT
        {string.Join(", ", cdcTable.Columns.Select(i => $"[{i.ColumnName}]"))}
    FROM
        dbo.__{table.Instance}_CT
    )
";
                    await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                }

                using (var cmd = _databaseExecutionService.CreateDbCommand(conn))
                {
                    cmd.CommandText = $@"
UPDATE
    cdc.change_tables
SET
    start_lsn = (SELECT
                    MIN(__$start_lsn)
	             FROM
		            cdc.{table.Instance}_CT
                )
WHERE
    capture_instance = '{table.Instance}_CT'
";
                    await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                }

                using (var cmd = _databaseExecutionService.CreateDbCommand(conn))
                {
                    cmd.CommandText = $@"
DROP TABLE IF EXISTS dbo.[__{table.Instance}_CT]";
                    await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                }

            }

        }
    }

    public Task DetectChangesAsync(ConnectionStringModel sourceConnectionString, ChangeTrackingTable table, ChangeTrackingMapping mapping)
    {
        return Task.CompletedTask;
    }

    public async Task<IEnumerable<ChangeTrackingTable>> GetTrackedTablesAsync(ConnectionStringModel sourceConnectionString)
    {
        return await _databaseExecutionService.GetRecordsAsync<ChangeTrackingTable>(sourceConnectionString, @"
SELECT 
    schemas.name SchemaName,
    tables.name TableName,
    change_tables.capture_instance Instance
FROM 
    cdc.change_tables INNER JOIN
    sys.tables ON
        change_tables.source_object_id = tables.object_id INNER JOIN
    sys.schemas ON
        tables.schema_id = schemas.schema_id
").ConfigureAwait(false);
    }

    public async Task<byte[]> GetCurrentVersionAsync(ConnectionStringModel sourceConnectionString, ChangeTrackingTable table)
    {
        using (var conn = _databaseExecutionService.CreateDbConnection(sourceConnectionString))
        {

            using (var cmd = _databaseExecutionService.CreateDbCommand(conn))
            {
                cmd.CommandText = $@"SELECT sys.fn_cdc_get_max_lsn()";

                return (await cmd.ExecuteScalarAsync().ConfigureAwait(false)) as byte[] ?? Array.Empty<byte>();

            }
        }
    }

    public async Task<int> GetTrackedTableChangesCountAsync(ConnectionStringModel sourceConnectionString, ChangeTrackingTable table, TableModel tableModel, byte[] currentVersion, ChangeTrackingMapping mapping)
    {
        var schemaName = table.SchemaName;
        var tableName = table.TableName;
        var columns = tableModel.Columns.OrderBy(i => i.ColumnId).Select(i => i.ColumnName).ToArray();
        var primaryKeyColumns = tableModel.Columns.Where(i => i.IsPrimaryKey).OrderBy(i => i.ColumnId).Select(i => i.ColumnName).ToArray();

        var pkColumns = primaryKeyColumns.Select(c => $"CT.[{c}]").ToList();
        var nonPkColumns = columns.Where(i => !primaryKeyColumns.Any(i2 => i2 == i)).Select(c => $"T.[{c}]").ToList();

        using (var conn = _databaseExecutionService.CreateDbConnection(sourceConnectionString))
        {
            using (var cmd = _databaseExecutionService.CreateDbCommand(conn))
            {
                if (mapping.IsNextSyncFull)
                {
                    cmd.CommandText = $@"
SELECT
    COUNT(*)
FROM
    [{schemaName}].[{tableName}] AS T
;
";
                }
                else
                {
                    cmd.CommandText = $@"
SELECT
    COUNT(*)
FROM
    cdc.fn_cdc_get_net_changes_{table.Instance}(@from_lsn , @to_lsn, 'all')
;
";

                    var lastSyncedVersion = mapping.LastSyncedVersion;

                    if (!lastSyncedVersion.Any(i => i != 0))
                    {
                        lastSyncedVersion = await GetMinVersionAsync(sourceConnectionString, table).ConfigureAwait(false);
                    }

                    cmd.Parameters.Add(new SqlParameter("@from_lsn", lastSyncedVersion));
                    cmd.Parameters.Add(new SqlParameter("@to_lsn", currentVersion));
                }

                var count = Convert.ToInt32(await cmd.ExecuteScalarAsync().ConfigureAwait(false));
                return count;
            }
        }

    }

    private async Task<byte[]> GetMinVersionAsync(ConnectionStringModel sourceConnectionString, ChangeTrackingTable table)
    {
        using (var conn = _databaseExecutionService.CreateDbConnection(sourceConnectionString))
        {
            using (var cmd = _databaseExecutionService.CreateDbCommand(conn))
            {
                cmd.CommandText = $@"SELECT sys.fn_cdc_get_min_lsn('{table.Instance}');";
                return (await cmd.ExecuteScalarAsync().ConfigureAwait(false)) as byte[] ?? Array.Empty<byte>();
            }
        }
    }

    public async IAsyncEnumerable<ChangeTrackingRow> GetTrackedTableChangesAsync(ConnectionStringModel sourceConnectionString, ChangeTrackingTable table, TableModel tableModel, byte[] currentVersion, ChangeTrackingMapping mapping)
    {
        var schemaName = table.SchemaName;
        var tableName = table.TableName;
        var columns = tableModel.Columns.OrderBy(i => i.ColumnId).Select(i => i.ColumnName).ToArray();
        var primaryKeyColumns = tableModel.Columns.Where(i => i.IsPrimaryKey).OrderBy(i => i.ColumnId).Select(i => i.ColumnName).ToArray();

        using (var conn = _databaseExecutionService.CreateDbConnection(sourceConnectionString))
        {
            using (var cmd = _databaseExecutionService.CreateDbCommand(conn))
            {
                if (mapping.IsNextSyncFull)
                {
                    cmd.CommandText = $@"
SELECT
    'I' SYS_CHANGE_OPERATION,
    {string.Join(", ", primaryKeyColumns.Select(i => $"T.[{i}]"))},
    {string.Join(", ", columns.Where(i => !primaryKeyColumns.Contains(i)).Select(i => $"T.[{i}]"))}
FROM
    [{schemaName}].[{tableName}] AS T
;
";
                }
                else
                {
                    cmd.CommandText = $@"
SELECT
    CASE WHEN CT.__$operation = 1 THEN 'D'
         WHEN CT.__$operation = 2 THEN 'I'
         WHEN CT.__$operation = 3 THEN 'U'
         WHEN CT.__$operation = 4 THEN 'U'
    END AS SYS_CHANGE_OPERATION,
    {string.Join(", ", primaryKeyColumns.Select(i => $"CT.[{i}]"))},
    {string.Join(", ", columns.Where(i => !primaryKeyColumns.Contains(i)).Select(i => $"CT.[{i}]"))}
FROM
    cdc.fn_cdc_get_net_changes_{table.Instance}(@from_lsn , @to_lsn, 'all') AS CT
;
";

                    var lastSyncedVersion = mapping.LastSyncedVersion;

                    if (!lastSyncedVersion.Any(i => i != 0))
                    {
                        lastSyncedVersion = await GetMinVersionAsync(sourceConnectionString, table).ConfigureAwait(false);
                    }

                    cmd.Parameters.Add(new SqlParameter("@from_lsn", lastSyncedVersion));
                    cmd.Parameters.Add(new SqlParameter("@to_lsn", currentVersion));

                }

                using (var reader = await cmd.ExecuteReaderAsync().ConfigureAwait(false))
                {
                    while (await reader.ReadAsync().ConfigureAwait(false))
                    {
                        var row = new ChangeTrackingRow
                        {
                            SchemaName = schemaName,
                            TableName = tableName,
                            ChangeOperation = reader.GetString(0),
                            ColumnValues = columns.ToDictionary(c => c, c => reader[c] == DBNull.Value ? null : reader[c])
                        };
                        yield return row;
                    }
                }
            }
        }
    }

    private Task UpgradeTableAsync(TableModel sourceTable, TableModel currentTable, DbConnection dbConnection)
    {
        var provider = _databaseFactory.GetDatabaseProvider(dbConnection);

        var newColumns = sourceTable.Columns.Where(i => !currentTable.Columns.Any(i2 => i.ColumnName == i2.ColumnName)).ToList();
        var removedColumns = currentTable.Columns.Where(i => !sourceTable.Columns.Any(i2 => i.ColumnName == i2.ColumnName)).ToList();

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

        return Task.CompletedTask;
    }

}
