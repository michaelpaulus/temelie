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

        context.RegisterSourceOutput(result, Generate);
    }

    void Generate(SourceProductionContext context, (string RootNamesapce, ImmutableArray<(string FileName, string Text)> files) result)
    {

        var rootNamesapce = result.RootNamesapce;

        var databaseModel = DatabaseModel.CreateFromFiles(result.files);

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
            var ns = rootNamesapce;
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
public record {className} : EntityBase, I{className}
{{
");
            foreach (var column in props)
            {
                if (column.IsPrimaryKey)
                {
                    sb.AppendLine($"    [System.ComponentModel.DataAnnotations.Key]");
                    if (!column.IsIdentity)
                    {
                        sb.AppendLine($"    [System.ComponentModel.DataAnnotations.Schema.DatabaseGenerated(System.ComponentModel.DataAnnotations.Schema.DatabaseGeneratedOption.None)]");
                    }
                }
                if (column.IsComputed)
                {
                    sb.AppendLine($"    [System.ComponentModel.DataAnnotations.Schema.DatabaseGenerated(System.ComponentModel.DataAnnotations.Schema.DatabaseGeneratedOption.Computed)]");
                }
                if (column.IsIdentity)
                {
                    sb.AppendLine($"    [System.ComponentModel.DataAnnotations.Schema.DatabaseGenerated(System.ComponentModel.DataAnnotations.Schema.DatabaseGeneratedOption.Identity)]");
                }
                if (column.Precision.HasValue && column.Scale.HasValue)
                {
                    sb.AppendLine($"    [Temelie.Entities.ColumnPrecision({column.Precision.Value}, {column.Scale.Value})]");
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
            context.CancellationToken.ThrowIfCancellationRequested();
            var props = getColumnProperties(table);
            addInterface(table, props);
            addRecord(table, props);

        }

        foreach (var table in databaseModel.Views)
        {
            context.CancellationToken.ThrowIfCancellationRequested();
            var props = getColumnProperties(table);
            addInterface(table, props);
            addRecord(table, props);
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
        public bool IsNullable { get; set; }
        public bool IsPrimaryKey { get; set; }
        public bool IsComputed { get; set; }
        public bool IsIdentity { get; set; }
        public string ColumnName { get; set; }
        public int ColumnId { get; set; }
        public int? Precision { get; set; }
        public int? Scale { get; set; }
        public bool IsForeignKey { get; internal set; }
        public string SystemTypeString { get; internal set; }
    }

}
