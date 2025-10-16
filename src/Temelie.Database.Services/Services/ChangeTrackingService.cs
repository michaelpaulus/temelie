using System.Transactions;
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

    public ChangeTrackingService(IEnumerable<IChangeTrackingProvider> changeTrackingProviders)
    {
        _changeTrackingProviders = changeTrackingProviders;
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

    public async Task SyncChangesAsync(ConnectionStringModel sourceConnectionString, ConnectionStringModel targetConnectionString)
    {
        var sourceDatabaseSyncProvider = GetDatabaseSyncProvider(sourceConnectionString);
        var tables = await sourceDatabaseSyncProvider.GetTrackedTablesAsync(sourceConnectionString).ConfigureAwait(false);
        foreach (var table in tables)
        {
            await SyncChangesAsync(sourceConnectionString, targetConnectionString, table).ConfigureAwait(false);
        }
    }

    public async Task SyncChangesAsync(ConnectionStringModel sourceConnectionString, ConnectionStringModel targetConnectionString, TrackedTable table)
    {
        var sourceDatabaseSyncProvider = GetDatabaseSyncProvider(sourceConnectionString);
        var targetDatabaseSyncProvider = GetDatabaseSyncProvider(targetConnectionString);

        var mapping = await targetDatabaseSyncProvider.GetMappingAsync(targetConnectionString, table).ConfigureAwait(false);

        if (mapping is null)
        {
            return;
        }

        var changes = await sourceDatabaseSyncProvider.GetTrackedTablesChangesAsync(sourceConnectionString, targetConnectionString, table, mapping).ConfigureAwait(false);

        if (changes.Any())
        {
            changes = changes.OrderBy(i => i.ChangeVersion).ToList();
            using (var ts = new TransactionScope(TransactionScopeAsyncFlowOption.Enabled))
            {
                await targetDatabaseSyncProvider.ApplyChangesAsync(targetConnectionString, table, mapping, changes).ConfigureAwait(false);
                ts.Complete();
            }
        }

    }

}
