using Temelie.Database.Models;

namespace Temelie.Database.Testing.Fakes;

/// <summary>
/// The schema of the fake database. Scripts change it through <see cref="FakeDatabase.WhenExecuted"/> rules.
/// </summary>
public sealed class DatabaseState
{
    public List<TableModel> Tables { get; } = [];
    public List<DefinitionModel> Definitions { get; } = [];
    public List<IndexModel> Indexes { get; } = [];
    public List<TriggerModel> Triggers { get; } = [];
    public List<ForeignKeyModel> ForeignKeys { get; } = [];
    public List<CheckConstraintModel> CheckConstraints { get; } = [];
    public List<SecurityPolicyModel> SecurityPolicies { get; } = [];

    public static DatabaseState From(DatabaseModel model)
    {
        var state = new DatabaseState();
        state.Tables.AddRange(model.Tables);
        state.Definitions.AddRange(model.Definitions);
        state.Indexes.AddRange(model.PrimaryKeys.Concat(model.Indexes));
        state.Triggers.AddRange(model.Triggers);
        state.ForeignKeys.AddRange(model.ForeignKeys);
        state.CheckConstraints.AddRange(model.CheckConstraints);
        state.SecurityPolicies.AddRange(model.SecurityPolicies);
        return state;
    }

    public DatabaseModel ToModel() => new(
        Tables.ToList(),
        Definitions.Where(i => i.View is not null).Select(i => i.View!).ToList(),
        Indexes.ToList(),
        Triggers.ToList(),
        ForeignKeys.ToList(),
        CheckConstraints.ToList(),
        Definitions.ToList(),
        SecurityPolicies.ToList());
}
