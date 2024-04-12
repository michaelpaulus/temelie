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
        var sb = new StringBuilder();

        sb.AppendLine(@$"namespace AdventureWorks.Database.Models;
    public class DatabaseModel
    {{

        private static readonly Lazy<IEnumerable<TableModel>> _tables = new Lazy<IEnumerable<TableModel>>(() =>
        {{
            var tables = new List<TableModel>
            {{
                

");
        var isFirst = true;
        foreach (var file in files)
        {
            sb.Append($"JsonSerializer.Deserialize<TableModel>(@\"{file.Text.Replace("\"", "\\\"")}\")");
            if (!isFirst)
            {
                sb.AppendLine(",");
            }
            isFirst = false;
        }

        sb.AppendLine(@$"
            }};

            return tables;
        }});
        public static IEnumerable<TableModel> Tables => _tables.Value;
    }}
");

        //context.AddSource("DatabaseModel_Generated", sb.ToString());
    }

}
