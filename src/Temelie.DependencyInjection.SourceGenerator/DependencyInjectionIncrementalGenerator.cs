using System.Text;
using Microsoft.CodeAnalysis;

namespace Temelie.DependencyInjection.SourceGenerator;

[Generator]
public class DependencyInjectionIncrementalGenerator : IIncrementalGenerator
{

    public virtual void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var compliationProvider = context.CompilationProvider.Select(static (compilation, token) =>
        {
            var visitor = new DependencyInjectionSymbolVisitor();
            visitor.Visit(compilation.GlobalNamespace);
            return visitor.Exports;
        });

        context.RegisterImplementationSourceOutput(compliationProvider, (context, symbols) =>
        {
            if (symbols.Any())
            {
                var contents = Generate(symbols);
                context.AddSource(nameof(DependencyInjectionIncrementalGenerator), contents);
            }
        });
    }

    public static string Generate(IEnumerable<Export> symbols)
    {
        var sb = new StringBuilder();

        sb.Append(@"namespace Microsoft.Extensions.DependencyInjection;

internal static class Temelie_DependencyInjection_IncrementalGenerator
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
            foreach (var export in symbols.Where(i => (i.IsSingleton || i.IsProvider || i.IsTransient) && !string.IsNullOrEmpty(i.ForType) && !string.IsNullOrEmpty(i.Type)).OrderBy(i => i.ForType).GroupBy(i => new { i.ForType, i.IsProvider }))
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
                        sb.AppendLine($"        services.AddTransient(typeof({item.ForType}), typeof({item.Type}));");
                    }
                    if (item.IsSingleton)
                    {
                        sb.AppendLine($"        services.AddSingleton(typeof({item.ForType}), typeof({item.Type}));");
                    }
                }
            }

            foreach (var hostedService in symbols.Where(i => i.IsHosted))
            {
                sb.AppendLine($"        services.AddHostedService<{hostedService.Type}>();");
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
            foreach (var startupConfig in symbols.Where(i => i.IsStartupConfig))
            {
                sb.AppendLine($"        new {startupConfig.Type}().Configure(configuration);");
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
            foreach (var startupConfig in symbols.Where(i => i.IsStartupConfig))
            {
                sb.AppendLine($"        new {startupConfig.Type}().Configure(services);");
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
            foreach (var startupConfig in symbols.Where(i => i.IsStartupConfig))
            {
                sb.AppendLine($"        new {startupConfig.Type}().Configure(provider);");
            }

            sb.Append(@"
    }
");
        }

        return sb.ToString();
    }

}
