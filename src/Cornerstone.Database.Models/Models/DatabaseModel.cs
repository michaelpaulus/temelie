namespace Cornerstone.Database.Models;

public class DatabaseModel(string databaseName,
    string quoteCharacterStart,
    string quoteCharacterEnd,
    IEnumerable<Models.TableModel> tables,
    IEnumerable<Models.ColumnModel> tableColumns,
    IEnumerable<Models.TableModel> views,
    IEnumerable<Models.ColumnModel> viewColumns,
    IEnumerable<Models.IndexModel> allIndexes,
    IEnumerable<Models.TriggerModel> triggers,
    IEnumerable<Models.ForeignKeyModel> foreignKeys,
    IEnumerable<Models.CheckConstraintModel> checkConstraints,
    IEnumerable<Models.DefinitionModel> definitions,
    IEnumerable<Models.SecurityPolicyModel> securityPolicies
    )
{

    public string DatabaseName { get; } = databaseName;
    public string QuoteCharacterStart { get; } = quoteCharacterStart;
    public string QuoteCharacterEnd { get; } = quoteCharacterEnd;
    public IEnumerable<TableModel> Tables { get; } = tables;
    public IEnumerable<ColumnModel> TableColumns { get; } = tableColumns;
    public IEnumerable<TableModel> Views { get; } = views;
    public IEnumerable<ColumnModel> ViewColumns { get; } = viewColumns;
    public IEnumerable<IndexModel> AllIndexes { get; } = allIndexes;
    public IEnumerable<TriggerModel> Triggers { get; } = triggers;
    public IEnumerable<ForeignKeyModel> ForeignKeys { get; } = foreignKeys;
    public IEnumerable<CheckConstraintModel> CheckConstraints { get; } = checkConstraints;
    public IEnumerable<DefinitionModel> Definitions { get; } = definitions;
    public IEnumerable<SecurityPolicyModel> SecurityPolicies { get; } = securityPolicies;

    private IList<Models.IndexModel> _indexes;
    public IList<Models.IndexModel> Indexes
    {
        get
        {
            if (this._indexes == null)
            {
                this._indexes = (from i in AllIndexes where !i.IsPrimaryKey select i).ToList();
            }
            return this._indexes;
        }
    }

    private IList<Models.IndexModel> _primaryKeys;

    public IList<Models.IndexModel> PrimaryKeys
    {
        get
        {
            if (this._primaryKeys == null)
            {
                this._primaryKeys = (from i in AllIndexes where i.IsPrimaryKey select i).ToList();
            }
            return this._primaryKeys;
        }
    }

    public IList<string> TableNames
    {
        get
        {
            return (
                from i in this.Tables
                select i.TableName).ToList();
        }
    }

    public IList<string> ViewNames
    {
        get
        {
            return (
                from i in this.Views
                select i.TableName).ToList();
        }
    }

}

