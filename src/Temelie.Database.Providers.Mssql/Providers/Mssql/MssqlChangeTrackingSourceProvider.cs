using System.Data;
using Temelie.Database.Models;
using Temelie.Database.Models.ChangeTracking;
using Temelie.Database.Services;
using Temelie.DependencyInjection;

namespace Temelie.Database.Providers;

[ExportProvider(typeof(IChangeTrackingSourceProvider))]
public class MssqlChangeTrackingSourceProvider : IChangeTrackingSourceProvider
{
    private readonly IDatabaseExecutionService _databaseExecutionService;

    public MssqlChangeTrackingSourceProvider(IDatabaseExecutionService databaseExecutionService)
    {
        _databaseExecutionService = databaseExecutionService;
    }

    public string Provider => "mssql_ct";

    public Task DetectChangesAsync(ConnectionStringModel sourceConnectionString)
    {
        return Task.CompletedTask;
    }

    public async Task<IEnumerable<ChangeTrackingTable>> GetTrackedTablesAsync(ConnectionStringModel sourceConnectionString)
    {
        return await _databaseExecutionService.GetRecordsAsync<ChangeTrackingTable>(sourceConnectionString, @"
SELECT
    schemas.name AS SchemaName,
    tables.name AS TableName
FROM
    sys.change_tracking_tables INNER JOIN
    sys.tables ON
        change_tracking_tables.object_id = tables.object_id INNER JOIN
    sys.schemas ON
        tables.schema_id = schemas.schema_id;
").ConfigureAwait(false);
    }

     public async Task<byte[]> GetCurrentVersionAsync(ConnectionStringModel sourceConnectionString, ChangeTrackingTable table)
    {

        using (var conn = _databaseExecutionService.CreateDbConnection(sourceConnectionString))
        {
            using (var cmd = _databaseExecutionService.CreateDbCommand(conn))
            {
                cmd.CommandText = @"
SELECT CONVERT(BINARY(10), CHANGE_TRACKING_CURRENT_VERSION())
";

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
    CHANGETABLE(CHANGES [{schemaName}].[{tableName}], {ConvertVersion(mapping.LastSyncedVersion)}) AS CT LEFT JOIN
    [{schemaName}].[{tableName}] AS T ON {string.Join(" AND ", primaryKeyColumns.Select(c => $"CT.[{c}] = T.[{c}]"))}
;
";
                }

                var count = Convert.ToInt32(await cmd.ExecuteScalarAsync().ConfigureAwait(false));
                return count;
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
    CT.SYS_CHANGE_OPERATION,
    {string.Join(", ", primaryKeyColumns.Select(i => $"CT.[{i}]"))},
    {string.Join(", ", columns.Where(i => !primaryKeyColumns.Contains(i)).Select(i => $"T.[{i}]"))}
FROM
    CHANGETABLE(CHANGES [{schemaName}].[{tableName}], {ConvertVersion(mapping.LastSyncedVersion)}) AS CT LEFT JOIN
    [{schemaName}].[{tableName}] AS T ON {string.Join(" AND ", primaryKeyColumns.Select(c => $"CT.[{c}] = T.[{c}]"))}
ORDER BY
    CT.SYS_CHANGE_VERSION
;
";
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

    private long ConvertVersion(byte[] version)
    {
        var testArray = version.Reverse().ToArray();
        return BitConverter.ToInt64(testArray);
    }

}
