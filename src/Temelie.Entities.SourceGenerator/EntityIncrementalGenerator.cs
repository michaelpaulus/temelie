using System.Text;
using Temelie.Database.Models;
using Microsoft.CodeAnalysis;

namespace Temelie.Entities.SourceGenerator;

[Generator]
public partial class EntityIncrementalGenerator : IIncrementalGenerator
{

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var options = context.AnalyzerConfigOptionsProvider.Select(static (c, _) => { c.GlobalOptions.TryGetValue("build_property.RootNamespace", out string rootNamespace); return rootNamespace; });

        var files = context.AdditionalTextsProvider
            .Where(static a => a.Path.EndsWith(".sql.json"))
            .Select(static (a, c) => new File(a.Path, a.GetText(c)));

        var result = files.Combine(options);

        context.RegisterSourceOutput(result, Generate);
    }

    private static void Generate(SourceProductionContext context, (File File, string RootNamesapce) result)
    {
        context.CancellationToken.ThrowIfCancellationRequested();
        var sourceFiles = Generate(result.RootNamesapce, result.File.FilePath, result.File.Content.ToString());
        foreach (var file in sourceFiles)
        {
            context.AddSource(file.Name, file.Code);
        }
    }

    public static IEnumerable<(string Name, string Code)> Generate(string rootNamespace, string filePath, string fileContents)
    {
        var sourceFiles = new List<(string Name, string Code)>();

        var databaseModel = DatabaseModel.CreateFromFiles([(filePath, fileContents)]);

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

                switch (column.ColumnType.ToUpper())
                {
                    case "DECIMAL":
                    case "NUMERIC":
                        prop.Precision = column.Precision;
                        prop.Scale = column.Scale;
                        break;

                }

                prop.SystemTypeString = ColumnModel.GetSystemTypeString(ColumnModel.GetSystemType(column.DbType));

                list.Add(prop);

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
                    if (prop.PropertyType != "int" &&
                        prop.PropertyType != "System.Guid" &&
                        prop.PropertyType != "System.DateTime")
                    {
                        prop.Default = $" = {GetTypeDefault(prop.PropertyType)};";
                    }
                }
            }

            return list;
        }

        void addInterface(TableModel table, IEnumerable<ColumnProperty> props)
        {
            var ns = rootNamespace;
            var className = table.ClassName;

            var extends = new StringBuilder();

            extends.Append($"IEntity<{className}>");

            var props2 = props.ToList();

            var modifiedColumns = new List<ColumnProperty>();
            var createdColumns = new List<ColumnProperty>();

            foreach (var prop in props)
            {
                if (prop.PropertyName.Equals("CreatedDate") ||
                    prop.PropertyName.Equals("CreatedBy"))
                {
                    createdColumns.Add(prop);
                }

                if (prop.PropertyName.Equals("ModifiedDate") ||
                    prop.PropertyName.Equals("ModifiedBy"))
                {
                    modifiedColumns.Add(prop);
                }
            }

            if (createdColumns.Count == 2)
            {
                extends.Append(", ICreatedByEntity");
                foreach (var col in createdColumns)
                {
                    props2.Remove(col);
                }
            }

            if (modifiedColumns.Count == 2)
            {
                extends.Append(", IModifiedByEntity");
                foreach (var col in modifiedColumns)
                {
                    props2.Remove(col);
                }
            }

            var sb = new StringBuilder();
            sb.AppendLine($@"#nullable enable
using Temelie.Entities;

namespace {ns};

public interface I{className} : {extends}
{{
");
            foreach (var column in props2)
            {
                sb.AppendLine($"    {column.PropertyType} {column.PropertyName} {{ get; set; }}");
            }

            sb.AppendLine(@$"
}}");
            sourceFiles.Add(($"{ns}.I{className}.g", sb.ToString()));
        }

        void addRecord(TableModel table, IEnumerable<ColumnProperty> props)
        {
            var ns = rootNamespace;
            var className = table.ClassName;

            var tableAttribute = string.IsNullOrEmpty(table.SchemaName) ? $"[Table(\"{table.TableName}\")]" : $"[Table(\"{table.TableName}\", Schema = \"{table.SchemaName}\")]";

            var sb = new StringBuilder();
            sb.AppendLine($@"#nullable enable
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Temelie.Entities;

namespace {ns};

{tableAttribute}
public record {className} : EntityBase, I{className}
{{
");
            foreach (var column in props)
            {
                if (column.IsPrimaryKey)
                {
                    sb.AppendLine($"    [Key]");
                    if (!column.IsIdentity)
                    {
                        sb.AppendLine($"    [DatabaseGenerated(DatabaseGeneratedOption.None)]");
                    }
                }
                if (column.IsComputed)
                {
                    sb.AppendLine($"    [DatabaseGenerated(DatabaseGeneratedOption.Computed)]");
                }
                if (column.IsIdentity)
                {
                    sb.AppendLine($"    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]");
                }
                if (column.Precision.HasValue && column.Scale.HasValue)
                {
                    sb.AppendLine($"    [Temelie.Entities.ColumnPrecision({column.Precision.Value}, {column.Scale.Value})]");
                }
                sb.AppendLine($"    [Column(\"{column.ColumnName}\", Order = {column.ColumnId})]");
                sb.AppendLine($"    public {column.PropertyType} {column.PropertyName} {{ get; set; }}{column.Default}");
            }

            sb.AppendLine(@$"
}}");
            sourceFiles.Add(($"{ns}.{className}.g", sb.ToString()));
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

        return sourceFiles;
    }

    static string GetTypeDefault(string propertyType)
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
