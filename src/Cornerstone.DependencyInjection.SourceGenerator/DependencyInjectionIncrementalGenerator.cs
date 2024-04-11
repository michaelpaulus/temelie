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
        var startupConfigurations = new List<string>();
        var hostedServices = new List<string>();

        foreach (var symbol in symbols)
        {
            var attribute = symbol.GetAttributes().FirstOrDefault(ad => ad.AttributeClass.Name == "ExportProviderAttribute" ||
            ad.AttributeClass.Name == "ExportSingletonAttribute" ||
            ad.AttributeClass.Name == "ExportTransientAttribute");

            if (attribute is not null)
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

            attribute = symbol.GetAttributes().FirstOrDefault(ad => ad.AttributeClass.Name == "ExportHostedServiceAttribute");

            if (attribute is not null)
            {
                hostedServices.Add(symbol.FullName());
            }

            attribute = symbol.GetAttributes().FirstOrDefault(ad => ad.AttributeClass.Name == "ExportStartupConfigurationAttribute");

            if (attribute is not null)
            {
                startupConfigurations.Add(symbol.FullName());
            }

        }


        var sb = new StringBuilder();

        sb.Append(@"namespace Microsoft.Extensions.DependencyInjection;

internal static class Cornerstone_DependencyInjection_IncrementalGenerator
{
");

        regsiterExports();

        startupConfiguration1();
        startupConfiguration2();
        startupConfiguration3();

        sb.Append(@"
}");

        void regsiterExports()
        {
            sb.Append(@"
    internal static void RegisterExports(this IServiceCollection services)
    {
");
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

            foreach (var hostedService in hostedServices)
            {
                sb.AppendLine($"        services.AddHostedService<{hostedService}>();");
            }

            sb.Append(@"
    }
");
        }

        void startupConfiguration1()
        {
            sb.Append(@"
    internal static void ConfigureStartup(this Microsoft.Extensions.Configuration.IConfigurationBuilder configuration)
    {
");
            foreach (var startupConfig in startupConfigurations)
            {
                sb.AppendLine($"        new {startupConfig}().Configure(configuration);");
            }

            sb.Append(@"
    }
");
        }

        void startupConfiguration2()
        {
            sb.Append(@"
    internal static void ConfigureStartup(this Microsoft.Extensions.DependencyInjection.IServiceCollection services)
    {
");
            foreach (var startupConfig in startupConfigurations)
            {
                sb.AppendLine($"        new {startupConfig}().Configure(services);");
            }

            sb.Append(@"
    }
");
        }

        void startupConfiguration3()
        {
            sb.Append(@"
    internal static void ConfigureStartup(this System.IServiceProvider provider)
    {
");
            foreach (var startupConfig in startupConfigurations)
            {
                sb.AppendLine($"        new {startupConfig}().Configure(provider);");
            }

            sb.Append(@"
    }
");
        }

        return sb.ToString();
    }

}
