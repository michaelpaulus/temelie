using Temelie.Database.Models;
using Temelie.Database.Models.ChangeTracking;

namespace Temelie.Database.Services;
public interface IChangeTrackingService
{
    Task DetectChangesAsync(ConnectionStringModel sourceConnectionString);
    Task<IEnumerable<(ChangeTrackingTable Table, ChangeTrackingMapping Mapping)>> GetTrackedTablesAndMappingsAsync(ConnectionStringModel sourceConnectionString, ConnectionStringModel targetConnectionString);
    Task SyncChangesAsync(ConnectionStringModel sourceConnectionString, ConnectionStringModel targetConnectionString);
    Task SyncChangesAsync(ConnectionStringModel sourceConnectionString, ConnectionStringModel targetConnectionString, ChangeTrackingTable table, ChangeTrackingMapping mapping);
}
