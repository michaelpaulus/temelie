using Temelie.Database.Models;
using Temelie.Database.Models.ChangeTracking;

namespace Temelie.Database.Services;
public interface IChangeTrackingService
{
    Task UpdateSchemaAsync(ConnectionStringModel sourceConnectionString, string sourceProvider);
    Task DetectChangesAsync(ConnectionStringModel sourceConnectionString, ChangeTrackingTable table, ChangeTrackingMapping mapping);
    Task<IEnumerable<ChangeTrackingTableAndMapping>> GetTrackedTablesAndMappingsAsync(int sourceId, ConnectionStringModel sourceConnectionString, ConnectionStringModel targetConnectionString);
    Task SyncChangesAsync(int sourceId, ConnectionStringModel sourceConnectionString, ConnectionStringModel targetConnectionString);
    Task SyncChangesAsync(ConnectionStringModel sourceConnectionString, ConnectionStringModel targetConnectionString, ChangeTrackingTable table, ChangeTrackingMapping mapping);
    Task FlagSyncingAsync(ConnectionStringModel targetConnectionString, ChangeTrackingMapping mapping, bool isSyncing);

}
