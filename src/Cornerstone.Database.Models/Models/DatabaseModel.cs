using System.Reflection;
using System.Text.Json;

namespace Cornerstone.Database.Models;

public class DatabaseModel(
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
    public IEnumerable<TableModel> Tables { get; } = tables;
    public IEnumerable<TableModel> Views { get; } = views;
    public IEnumerable<TriggerModel> Triggers { get; } = triggers;
    public IEnumerable<ForeignKeyModel> ForeignKeys { get; } = foreignKeys;
    public IEnumerable<CheckConstraintModel> CheckConstraints { get; } = checkConstraints;
    public IEnumerable<DefinitionModel> Definitions { get; } = definitions;
    public IEnumerable<SecurityPolicyModel> SecurityPolicies { get; } = securityPolicies;

    public IEnumerable<Models.IndexModel> Indexes => (from i in _allIndexes where !i.IsPrimaryKey select i).ToList();

    public IEnumerable<Models.IndexModel> PrimaryKeys => (from i in _allIndexes where i.IsPrimaryKey select i).ToList();

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
        var tables = new List<Models.TableModel>();
        var views = new List<Models.TableModel>();
        var allIndexes = new List<Models.IndexModel>();
        var triggers = new List<Models.TriggerModel>();
        var foreignKeys = new List<Models.ForeignKeyModel>();
        var checkConstraints = new List<Models.CheckConstraintModel>();
        var definitions = new List<Models.DefinitionModel>();
        var securityPolicies = new List<Models.SecurityPolicyModel>();

        foreach (var file in files)
        {
            var name = file.FileName;
            if (name.EndsWith(".sql.json"))
            {
                var json = file.Contents;
                if (name.Contains("02_Tables"))
                {
                    var model = JsonSerializer.Deserialize<TableModel>(json, new JsonSerializerOptions() { PropertyNameCaseInsensitive = true })!;
                    tables.Add(model);
                }
                else if (name.Contains("03_Indexes"))
                {
                    var model = JsonSerializer.Deserialize<IndexModel>(json, new JsonSerializerOptions() { PropertyNameCaseInsensitive = true })!;
                    allIndexes.Add(model);
                }
                else if (name.Contains("04_CheckConstraints"))
                {
                    var model = JsonSerializer.Deserialize<CheckConstraintModel>(json, new JsonSerializerOptions() { PropertyNameCaseInsensitive = true })!;
                    checkConstraints.Add(model);
                }
                else if (name.Contains("05_Programmability") || name.Contains("05_Views"))
                {
                    var model = JsonSerializer.Deserialize<DefinitionModel>(json, new JsonSerializerOptions() { PropertyNameCaseInsensitive = true })!;
                    definitions.Add(model);
                    if (model.View is not null)
                    {
                        views.Add(model.View);
                    }
                }
                else if (name.Contains("06_Triggers"))
                {
                    var model = JsonSerializer.Deserialize<TriggerModel>(json, new JsonSerializerOptions() { PropertyNameCaseInsensitive = true })!;
                    triggers.Add(model);
                }
                else if (name.Contains("08_ForeignKeys"))
                {
                    var model = JsonSerializer.Deserialize<ForeignKeyModel>(json, new JsonSerializerOptions() { PropertyNameCaseInsensitive = true })!;
                    foreignKeys.Add(model);
                }
                else if (name.Contains("09_SecurityPolicies"))
                {
                    var model = JsonSerializer.Deserialize<SecurityPolicyModel>(json, new JsonSerializerOptions() { PropertyNameCaseInsensitive = true })!;
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

