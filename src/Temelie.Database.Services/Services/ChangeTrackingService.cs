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

    public ChangeTrackingService(IEnumerable<IChangeTrackingProvider> changeTrackingProviders,
        IDatabaseModelService databaseModelService)
    {
        _changeTrackingProviders = changeTrackingProviders;
        _databaseModelService = databaseModelService;
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

    public Task DetectChangesAsync(ConnectionStringModel sourceConnectionString)
    {
        return Task.CompletedTask;
    }

    public async Task SyncChangesAsync(ConnectionStringModel sourceConnectionString, ConnectionStringModel targetConnectionString)
    {
        var sourceDatabaseSyncProvider = GetDatabaseSyncProvider(sourceConnectionString);
        var tables = await sourceDatabaseSyncProvider.GetTrackedTablesAsync(sourceConnectionString).ConfigureAwait(false);
        foreach (var table in tables)
        {
            await SyncChangesAsync(sourceConnectionString, targetConnectionString, table).ConfigureAwait(false);
        }
    }

    public async Task SyncChangesAsync(ConnectionStringModel sourceConnectionString, ConnectionStringModel targetConnectionString, ChangeTrackingTable table)
    {
        var sourceDatabaseSyncProvider = GetDatabaseSyncProvider(sourceConnectionString);
        var targetDatabaseSyncProvider = GetDatabaseSyncProvider(targetConnectionString);

        var mapping = await targetDatabaseSyncProvider.GetMappingAsync(targetConnectionString, table).ConfigureAwait(false);

        if (mapping is null)
        {
            return;
        }

        var databaseModel = _databaseModelService.CreateModel(sourceConnectionString);

        var sourceTable = databaseModel.Tables.Where(i => i.TableName == table.TableName && i.SchemaName == table.SchemaName).FirstOrDefault();

        if (sourceTable is null)
        {
            throw new Exception("Source table not found in database model.");
        }

        var changeCount = await sourceDatabaseSyncProvider.GetTrackedTableChangesCountAsync(sourceConnectionString, targetConnectionString, table, sourceTable, mapping.LastSyncedVersion).ConfigureAwait(false);

        if (changeCount > 0)
        {
            var changes = sourceDatabaseSyncProvider.GetTrackedTableChangesAsync(sourceConnectionString, targetConnectionString, table, sourceTable, mapping.LastSyncedVersion);

            await targetDatabaseSyncProvider.ApplyChangesAsync(targetConnectionString, table, sourceTable, mapping, changes, changeCount).ConfigureAwait(false);

            await targetDatabaseSyncProvider.UpdateSyncedVersionAsync(targetConnectionString, table, table.CurrentVersion.GetValueOrDefault()).ConfigureAwait(false);
        }

    }

}
