using System.Data;
using Microsoft.Data.SqlClient;
using Temelie.Database.Models;
using Temelie.Database.Models.ChangeTracking;
using Temelie.Database.Services;
using Temelie.DependencyInjection;

namespace Temelie.Database.Providers;

[ExportProvider(typeof(IChangeTrackingSourceProvider))]
public class MssqlChangeDataCaptureSourceProvider : IChangeTrackingSourceProvider
{
    private readonly IDatabaseExecutionService _databaseExecutionService;

    public MssqlChangeDataCaptureSourceProvider(IDatabaseExecutionService databaseExecutionService)
    {
        _databaseExecutionService = databaseExecutionService;
    }

    public string Provider => $"mssql_cdc";

    public Task DetectChangesAsync(ConnectionStringModel sourceConnectionString)
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
    cdc.fn_cdc_get_all_changes_{table.Instance}(@from_lsn , @to_lsn, 'all')
;
";
                    cmd.Parameters.Add(new SqlParameter("@from_lsn", mapping.LastSyncedVersion));
                    cmd.Parameters.Add(new SqlParameter("@to_lsn", currentVersion));
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
    CASE WHEN CT.__$operation = 1 THEN 'D'
         WHEN CT.__$operation = 2 THEN 'I'
         WHEN CT.__$operation = 3 THEN 'U'
         WHEN CT.__$operation = 4 THEN 'U'
    END AS SYS_CHANGE_OPERATION,
    {string.Join(", ", primaryKeyColumns.Select(i => $"CT.[{i}]"))},
    {string.Join(", ", columns.Where(i => !primaryKeyColumns.Contains(i)).Select(i => $"CT.[{i}]"))}
FROM
    cdc.fn_cdc_get_all_changes_{table.Instance}(@from_lsn , @to_lsn, 'all') AS CT
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
                            ChangeVersion = reader.GetInt64(0),
                            ChangeOperation = reader.GetString(1),
                            ColumnValues = columns.ToDictionary(c => c, c => reader[c] == DBNull.Value ? null : reader[c])
                        };
                        yield return row;
                    }
                }
            }
        }
    }

}
