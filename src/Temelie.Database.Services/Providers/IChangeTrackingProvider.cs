#nullable enable

using Temelie.Database.Models;
using Temelie.Database.Models.ChangeTracking;

namespace Temelie.Database.Providers;

public interface IChangeTrackingProvider
{
    string Provider { get; }

    Task<IEnumerable<TrackedTable>> GetTrackedTablesAsync(ConnectionStringModel sourceConnectionString);

    Task<ChangeTrackingMapping?> GetMappingAsync(ConnectionStringModel targetConnectionString, TrackedTable table);

    Task<IEnumerable<ChangeTrackingRow>> GetTrackedTablesChangesAsync(ConnectionStringModel sourceConnectionString, ConnectionStringModel targetConnectionString, TrackedTable table, ChangeTrackingMapping mapping);

    Task ApplyChangesAsync(ConnectionStringModel targetConnectionString, TrackedTable table, ChangeTrackingMapping mapping, IEnumerable<ChangeTrackingRow> changes);
}
