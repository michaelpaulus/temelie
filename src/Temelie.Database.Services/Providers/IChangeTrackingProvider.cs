#nullable enable

using Temelie.Database.Models;
using Temelie.Database.Models.ChangeTracking;

namespace Temelie.Database.Providers;

public interface IChangeTrackingProvider
{
    string Provider { get; }

    Task<IEnumerable<ChangeTrackingTable>> GetTrackedTablesAsync(ConnectionStringModel sourceConnectionString);

    Task<ChangeTrackingMapping?> GetMappingAsync(ConnectionStringModel targetConnectionString, ChangeTrackingTable table);

    Task<IEnumerable<ChangeTrackingRow>> GetTrackedTableChangesAsync(ConnectionStringModel sourceConnectionString, ConnectionStringModel targetConnectionString, ChangeTrackingTable table, long previousVersionId);

    Task ApplyChangesAsync(ConnectionStringModel targetConnectionString, ChangeTrackingTable table, ChangeTrackingMapping mapping, IEnumerable<ChangeTrackingRow> changes);
}
