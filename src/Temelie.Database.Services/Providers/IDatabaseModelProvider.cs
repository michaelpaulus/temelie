using Temelie.Database.Models;

namespace Temelie.Database.Providers;

public interface IDatabaseModelProvider
{
    void Initialize(ColumnModel model);
    void Initialize(TableModel model);
    void Initialize(TriggerModel model);
    void Initialize(IndexModel model);
    void Initialize(ForeignKeyModel model);
    void Initialize(DefinitionModel model);
    void Initialize(SecurityPolicyModel model);
}
