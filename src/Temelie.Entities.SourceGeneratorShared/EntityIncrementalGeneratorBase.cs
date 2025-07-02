using System.Text;
using Temelie.Database.Models;
using Microsoft.CodeAnalysis;
using System.Collections.Immutable;

namespace Temelie.Entities.SourceGenerator;

public abstract class EntityIncrementalGeneratorBase : IIncrementalGenerator
{

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var options = context.AnalyzerConfigOptionsProvider.Select(static (c, _) => { c.GlobalOptions.TryGetValue("build_property.RootNamespace", out string rootNamespace); return rootNamespace; });

        var files = context.AdditionalTextsProvider
            .Where(static a => a.Path.EndsWith(".sql.json"))
            .Select(static (a, c) => new File(a.Path, a.GetText(c)));

        var result = files.Combine(options).Collect();

        context.RegisterSourceOutput(result, Generate);
    }

    private void Generate(SourceProductionContext context, ImmutableArray<(File File, string RootNamesapce)> result)
    {
        context.CancellationToken.ThrowIfCancellationRequested();

        if (result.Any())
        {
            var sourceFiles = Generate(result.First().RootNamesapce, result.Select(i => (i.File.FilePath, i.File.Content.ToString())).ToList());
            foreach (var file in sourceFiles)
            {
                context.AddSource(file.Name, file.Code);
            }
        }
    }

    public virtual IEnumerable<(string Name, string Code)> Generate(string rootNamespace, IEnumerable<(string FilePath, string FileContents)> files)
    {
        var sourceFiles = new List<(string Name, string Code)>();

        var databaseModel = DatabaseModel.CreateFromFiles(files);

        sourceFiles.Add(($"{rootNamespace}.IProjectEntity.g", $@"namespace {rootNamespace};

public partial interface IProjectEntity
{{
}}"));

        foreach (var table in databaseModel.Tables)
        {
            var props = GetColumnProperties(table);
            AddInterface(table, props, rootNamespace, sourceFiles);
            AddRecord(table, props, rootNamespace, sourceFiles, databaseModel);

        }

        foreach (var table in databaseModel.Views)
        {
            var props = GetColumnProperties(table);
            AddInterface(table, props, rootNamespace, sourceFiles);
            AddRecord(table, props, rootNamespace, sourceFiles, databaseModel);
        }

        return sourceFiles;
    }

    protected virtual string GetRecordAttributes(TableModel table, IEnumerable<ColumnProperty> props, DatabaseModel databaseModel)
    {
        var tableAttributes = new StringBuilder();

        tableAttributes.AppendLine(string.IsNullOrEmpty(table.SchemaName) ? $"[Table(\"{table.TableName}\")]" : $"[Table(\"{table.TableName}\", Schema = \"{table.SchemaName}\")]");

        var triggers = databaseModel.Triggers.Any(i => i.TableName == table.TableName && i.SchemaName == table.SchemaName);

        if (triggers)
        {
            tableAttributes.AppendLine($"[Temelie.Entities.HasTrigger]");
        }

        return tableAttributes.ToString();
    }

    protected virtual string GetRecordProperties(TableModel table, IEnumerable<ColumnProperty> props)
    {
        var sb = new StringBuilder();
        foreach (var column in props)
        {
            sb.AppendLine(GetRecordProperty(table, column));
        }
        return sb.ToString();
    }

    protected virtual string GetRecordProperty(TableModel table, ColumnProperty column)
    {
        var sb = new StringBuilder();

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

        return sb.ToString();
    }

    protected virtual string GetRecordExtends(TableModel table, IEnumerable<ColumnProperty> props)
    {
        var sb = new StringBuilder();
        sb.Append($"EntityBase, I{table.ClassName}");
        return sb.ToString();
    }

    protected virtual void AddRecord(TableModel table, IEnumerable<ColumnProperty> props, string rootNamespace, List<(string Name, string Code)> sourceFiles, DatabaseModel databaseModel)
    {
        var recordAttributes = GetRecordAttributes(table, props, databaseModel);
        var extends = GetRecordExtends(table, props);
        var sb = new StringBuilder();
        sb.AppendLine($@"#nullable enable
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Temelie.Entities;

namespace {rootNamespace};

{recordAttributes}public partial record {table.ClassName} : {extends}
{{
");
        sb.AppendLine(GetRecordProperties(table, props));
        sb.AppendLine(@$"
}}");
        sourceFiles.Add(($"{rootNamespace}.{table.ClassName}.g", sb.ToString()));
    }

    protected virtual (string Extends, IEnumerable<ColumnProperty> FilteredProps) GetInterfaceExtends(TableModel table, IEnumerable<ColumnProperty> props)
    {
        var extends = new StringBuilder();

        extends.Append($"IEntity<{table.ClassName}>");
        extends.Append($", IProjectEntity");

        var props2 = props.ToList();

        if (props.Any(i => i.PropertyName.Equals("CreatedDate")))
        {
            extends.Append(", ICreatedDateEntity");
            props2 = props2.Where(i => i.PropertyName != "CreatedDate").ToList();
        }

        if (props.Any(i => i.PropertyName.Equals("CreatedBy")))
        {
            extends.Append(", ICreatedByEntity");
            props2 = props2.Where(i => i.PropertyName != "CreatedBy").ToList();
        }

        if (props.Any(i => i.PropertyName.Equals("ModifiedDate")))
        {
            extends.Append(", IModifiedDateEntity");
            props2 = props2.Where(i => i.PropertyName != "ModifiedDate").ToList();
        }

        if (props.Any(i => i.PropertyName.Equals("ModifiedBy")))
        {
            extends.Append(", IModifiedByEntity");
            props2 = props2.Where(i => i.PropertyName != "ModifiedBy").ToList();
        }

        return (extends.ToString(), props2);
    }

    protected virtual string GetInterfaceProperties(TableModel table, IEnumerable<ColumnProperty> props)
    {
        var sb = new StringBuilder();
        foreach (var column in props)
        {
            sb.AppendLine(GetInterfaceProperty(table, column));
        }
        return sb.ToString();
    }

    protected virtual string GetInterfaceProperty(TableModel table, ColumnProperty column)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"    {column.PropertyType} {column.PropertyName} {{ get; set; }}");
        return sb.ToString();
    }

    protected virtual void AddInterface(TableModel table, IEnumerable<ColumnProperty> props, string rootNamespace, List<(string Name, string Code)> sourceFiles)
    {
        var (extends, props2) = GetInterfaceExtends(table, props);

        var sb = new StringBuilder();
        sb.AppendLine($@"#nullable enable
using Temelie.Entities;

namespace {rootNamespace};

public partial interface I{table.ClassName} : {extends}
{{
");
        sb.AppendLine(GetInterfaceProperties(table, props2));
        sb.AppendLine(@$"
}}");
        sourceFiles.Add(($"{rootNamespace}.I{table.ClassName}.g", sb.ToString()));
    }

    protected virtual IEnumerable<ColumnProperty> GetColumnProperties(TableModel table)
    {
        var list = new List<ColumnProperty>();

        foreach (var column in table.Columns.Where(i => !i.IsIgnored))
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

    protected virtual string GetTypeDefault(string propertyType)
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
