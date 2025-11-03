using Temelie.Database.Extensions;
using Temelie.Database.Models;
using Temelie.Database.Models.ChangeTracking;
using Temelie.Database.Providers;
using Temelie.DependencyInjection;

namespace Temelie.Database.Services;

[ExportTransient(typeof(IChangeTrackingService))]
public class ChangeTrackingService : IChangeTrackingService
{
    private readonly IEnumerable<IChangeTrackingProvider> _changeTrackingProviders;
    private readonly IDatabaseModelService _databaseModelService;
    private readonly IDatabaseExecutionService _databaseExecutionService;

    public ChangeTrackingService(IEnumerable<IChangeTrackingProvider> changeTrackingProviders,
        IDatabaseModelService databaseModelService,
        IDatabaseExecutionService databaseExecutionService)
    {
        _changeTrackingProviders = changeTrackingProviders;
        _databaseModelService = databaseModelService;
        _databaseExecutionService = databaseExecutionService;
    }

    private IChangeTrackingProvider GetDatabaseSyncProvider(ConnectionStringModel connectionString)
    {
        var databaseProvider = _changeTrackingProviders.FirstOrDefault(i => i.Provider.EqualsIgnoreCase(connectionString.DatabaseProviderName));
        if (databaseProvider is null)
        {
            throw new InvalidOperationException("No database provider found for the configured database.");
        }
        return databaseProvider;
    }

    public async Task DetectChangesAsync(ConnectionStringModel sourceConnectionString)
    {
        var sourceDatabaseSyncProvider = GetDatabaseSyncProvider(sourceConnectionString);
        await sourceDatabaseSyncProvider.DetectChangesAsync(sourceConnectionString).ConfigureAwait(false);
    }

    public async Task<IEnumerable<ChangeTrackingTableAndMapping>> GetTrackedTablesAndMappingsAsync(string source, ConnectionStringModel sourceConnectionString, ConnectionStringModel targetConnectionString)
    {
        var list = new List<ChangeTrackingTableAndMapping>();

        var sourceDatabaseSyncProvider = GetDatabaseSyncProvider(sourceConnectionString);
        var targetDatabaseSyncProvider = GetDatabaseSyncProvider(targetConnectionString);
        var tables = await sourceDatabaseSyncProvider.GetTrackedTablesAsync(sourceConnectionString).ConfigureAwait(false);
        var mappings = await targetDatabaseSyncProvider.GetMappingsAsync(source, targetConnectionString).ConfigureAwait(false);
        foreach (var table in tables)
        {
            var mapping = mappings.Where(i => i.SourceTableName.EqualsIgnoreCase(table.TableName) && i.SourceSchemaName.EqualsIgnoreCase(table.SchemaName)).FirstOrDefault();
            if (mapping is not null)
            {
                list.Add(new ChangeTrackingTableAndMapping { Table = table, Mapping = mapping });
            }
        }
        return list;
    }

    public async Task SyncChangesAsync(string source, ConnectionStringModel sourceConnectionString, ConnectionStringModel targetConnectionString)
    {
        var mappings = await GetTrackedTablesAndMappingsAsync(source, sourceConnectionString, targetConnectionString).ConfigureAwait(false);
        foreach (var mapping in mappings)
        {
            await SyncChangesAsync(sourceConnectionString, targetConnectionString, mapping.Table, mapping.Mapping).ConfigureAwait(false);
        }
    }

    public async Task SyncChangesAsync(ConnectionStringModel sourceConnectionString, ConnectionStringModel targetConnectionString, ChangeTrackingTable table, ChangeTrackingMapping mapping)
    {
        var sourceDatabaseSyncProvider = GetDatabaseSyncProvider(sourceConnectionString);
        var targetDatabaseSyncProvider = GetDatabaseSyncProvider(targetConnectionString);

        TableModel sourceTable = null;

        using (var conn = _databaseExecutionService.CreateDbConnection(sourceConnectionString))
        {
            var columns = _databaseModelService.GetTableColumns(conn);
            var tables = _databaseModelService.GetTables(conn, new DatabaseModelOptions(), columns);
            sourceTable = tables.Where(i => i.TableName.EqualsIgnoreCase(table.TableName) && i.SchemaName.EqualsIgnoreCase(table.SchemaName)).FirstOrDefault();
        }

        if (sourceTable is null)
        {
            throw new Exception("Source table not found in database model.");
        }

        var changeCount = await sourceDatabaseSyncProvider.GetTrackedTableChangesCountAsync(sourceConnectionString, table, sourceTable, mapping).ConfigureAwait(false);

        if (changeCount > 0)
        {
            var changes = sourceDatabaseSyncProvider.GetTrackedTableChangesAsync(sourceConnectionString, table, sourceTable, mapping);

            await targetDatabaseSyncProvider.ApplyChangesAsync(targetConnectionString, table, sourceTable, mapping, changes, changeCount).ConfigureAwait(false);

            await targetDatabaseSyncProvider.UpdateSyncedVersionAsync(targetConnectionString, mapping.ChangeTrackingMappingId, table.CurrentVersion).ConfigureAwait(false);
        }

        await FlagSyncingAsync(targetConnectionString, mapping.ChangeTrackingMappingId, false).ConfigureAwait(false);
    }

    public async Task FlagSyncingAsync(ConnectionStringModel targetConnectionString, int changeTrackingMappingId, bool isSyncing)
    {
        var targetDatabaseSyncProvider = GetDatabaseSyncProvider(targetConnectionString);
        await targetDatabaseSyncProvider.FlagSyncingAsync(targetConnectionString, changeTrackingMappingId, isSyncing).ConfigureAwait(false);
    }

}
