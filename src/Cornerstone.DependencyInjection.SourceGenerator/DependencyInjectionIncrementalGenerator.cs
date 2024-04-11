using System.Text;
using Microsoft.CodeAnalysis;

namespace Cornerstone.DependencyInjection.SourceGenerator;

[Generator]
public class DependencyInjectionIncrementalGenerator : IIncrementalGenerator
{

    public virtual void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var compliationProvider = context.CompilationProvider.Select(static (compilation, token) =>
        {
            var visitor = new DependencyInjectionSymbolVisitor();
            visitor.Visit(compilation.GlobalNamespace);
            return visitor.Symbols;
        });

        context.RegisterImplementationSourceOutput(compliationProvider, (context, value) =>
        {
            if (value.Any())
            {
                var contents = Generate(value);
                context.AddSource(nameof(DependencyInjectionIncrementalGenerator), contents);
            }
        });
    }

    public static string Generate(IEnumerable<INamedTypeSymbol> symbols)
    {
        var exports = new List<Export>();

        var sb = new StringBuilder();

        sb.Append(@"namespace Microsoft.Extensions.DependencyInjection;

internal static class Cornerstone_DependencyInjection_IncrementalGenerator
{

    internal static void RegisterExports(this IServiceCollection services)
    {
");

        foreach (var symbol in symbols)
        {
            var attribute = symbol.GetAttributes().FirstOrDefault(ad => ad.AttributeClass.Name == "ExportProviderAttribute" ||
            ad.AttributeClass.Name == "ExportSingletonAttribute" ||
            ad.AttributeClass.Name == "ExportTransientAttribute");

            if (attribute != null)
            {
                var forType = attribute.ConstructorArguments[0].Value as INamedTypeSymbol;

                var export = new Export()
                {
                    IsProvider = attribute.AttributeClass.Name == "ExportProviderAttribute",
                    IsSingleton = attribute.AttributeClass.Name == "ExportSingletonAttribute",
                    IsTransient = attribute.AttributeClass.Name == "ExportTransientAttribute",
                    Type = symbol.FullName(),
                    ForType = forType.FullName()
                };
                     
                if (attribute.NamedArguments.Any(i => i.Key == "Priority"))
                {
                    export.Priority = Convert.ToInt32(attribute.NamedArguments.First(i => i.Key == "Priority").Value.Value);
                }

                exports.Add(export);
            }
        }

        foreach (var export in exports.OrderBy(i => i.ForType).GroupBy(i => new { i.ForType, i.IsProvider }))
        {
            var list = new List<Export>();

            if (export.Count() > 1)
            {
                if (export.Key.IsProvider)
                {
                    list.AddRange(export);
                }
                else
                {
                    list.Add(export.OrderBy(i => i.Priority).First());
                }
            }
            else
            {
                list.Add(export.First());
            }

            foreach (var item in list)
            {
                if (item.IsProvider || item.IsTransient)
                {
                    sb.AppendLine($"        services.AddTransient<{item.ForType}, {item.Type}>();");
                }
                if (item.IsSingleton)
                {
                    sb.AppendLine($"        services.AddSingleton<{item.ForType}, {item.Type}>();");
                }
            }
        }

        sb.Append(@"
    }
}");

        return sb.ToString();
    }

}
