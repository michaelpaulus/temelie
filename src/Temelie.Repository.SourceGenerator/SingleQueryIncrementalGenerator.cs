using System.Text;
using Microsoft.CodeAnalysis;
using Temelie.Database.Extensions;

namespace Temelie.Repository.SourceGenerator;

[Generator]
public class SingleQueryIncrementalGenerator : IIncrementalGenerator
{

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var options = context.AnalyzerConfigOptionsProvider.Select((c, _) => { c.GlobalOptions.TryGetValue("build_property.RootNamespace", out string rootNamespace); return rootNamespace; });

        var compliationProvider = context.CompilationProvider.Select(static (compilation, token) =>
        {
            var visitor = new EntitySymbolVisitor();
            visitor.Visit(compilation.GlobalNamespace);
            return visitor.Entities;
        });

        context.RegisterSourceOutput(options.Combine(compliationProvider), Generate);
    }

    private void Generate(SourceProductionContext context, (string RootNamespace, IEnumerable<Entity> Entities) result)
    {
        foreach (var item in result.Entities)
        {
            Generate(context, result.RootNamespace, item);
        }
    }

    void Generate(SourceProductionContext context, string assemblyName, Entity entity)
    {
        if (entity.IsView)
        {
            return;
        }

        var ns = assemblyName;

        var keys = entity.Properties.Where(i => i.IsPrimaryKey).ToList();

        if (keys.Count > 0)
        {
            var sb = new StringBuilder();

            sb.AppendLine($@"using Temelie.Repository;

namespace {ns};

public class {entity.Name}SingleQuery({string.Join(", ", keys.Select(i => $"{i.FullType} {i.Name.ToCamelCase()}"))}) : IQuerySpec<{entity.FullType}>
{{
    public IQueryable<{entity.FullType}> Apply(IQueryable<{entity.FullType}> query)
    {{
        return query.Where(i => {string.Join(" && ", keys.Select(i => $"i.{i.Name} == {i.Name.ToCamelCase()}"))});
    }}
}}
");

            context.AddSource($"{entity.FullType}.SingleQuery.g", sb.ToString());
        }
    }

}
