#nullable enable

using Temelie.Database.Models;
using Temelie.Database.Models.ChangeTracking;

namespace Temelie.Database.Providers;

public interface IChangeTrackingProvider
{
    string Provider { get; }

    Task DetectChangesAsync(ConnectionStringModel sourceConnectionString);
    Task<IEnumerable<ChangeTrackingTable>> GetTrackedTablesAsync(ConnectionStringModel sourceConnectionString);
    Task<IEnumerable<ChangeTrackingMapping>> GetMappingsAsync(string source, ConnectionStringModel targetConnectionString);

    Task<int> GetTrackedTableChangesCountAsync(ConnectionStringModel sourceConnectionString, ChangeTrackingTable table, TableModel tableModel, ChangeTrackingMapping mapping);
    IAsyncEnumerable<ChangeTrackingRow> GetTrackedTableChangesAsync(ConnectionStringModel sourceConnectionString, ChangeTrackingTable table, TableModel tableModel, ChangeTrackingMapping mapping);

    Task ApplyChangesAsync(ConnectionStringModel targetConnectionString, ChangeTrackingTable table, TableModel tableModel, ChangeTrackingMapping mapping, IAsyncEnumerable<ChangeTrackingRow> changes, int count);
    Task UpdateSyncedVersionAsync(ConnectionStringModel targetConnectionString, int changeTrackingMappingId, byte[] version);
    Task FlagSyncingAsync(ConnectionStringModel targetConnectionString, int changeTrackingMappingId, bool isSyncing);
}
