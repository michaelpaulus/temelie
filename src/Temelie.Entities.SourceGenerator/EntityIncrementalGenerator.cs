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

            var pkColumns = new Dictionary<string, string>();

            void addTable(TableModel table)
            {
                var ns = rootNamesapce;
                var className = table.ClassName;

                var sb = new StringBuilder();
                sb.AppendLine($@"#nullable enable
using Temelie.Entities;

namespace {ns};

[System.ComponentModel.DataAnnotations.Schema.Table(""{table.TableName}"", Schema = ""{table.SchemaName}"")]
public record {className} : IEntity<{className}>
{{
");
                foreach (var column in table.Columns)
                {
                    var propertyName = column.PropertyName;
                    var propertyType = ColumnModel.GetSystemTypeString(ColumnModel.GetSystemType(column.DbType));
                    var dft = "";

                    var fkSouceColumn = databaseModel.GetForeignKeySourceColumn(table.SchemaName, table.TableName, column.ColumnName);
                    if (fkSouceColumn is not null)
                    {
                        propertyType = fkSouceColumn.PropertyName;
                        sb.AppendLine($"    [Temelie.Entities.EntityId]");
                    }
                    else if (column.IsPrimaryKey)
                    {
                        propertyType = column.PropertyName;
                        if (!pkColumns.ContainsKey(propertyType))
                        {
                            pkColumns.Add(propertyType, ColumnModel.GetSystemTypeString(ColumnModel.GetSystemType(column.DbType)));
                        }
                        sb.AppendLine($"    [Temelie.Entities.EntityId]");
                    }

                    if (propertyName == className)
                    {
                        propertyName += "_Id";
                    }

                    if (column.IsNullable)
                    {
                        propertyType += "?";
                        sb.AppendLine($"    [Temelie.Entities.Nullable]");
                    }
                    else
                    {
                        dft = $" = {GetTypeDefault(propertyType)};";
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
                    sb.AppendLine($"    public {propertyType} {propertyName} {{ get; set; }}{dft}");
                }

                sb.AppendLine(@$"
}}
");
                context.AddSource($"{ns}.{className}.g", sb.ToString());
            }

            foreach (var table in databaseModel.Tables)
            {
                addTable(table);
            }

            foreach (var table in databaseModel.Views)
            {
                addTable(table);
            }

            foreach (var group in pkColumns)
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

}
