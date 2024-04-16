using System.Reflection;
using System.Text.Json;

namespace Temelie.Database.Models;

public class DatabaseModel(
    IEnumerable<TableModel> tables,
    IEnumerable<TableModel> views,
    IEnumerable<IndexModel> allIndexes,
    IEnumerable<TriggerModel> triggers,
    IEnumerable<ForeignKeyModel> foreignKeys,
    IEnumerable<CheckConstraintModel> checkConstraints,
    IEnumerable<DefinitionModel> definitions,
    IEnumerable<SecurityPolicyModel> securityPolicies
    )
{
    private readonly IEnumerable<IndexModel> _allIndexes = allIndexes;
    public IEnumerable<TableModel> Tables { get; } = tables;
    public IEnumerable<TableModel> Views { get; } = views;
    public IEnumerable<TriggerModel> Triggers { get; } = triggers;
    public IEnumerable<ForeignKeyModel> ForeignKeys { get; } = foreignKeys;
    public IEnumerable<CheckConstraintModel> CheckConstraints { get; } = checkConstraints;
    public IEnumerable<DefinitionModel> Definitions { get; } = definitions;
    public IEnumerable<SecurityPolicyModel> SecurityPolicies { get; } = securityPolicies;

    public IEnumerable<IndexModel> Indexes => (from i in _allIndexes where !i.IsPrimaryKey select i).ToList();

    public IEnumerable<IndexModel> PrimaryKeys => (from i in _allIndexes where i.IsPrimaryKey select i).ToList();

    public ColumnModel GetForeignKeySourceColumn(string sourceTableName, string sourceColumnName)
    {
        foreach (var fk in ForeignKeys)
        {
            if (fk.TableName == sourceTableName)
            {
                foreach (var detail in fk.Detail)
                {
                    if (detail.Column == sourceColumnName)
                    {
                        var referencedTable = Tables.FirstOrDefault(i => i.TableName == fk.ReferencedTableName);
                        if (referencedTable is not null)
                        {
                            var referencedColumn = referencedTable.Columns.FirstOrDefault(i => i.ColumnName == detail.ReferencedColumn);
                            if (referencedColumn is not null)
                            {
                                var fkColumn = GetForeignKeySourceColumn(referencedTable.TableName, referencedColumn.ColumnName);
                                if (fkColumn is not null)
                                {
                                    return fkColumn;
                                }
                                return referencedColumn;
                            }
                        }

                    }
                }
            }
        }
        return null;
    }

    public static DatabaseModel CreateFromAssembly(Assembly assembly)
    {
        var files = new List<(string, string)>();
        foreach (var name in assembly.GetManifestResourceNames())
        {
            if (name.EndsWith(".sql.json"))
            {
                using var stream = assembly.GetManifestResourceStream(name);
                using var reader = new StreamReader(stream!);
                var json = reader.ReadToEnd();
                files.Add((name, json));
            }
        }
        return CreateFromFiles(files);
    }

    public static DatabaseModel CreateFromFiles(IEnumerable<(string FileName, string Contents)> files)
    {
        var tables = new List<TableModel>();
        var views = new List<TableModel>();
        var allIndexes = new List<IndexModel>();
        var triggers = new List<TriggerModel>();
        var foreignKeys = new List<ForeignKeyModel>();
        var checkConstraints = new List<CheckConstraintModel>();
        var definitions = new List<DefinitionModel>();
        var securityPolicies = new List<SecurityPolicyModel>();

        foreach (var file in files)
        {
            var name = file.FileName;
            if (name.EndsWith(".sql.json"))
            {
                var json = file.Contents;
                if (name.Contains("02_Tables"))
                {
                    var model = JsonSerializer.Deserialize(json, typeof(TableModel), ModelsJsonSerializerOptions.Default)! as TableModel;
                    tables.Add(model);
                }
                else if (name.Contains("03_Indexes"))
                {
                    var model = JsonSerializer.Deserialize(json, typeof(IndexModel), ModelsJsonSerializerOptions.Default)! as IndexModel;
                    allIndexes.Add(model);
                }
                else if (name.Contains("04_CheckConstraints"))
                {
                    var model = JsonSerializer.Deserialize(json, typeof(CheckConstraintModel), ModelsJsonSerializerOptions.Default)! as CheckConstraintModel;
                    checkConstraints.Add(model);
                }
                else if (name.Contains("05_Programmability") || name.Contains("05_Views"))
                {
                    var model = JsonSerializer.Deserialize(json, typeof(DefinitionModel), ModelsJsonSerializerOptions.Default)! as DefinitionModel;
                    definitions.Add(model);
                    if (model.View is not null)
                    {
                        views.Add(model.View);
                    }
                }
                else if (name.Contains("06_Triggers"))
                {
                    var model = JsonSerializer.Deserialize(json, typeof(TriggerModel), ModelsJsonSerializerOptions.Default)! as TriggerModel;
                    triggers.Add(model);
                }
                else if (name.Contains("08_ForeignKeys"))
                {
                    var model = JsonSerializer.Deserialize(json, typeof(ForeignKeyModel), ModelsJsonSerializerOptions.Default)! as ForeignKeyModel;
                    foreignKeys.Add(model);
                }
                else if (name.Contains("09_SecurityPolicies"))
                {
                    var model = JsonSerializer.Deserialize(json, typeof(SecurityPolicyModel), ModelsJsonSerializerOptions.Default)! as SecurityPolicyModel;
                    securityPolicies.Add(model);
                }
            }
        }

        var databaseModel = new DatabaseModel(
            tables,
            views,
            allIndexes,
            triggers,
            foreignKeys,
            checkConstraints,
            definitions,
            securityPolicies);

        return databaseModel;
    }

}

