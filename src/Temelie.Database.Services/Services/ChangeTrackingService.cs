using Temelie.Database.Extensions;
using Temelie.Database.Models;
using Temelie.Database.Models.ChangeTracking;
using Temelie.Database.Providers;
using Temelie.DependencyInjection;

namespace Temelie.Database.Services;

[ExportTransient(typeof(IChangeTrackingService))]
public class ChangeTrackingService : IChangeTrackingService
{
    private readonly IDatabaseModelService _databaseModelService;
    private readonly IDatabaseExecutionService _databaseExecutionService;
    private readonly IEnumerable<IChangeTrackingSourceProvider> _changeTrackingSourceProviders;
    private readonly IEnumerable<IChangeTrackingTargetProvider> _changeTrackingTargetProviders;

    public ChangeTrackingService(
        IDatabaseModelService databaseModelService,
        IDatabaseExecutionService databaseExecutionService,
        IEnumerable<IChangeTrackingSourceProvider> changeTrackingSourceProviders,
        IEnumerable<IChangeTrackingTargetProvider> changeTrackingTargetProviders)
    {
        _databaseModelService = databaseModelService;
        _databaseExecutionService = databaseExecutionService;
        _changeTrackingSourceProviders = changeTrackingSourceProviders;
        _changeTrackingTargetProviders = changeTrackingTargetProviders;
    }

    private IChangeTrackingSourceProvider GetSourceDatabaseSyncProvider(ChangeTrackingMapping mapping)
    {
        var databaseProvider = _changeTrackingSourceProviders.FirstOrDefault(i => i.Provider.EqualsIgnoreCase(mapping.SourceProvider));
        if (databaseProvider is null)
        {
            throw new InvalidOperationException("No database provider found for the configured database.");
        }
        return databaseProvider;
    }

    private IChangeTrackingTargetProvider GetTargetDatabaseSyncProvider(ConnectionStringModel connectionString)
    {
        var databaseProvider = _changeTrackingTargetProviders.FirstOrDefault(i => i.Provider.EqualsIgnoreCase(connectionString.DatabaseProviderName));
        if (databaseProvider is null)
        {
            throw new InvalidOperationException("No database provider found for the configured database.");
        }
        return databaseProvider;
    }

    public async Task UpdateSchemaAsync(ConnectionStringModel sourceConnectionString, string sourceProvider)
    {
        var provider = _changeTrackingSourceProviders.FirstOrDefault(i => i.Provider.EqualsIgnoreCase(sourceProvider));

        if (provider is null)
        {
            throw new InvalidOperationException($"No database provider found for {sourceProvider}.");
        }

        var tables = await provider.GetTrackedTablesAsync(sourceConnectionString).ConfigureAwait(false);
        foreach (var table in tables)
        {
            await provider.UpdateSchemaAsync(sourceConnectionString, table).ConfigureAwait(false);
        }

    }

    public async Task DetectChangesAsync(ConnectionStringModel sourceConnectionString, ChangeTrackingTable table, ChangeTrackingMapping mapping)
    {
        var sourceDatabaseSyncProvider = GetSourceDatabaseSyncProvider(mapping);
        await sourceDatabaseSyncProvider.DetectChangesAsync(sourceConnectionString, table, mapping).ConfigureAwait(false);
    }

    public async Task<IEnumerable<ChangeTrackingTableAndMapping>> GetTrackedTablesAndMappingsAsync(int sourceId, ConnectionStringModel sourceConnectionString, ConnectionStringModel targetConnectionString)
    {
        var list = new List<ChangeTrackingTableAndMapping>();

        var targetDatabaseSyncProvider = GetTargetDatabaseSyncProvider(targetConnectionString);

        var mappings = await targetDatabaseSyncProvider.GetMappingsAsync(sourceId, targetConnectionString).ConfigureAwait(false);

        foreach (var group in mappings.GroupBy(i => new { i.SourceProvider }))
        {
            var sourceDatabaseSyncProvider = GetSourceDatabaseSyncProvider(group.First());
            var tables = await sourceDatabaseSyncProvider.GetTrackedTablesAsync(sourceConnectionString).ConfigureAwait(false);
            foreach (var table in tables)
            {
                var mapping = mappings.Where(i => i.SourceTableName.EqualsIgnoreCase(table.TableName) && i.SourceSchemaName.EqualsIgnoreCase(table.SchemaName)).FirstOrDefault();
                if (mapping is not null)
                {
                    list.Add(new ChangeTrackingTableAndMapping { Table = table, Mapping = mapping });
                }
            }
        }

        return list;
    }

    public async Task SyncChangesAsync(int sourceId, ConnectionStringModel sourceConnectionString, ConnectionStringModel targetConnectionString)
    {
        var mappings = await GetTrackedTablesAndMappingsAsync(sourceId, sourceConnectionString, targetConnectionString).ConfigureAwait(false);
        foreach (var mapping in mappings)
        {
            await SyncChangesAsync(sourceConnectionString, targetConnectionString, mapping.Table, mapping.Mapping).ConfigureAwait(false);
        }
    }

    public async Task SyncChangesAsync(ConnectionStringModel sourceConnectionString, ConnectionStringModel targetConnectionString, ChangeTrackingTable table, ChangeTrackingMapping mapping)
    {
        var sourceDatabaseSyncProvider = GetSourceDatabaseSyncProvider(mapping);
        var targetDatabaseSyncProvider = GetTargetDatabaseSyncProvider(targetConnectionString);

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

        var currentVersion = await sourceDatabaseSyncProvider.GetCurrentVersionAsync(sourceConnectionString, table).ConfigureAwait(false);

        var changeCount = await sourceDatabaseSyncProvider.GetTrackedTableChangesCountAsync(sourceConnectionString, table, sourceTable, currentVersion, mapping).ConfigureAwait(false);

        if (changeCount > 0)
        {
            var changes = sourceDatabaseSyncProvider.GetTrackedTableChangesAsync(sourceConnectionString, table, sourceTable, currentVersion, mapping);

            await targetDatabaseSyncProvider.ApplyChangesAsync(targetConnectionString, table, sourceTable, mapping, changes, changeCount).ConfigureAwait(false);

            await targetDatabaseSyncProvider.UpdateSyncedVersionAsync(targetConnectionString, mapping.ChangeTrackingMappingId, currentVersion).ConfigureAwait(false);
        }

        await FlagSyncingAsync(targetConnectionString, mapping.ChangeTrackingMappingId, false).ConfigureAwait(false);
    }

    public async Task FlagSyncingAsync(ConnectionStringModel targetConnectionString, int changeTrackingMappingId, bool isSyncing)
    {
        var targetDatabaseSyncProvider = GetTargetDatabaseSyncProvider(targetConnectionString);
        await targetDatabaseSyncProvider.FlagSyncingAsync(targetConnectionString, changeTrackingMappingId, isSyncing).ConfigureAwait(false);
    }

}
