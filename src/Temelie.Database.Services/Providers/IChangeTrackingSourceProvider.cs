#nullable enable

using Temelie.Database.Models;
using Temelie.Database.Models.ChangeTracking;

namespace Temelie.Database.Providers;

public interface IChangeTrackingSourceProvider
{
    string Provider { get; }

    Task DetectChangesAsync(ConnectionStringModel sourceConnectionString);
    Task<IEnumerable<ChangeTrackingTable>> GetTrackedTablesAsync(ConnectionStringModel sourceConnectionString);
    Task<byte[]> GetCurrentVersionAsync(ConnectionStringModel sourceConnectionString, ChangeTrackingTable table);
    Task<int> GetTrackedTableChangesCountAsync(ConnectionStringModel sourceConnectionString, ChangeTrackingTable table, TableModel tableModel, byte[] currentVersion, ChangeTrackingMapping mapping);
    IAsyncEnumerable<ChangeTrackingRow> GetTrackedTableChangesAsync(ConnectionStringModel sourceConnectionString, ChangeTrackingTable table, TableModel tableModel, byte[] currentVersion, ChangeTrackingMapping mapping);
}
