#nullable enable

using Temelie.Database.Models;
using Temelie.Database.Models.ChangeTracking;

namespace Temelie.Database.Providers;

public interface IChangeTrackingTargetProvider
{
    string Provider { get; }
    Task<IEnumerable<ChangeTrackingMapping>> GetMappingsAsync(int sourceId, ConnectionStringModel targetConnectionString);
    Task ApplyChangesAsync(ConnectionStringModel targetConnectionString, ChangeTrackingTable table, TableModel tableModel, ChangeTrackingMapping mapping, IAsyncEnumerable<ChangeTrackingRow> changes, int count);
    Task UpdateSyncedVersionAsync(ConnectionStringModel targetConnectionString, int changeTrackingMappingId, byte[] version);
    Task FlagSyncingAsync(ConnectionStringModel targetConnectionString, ChangeTrackingMapping mapping, bool isSyncing);
}
