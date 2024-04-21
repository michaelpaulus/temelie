using System.Collections.Immutable;
using System.Text;
using Temelie.Database.Models;
using Microsoft.CodeAnalysis;

namespace Temelie.Entities.SourceGenerator;

[Generator]
public class EntityIncrementalGenerator : IIncrementalGenerator
{

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var options = context.AnalyzerConfigOptionsProvider.Select((c, _) => { c.GlobalOptions.TryGetValue("build_property.RootNamespace", out string rootNamespace); return rootNamespace; });

        var files = context.AdditionalTextsProvider
            .Where(a => a.Path.EndsWith(".sql.json"))
            .Select((a, c) => (a.Path, a.GetText(c)!.ToString()))
            .Collect();

        var result = options.Combine(files);

        context.RegisterImplementationSourceOutput(result, Generate);
    }

    void Generate(SourceProductionContext context, (string RootNamesapce, ImmutableArray<(string FileName, string Text)> files) result)
    {
        var rootNamesapce = result.RootNamesapce;

        try
        {

            var databaseModel = DatabaseModel.CreateFromFiles(result.files);

            var entityIds = new Dictionary<string, string>();

            IEnumerable<ColumnProperty> getColumnProperties(TableModel table)
            {
                var list = new List<ColumnProperty>();

                foreach (var column in table.Columns)
                {
                    var prop = new ColumnProperty();
            
                    prop.ColumnName = column.ColumnName;
                    prop.ColumnId = column.ColumnId;
                    prop.PropertyName = column.PropertyName;
                    prop.PropertyType = ColumnModel.GetSystemTypeString(ColumnModel.GetSystemType(column.DbType));
                    prop.Default = "";
                    prop.IsIdentity = column.IsIdentity;
                    prop.IsComputed = column.IsComputed;
                    prop.IsPrimaryKey = column.IsPrimaryKey;
                    prop.SystemTypeString = ColumnModel.GetSystemTypeString(ColumnModel.GetSystemType(column.DbType));

                    list.Add(prop);

                    var fkSouceColumn = databaseModel.GetForeignKeySourceColumn(table.SchemaName, table.TableName, column.ColumnName);
                    if (fkSouceColumn is not null)
                    {
                        prop.IsForeignKey = true;
                        prop.PropertyType = fkSouceColumn.PropertyName;
                        prop.IsEntityId = true;
                    }
                    else if (column.IsPrimaryKey)
                    {
                        prop.PropertyType = column.PropertyName;
                        prop.IsEntityId = true;
                    }

                    if (prop.PropertyName == table.ClassName)
                    {
                        prop.PropertyName += "_Id";
                    }

                    if (column.IsNullable)
                    {
                        prop.PropertyType += "?";
                        prop.IsNullable = true;
                    }
                    else
                    {
                        prop.Default = $" = {GetTypeDefault(prop.PropertyType)};";
                    }
                }

                return list;
            }

            void addInterface(TableModel table, IEnumerable<ColumnProperty> props)
            {
                var ns = rootNamesapce;
                var className = table.ClassName;

                var sb = new StringBuilder();
                sb.AppendLine($@"#nullable enable
using Temelie.Entities;

namespace {ns};

public interface I{className} : IEntity <{className}>
{{
");
                foreach (var column in props)
                {
                    sb.AppendLine($"    {column.PropertyType} {column.PropertyName} {{ get; set; }}");
                }

                sb.AppendLine(@$"
}}
");
                context.AddSource($"{ns}.I{className}.g", sb.ToString());
            }

            void addRecord(TableModel table, IEnumerable<ColumnProperty> props)
            {
                var ns = rootNamesapce;
                var className = table.ClassName;

                var sb = new StringBuilder();
                sb.AppendLine($@"#nullable enable
using Temelie.Entities;

namespace {ns};

[System.ComponentModel.DataAnnotations.Schema.Table(""{table.TableName}"", Schema = ""{table.SchemaName}"")]
public record {className} : I{className}
{{
");
                foreach (var column in props)
                {


                    if (column.IsEntityId &&
                        !column.IsForeignKey)
                    {
                        if (!entityIds.ContainsKey(column.PropertyType))
                        {
                            entityIds.Add(column.PropertyType, column.SystemTypeString);
                        }
                    }
                    
                    if (column.IsPrimaryKey)
                    {
                        sb.AppendLine($"    [System.ComponentModel.DataAnnotations.Key]");
                    }
                    if (column.IsComputed)
                    {
                        sb.AppendLine($"    [System.ComponentModel.DataAnnotations.Schema.DatabaseGenerated(System.ComponentModel.DataAnnotations.Schema.DatabaseGeneratedOption.Computed)]");
                    }
                    if (column.IsIdentity)
                    {
                        sb.AppendLine($"    [System.ComponentModel.DataAnnotations.Schema.DatabaseGenerated(System.ComponentModel.DataAnnotations.Schema.DatabaseGeneratedOption.Identity)]");
                    }
                    sb.AppendLine($"    [System.ComponentModel.DataAnnotations.Schema.Column(\"{column.ColumnName}\", Order = {column.ColumnId})]");
                    sb.AppendLine($"    public {column.PropertyType} {column.PropertyName} {{ get; set; }}{column.Default}");
                }

                sb.AppendLine(@$"
}}
");
                context.AddSource($"{ns}.{className}.g", sb.ToString());
            }

            foreach (var table in databaseModel.Tables)
            {
                var props = getColumnProperties(table);
                addInterface(table, props);
                addRecord(table, props);

            }

            foreach (var table in databaseModel.Views)
            {
                var props = getColumnProperties(table);
                addInterface(table, props);
                addRecord(table, props);
            }

            foreach (var group in entityIds)
            {
                var ns = rootNamesapce;
                var className = group.Key;
                var propertyType = group.Value;

                var sb = new StringBuilder();
                sb.AppendLine(@$"using Temelie.Entities;
#nullable enable
namespace {ns};
public record struct {className}({propertyType} Value = {GetTypeDefault(propertyType)}) : IEntityId, IComparable<{className}>
{{
    public int CompareTo({className} other)
    {{
        return Value.CompareTo(other.Value);
    }}
}}
");
                context.AddSource($"{ns}.{className}.g", sb.ToString());
            }
        }
        catch (Exception ex)
        {
            throw new Exception(ex.ToString());
        }


    }

    private string GetTypeDefault(string propertyType)
    {
        if (propertyType == "string")
        {
            return "\"\"";
        }
        else if (propertyType == "int")
        {
            return "0";
        }
        else if (propertyType == "long")
        {
            return "0";
        }
        else if (propertyType == "short")
        {
            return "0";
        }
        return "default";
    }

    private class ColumnProperty
    {
        public string PropertyName { get; set; }
        public string PropertyType { get; set; }
        public string Default { get; set; }
        public bool IsEntityId { get; set; }
        public bool IsNullable { get; set; }
        public bool IsPrimaryKey { get; set; }
        public bool IsComputed { get; set; }
        public bool IsIdentity { get; set; }
        public string ColumnName { get; set; }
        public int ColumnId { get; set; }
        public bool IsForeignKey { get; internal set; }
        public string SystemTypeString { get; internal set; }
    }

}
