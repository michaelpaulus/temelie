using Temelie.Database.Models;

namespace Temelie.Database.Providers;

public interface IDatabaseModelProvider
{
    public void Initialize(ColumnModel model) { }
    public void Initialize(TableModel model) { }
    public void Initialize(TriggerModel model) { }
    public void Initialize(IndexModel model) { }
    public void Initialize(ForeignKeyModel model) { }
    public void Initialize(DefinitionModel model) { }
    public void Initialize(SecurityPolicyModel model) { }
    public void Initialize(CheckConstraintModel model) { }
}
