using System.Text.Json;
using System.Transactions;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Temelie.Database.Models;
using Temelie.Database.Models.ChangeTracking;
using Temelie.Database.Services;
using Temelie.DependencyInjection;

namespace Temelie.Database.Providers;

[ExportProvider(typeof(IChangeTrackingProvider))]
public class MssqlDatabaseSyncProvider : IChangeTrackingProvider
{
    private readonly IDatabaseExecutionService _databaseExecutionService;

    public MssqlDatabaseSyncProvider(IConfiguration configuration, IDatabaseExecutionService databaseExecutionService)
    {
        _databaseExecutionService = databaseExecutionService;
    }

    public string Provider => nameof(SqlConnection);

    public async Task<IEnumerable<ChangeTrackingTable>> GetTrackedTablesAsync(ConnectionStringModel sourceConnectionString)
    {
        return await GetRecordsAsync<ChangeTrackingTable>(sourceConnectionString, @"
SELECT
    schemas.name AS SchemaName,
    tables.name AS TableName,
    (SELECT
         name AS Name,
         system_type_id AS SystemTypeId,
         column_id AS ColumnId,
         max_length AS MaxLength,
         precision AS Precision,
         scale AS Scale,
         is_nullable AS IsNullable,
         is_computed AS IsComputed
     FROM
         sys.columns
     WHERE
          columns.object_id = tables.object_id
     FOR JSON AUTO) AS ColumnsJSON,
    (SELECT
         columns.name AS Name,
         index_columns.column_id AS ColumnId,
         index_columns.key_ordinal AS KeyOrdinal
     FROM
         sys.indexes INNER JOIN
         sys.index_columns ON
             indexes.object_id = index_columns.object_id AND
             indexes.index_id = index_columns.index_id INNER JOIN
         sys.columns ON
             index_columns.object_id = columns.object_id AND
             index_columns.column_id = columns.column_id
     WHERE
          indexes.object_id = tables.object_id AND
          indexes.is_primary_key = 1
     FOR JSON AUTO) AS PkColumnsJSON,
    CHANGE_TRACKING_CURRENT_VERSION() AS CurrentVersionId
FROM
    sys.change_tracking_tables INNER JOIN
    sys.tables ON
        change_tracking_tables.object_id = tables.object_id INNER JOIN
    sys.schemas ON
        tables.schema_id = schemas.schema_id;
").ConfigureAwait(false);
    }

    public async IAsyncEnumerable<ChangeTrackingRow> GetTrackedTableChangesAsync(ConnectionStringModel sourceConnectionString, ConnectionStringModel targetConnectionString, ChangeTrackingTable table, long previousVersionId)
    {
        var schemaName = table.SchemaName;
        var tableName = table.TableName;
        var columns = GetColumns(table.ColumnsJSON).OrderBy(i => i.ColumnId).Select(i => i.Name).ToArray();
        var primaryKeyColumns = GetPkColumns(table.PkColumnsJSON).OrderBy(i => i.KeyOrdinal).Select(i => i.Name).ToArray();

        var pkColumns = primaryKeyColumns.Select(c => $"CT.[{c}]").ToList();
        var nonPkColumns = columns.Where(i => !primaryKeyColumns.Any(i2 => i2 == i)).Select(c => $"T.[{c}]").ToList();

        using (var conn = _databaseExecutionService.CreateDbConnection(sourceConnectionString))
        using (var cmd = _databaseExecutionService.CreateDbCommand(conn))
        {
            cmd.CommandText = $@"
SELECT
    CT.SYS_CHANGE_VERSION,
    CT.SYS_CHANGE_OPERATION,
    {string.Join(", ", pkColumns)}{(nonPkColumns.Any() ? "," : "")}
    {string.Join(", ", nonPkColumns)}
FROM
    CHANGETABLE(CHANGES [{schemaName}].[{tableName}], {previousVersionId}) AS CT LEFT JOIN
    [{schemaName}].[{tableName}] AS T ON {string.Join(" AND ", primaryKeyColumns.Select(c => $"CT.[{c}] = T.[{c}]"))}
ORDER BY
    CT.SYS_CHANGE_VERSION
;
";
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

    public async Task<ChangeTrackingMapping?> GetMappingAsync(ConnectionStringModel targetConnectionString, ChangeTrackingTable table)
    {
        return (await GetRecordsAsync<ChangeTrackingMapping>(targetConnectionString, @"
SELECT
    ChangeTrackingMappingId,
    SourceSchemaName,
    SourceTableName,
    TargetSchemaName,
    TargetTableName,
    LastSyncedVersion,
    CreatedDate,
    CreatedBy,
    ModifiedDate,
    ModifiedBy
FROM
    ChangeTrackingMappings
WHERE
    SourceSchemaName = @SourceSchemaName AND
    SourceTableName = @SourceTableName 
",
    new SqlParameter("@SourceSchemaName", table.SchemaName),
    new SqlParameter("@SourceTableName", table.TableName)).ConfigureAwait(false)).FirstOrDefault();
    }

    public async Task ApplyChangesAsync(ConnectionStringModel targetConnectionString, ChangeTrackingTable table, ChangeTrackingMapping mapping, IEnumerable<ChangeTrackingRow> changes)
    {
        using (var ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
        {
            string getColumnValue(object? value)
            {
                if (value is JsonElement jsonElement)
                {
                    value = jsonElement.ValueKind switch
                    {
                        JsonValueKind.String => jsonElement.GetString(),
                        JsonValueKind.Number => jsonElement.GetDecimal(),
                        JsonValueKind.True => true,
                        JsonValueKind.False => false,
                        JsonValueKind.Null => null,
                        _ => throw new InvalidOperationException($"Unsupported JSON value kind: {jsonElement.ValueKind}")
                    };
                }

                if (value == null)
                {
                    return "NULL";
                }
                if (value is string stringValue)
                {
                    return $"'{stringValue.Replace("'", "''")}'";
                }
                if (value is Guid guidValue)
                {
                    return $"'{guidValue}'";
                }
                if (value is DateTime dateValue)
                {
                    return $"'{dateValue.ToString("yyyy-MM-ddTHH:mm:ss")}'";
                }
                if (value is bool boolValue)
                {
                    return $"{(boolValue ? "1" : "0")}";
                }

                return value.ToString()!;
            }

            string generateUpsert(ChangeTrackingRow change)
            {
                var data = change.ColumnValues;
                var columns = GetColumns(table.ColumnsJSON).Select(i => i.Name).ToArray();
                var pkColumns = GetPkColumns(table.PkColumnsJSON).Select(i => i.Name).ToArray();

                return @$"
MERGE INTO [{mapping.TargetSchemaName}].[{mapping.TargetTableName}]
USING (VALUES (
{string.Join(", ", columns.Select(c => getColumnValue(data[c])))}
)) AS source ({string.Join(", ", data.Keys)})
ON {string.Join(" AND ", pkColumns.Select(c => $"[{mapping.TargetTableName}].[{c}] = source.{c}"))}
WHEN MATCHED THEN
 UPDATE
SET
    {string.Join(", ", data.Where(i => !pkColumns.Any(i2 => i2 == i.Key)).Select(i2 => string.Join(", ", $"{i2.Key} = {getColumnValue(i2.Value)}")))}
WHEN NOT MATCHED THEN
 INSERT ({string.Join(", ", data.Keys)})
 VALUES ({string.Join(", ", data.Select(i => getColumnValue(i.Value)))});
";
            }

            string generateDelete(ChangeTrackingRow change)
            {
                var data = change.ColumnValues;
                return @$"
DELETE FROM [{mapping.TargetSchemaName}].[{mapping.TargetTableName}]
WHERE {string.Join(" AND ", GetPkColumns(table.PkColumnsJSON).Select(c => $"[{c.Name}] = {getColumnValue(data[c.Name])}"))}
";
            }

            foreach (var change in changes)
            {
                using (var conn = _databaseExecutionService.CreateDbConnection(targetConnectionString))
                using (var cmd = _databaseExecutionService.CreateDbCommand(conn))
                {
                    cmd.CommandText = $@"
{change.ChangeOperation switch
                    {
                        "I" => generateUpsert(change),
                        "U" => generateUpsert(change),
                        "D" => generateDelete(change),
                        _ => throw new InvalidOperationException($"Unknown change operation: {change.ChangeOperation}")
                    }}
";
                    await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
                }

            }

            ts.Complete();
        }
    }

    private static ChangeTrackingColumn[] GetColumns(string columnsJSON)
    {
        return string.IsNullOrWhiteSpace(columnsJSON) ? Array.Empty<ChangeTrackingColumn>() : JsonSerializer.Deserialize<ChangeTrackingColumn[]>(columnsJSON)!;
    }

    private static ChangeTrackingPkColumn[] GetPkColumns(string pkColumnsJSON)
    {
        return string.IsNullOrWhiteSpace(pkColumnsJSON) ? Array.Empty<ChangeTrackingPkColumn>() : JsonSerializer.Deserialize<ChangeTrackingPkColumn[]>(pkColumnsJSON)!;
    }

    private async Task<IEnumerable<T>> GetRecordsAsync<T>(ConnectionStringModel connectionString, string query, params SqlParameter[] parameters)
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

    public async Task UpdateSyncedVersionAsync(ConnectionStringModel targetConnectionString, ChangeTrackingTable table, long version)
    {
        var mapping = await GetMappingAsync(targetConnectionString, table).ConfigureAwait(false);
        if (mapping is null)
        {
            throw new InvalidOperationException("Mapping not found when updating synced version.");
        }
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
            cmd.Parameters.Add(new SqlParameter("@ChangeTrackingMappingId", mapping.ChangeTrackingMappingId));
            await cmd.ExecuteNonQueryAsync().ConfigureAwait(false);
            ts.Complete();
        }
    }

}
