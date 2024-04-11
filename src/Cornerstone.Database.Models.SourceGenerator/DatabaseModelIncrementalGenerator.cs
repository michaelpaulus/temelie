using Microsoft.CodeAnalysis;

namespace Cornerstone.Database.Models.SourceGenerator;

[Generator]
public class DatabaseModelIncrementalGenerator : IIncrementalGenerator
{

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var files = context.AdditionalTextsProvider
            .Where(a => a.Path.EndsWith(".sql.json"))
            .Select((a, c) => (Path.GetFileNameWithoutExtension(a.Path), a.GetText(c)!.ToString()));

        context.RegisterSourceOutput(files, Generate);
    }

    void Generate(SourceProductionContext context, (string FileName, string Text) file)
    {
        context.AddSource(file.FileName, @$"namespace AdventureWorks.Database.Models;
    public class {file.FileName.Replace(".", "_")}
    {{
    }}
");
    }

}
