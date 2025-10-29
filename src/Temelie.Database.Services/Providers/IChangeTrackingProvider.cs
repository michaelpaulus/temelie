#nullable enable

using Temelie.Database.Models;
using Temelie.Database.Models.ChangeTracking;

namespace Temelie.Database.Providers;

public interface IChangeTrackingProvider
{
    string Provider { get; }

    Task<IEnumerable<ChangeTrackingTable>> GetTrackedTablesAsync(ConnectionStringModel sourceConnectionString);
    Task<IEnumerable<ChangeTrackingMapping>> GetMappingsAsync(ConnectionStringModel targetConnectionString);

    Task<int> GetTrackedTableChangesCountAsync(ConnectionStringModel sourceConnectionString, ConnectionStringModel targetConnectionString, ChangeTrackingTable table, TableModel tableModel, byte[] previousVersion);
    IAsyncEnumerable<ChangeTrackingRow> GetTrackedTableChangesAsync(ConnectionStringModel sourceConnectionString, ConnectionStringModel targetConnectionString, ChangeTrackingTable table, TableModel tableModel, byte[] previousVersion);

    Task ApplyChangesAsync(ConnectionStringModel targetConnectionString, ChangeTrackingTable table, TableModel tableModel, ChangeTrackingMapping mapping, IAsyncEnumerable<ChangeTrackingRow> changes, int count);
    Task UpdateSyncedVersionAsync(ConnectionStringModel targetConnectionString, ChangeTrackingTable table, ChangeTrackingMapping mapping, byte[] version);
}
