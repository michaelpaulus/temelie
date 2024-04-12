using System.Collections.Immutable;
using System.Text;
using Microsoft.CodeAnalysis;

namespace Cornerstone.Database.Models.SourceGenerator;

[Generator]
public class DatabaseModelIncrementalGenerator : IIncrementalGenerator
{

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var files = context.AdditionalTextsProvider
            .Where(a => a.Path.EndsWith(".sql.json"))
            .Select((a, c) => (Path.GetFileNameWithoutExtension(a.Path), a.GetText(c)!.ToString()))
            .Collect();

        context.RegisterSourceOutput(files, Generate);
    }

    void Generate(SourceProductionContext context, ImmutableArray<(string FileName, string Text)> files)
    {
        var databaseModel = DatabaseModel.CreateFromFiles(files);

        foreach (var table in databaseModel.Tables)
        {
            var ns = "AdventureWorks.Entities";
            var className = table.TableName;

            var sb = new StringBuilder();
            sb.AppendLine(@$"namespace {ns};
    public record {className} : IEntity<{className}>
    {{
");
            foreach (var column in table.Columns)
            {
                var propertyName = column.ColumnName;
                var propertyType = column.ColumnType;
                sb.AppendLine($"    public {propertyType} {propertyName} {{ get; set; }};");
            }

            sb.AppendLine(@$"
    }}
");
            context.AddSource($"{ns}.{className}_Generated", sb.ToString());
        }

    }

}
