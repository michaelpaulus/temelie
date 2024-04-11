namespace Cornerstone.Database.Models;

public class DatabaseModel(string databaseName,
    string providerName,
    IEnumerable<Models.TableModel> tables,
    IEnumerable<Models.TableModel> views,
    IEnumerable<Models.IndexModel> allIndexes,
    IEnumerable<Models.TriggerModel> triggers,
    IEnumerable<Models.ForeignKeyModel> foreignKeys,
    IEnumerable<Models.CheckConstraintModel> checkConstraints,
    IEnumerable<Models.DefinitionModel> definitions,
    IEnumerable<Models.SecurityPolicyModel> securityPolicies
    )
{
    private readonly IEnumerable<IndexModel> _allIndexes = allIndexes;

    public string DatabaseName { get; } = databaseName;
    public string ProviderName { get; } = providerName;
    public IEnumerable<TableModel> Tables { get; } = tables;
    public IEnumerable<TableModel> Views { get; } = views;
    public IEnumerable<TriggerModel> Triggers { get; } = triggers;
    public IEnumerable<ForeignKeyModel> ForeignKeys { get; } = foreignKeys;
    public IEnumerable<CheckConstraintModel> CheckConstraints { get; } = checkConstraints;
    public IEnumerable<DefinitionModel> Definitions { get; } = definitions;
    public IEnumerable<SecurityPolicyModel> SecurityPolicies { get; } = securityPolicies;

    public IEnumerable<Models.IndexModel> Indexes => (from i in _allIndexes where !i.IsPrimaryKey select i).ToList();

    public IEnumerable<Models.IndexModel> PrimaryKeys => (from i in _allIndexes where i.IsPrimaryKey select i).ToList();

}

