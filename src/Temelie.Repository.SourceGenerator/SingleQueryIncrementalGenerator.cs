using System.Text;
using Microsoft.CodeAnalysis;
using Temelie.Database.Extensions;

namespace Temelie.Repository.SourceGenerator;

[Generator]
public class SingleQueryIncrementalGenerator : IIncrementalGenerator
{

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var options = context.AnalyzerConfigOptionsProvider.Select(static (c, _) => { c.GlobalOptions.TryGetValue("build_property.RootNamespace", out string rootNamespace); return rootNamespace; });

        var compliationProvider = context.CompilationProvider.Select(static (compilation, token) =>
        {
            var visitor = new EntitySymbolVisitor(token);
            visitor.Visit(compilation.GlobalNamespace);
            return visitor.Entities;
        });

        context.RegisterSourceOutput(options.Combine(compliationProvider).WithComparer(new EntityListEqualityComparer()), Generate);
    }

    static void Generate(SourceProductionContext context, (string RootNamespace, IEnumerable<Entity> Entities) result)
    {
        foreach (var item in result.Entities)
        {
            context.CancellationToken.ThrowIfCancellationRequested();
            var generated = Generate(result.RootNamespace, item);
            if (!string.IsNullOrEmpty(generated.Name))
            {
                context.AddSource(generated.Name, generated.Code);
            }
        }
    }

    public static (string Name, string Code) Generate(string rootNamespace, Entity entity)
    {
        if (entity.IsView)
        {
            return default;
        }

        var ns = rootNamespace;

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
           return($"{entity.FullType}.SingleQuery.g", sb.ToString());
        }

        return default;
    }

}
