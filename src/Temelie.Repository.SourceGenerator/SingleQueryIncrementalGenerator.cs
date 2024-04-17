using System.Text;
using Microsoft.CodeAnalysis;
using Temelie.Database.Extensions;

namespace Temelie.Repository.SourceGenerator;

[Generator]
public class SingleQueryIncrementalGenerator : IIncrementalGenerator
{

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var compliationProvider = context.CompilationProvider.Select(static (compilation, token) =>
        {
            var visitor = new EntitySymbolVisitor();
            visitor.Visit(compilation.GlobalNamespace);
            return (compilation.AssemblyName, visitor.Entities);
        });

        context.RegisterImplementationSourceOutput(compliationProvider, (context, results) =>
        {
            foreach (var result in results.Entities)
            {
                Generate(context, results.AssemblyName, result);
            }
        });
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
