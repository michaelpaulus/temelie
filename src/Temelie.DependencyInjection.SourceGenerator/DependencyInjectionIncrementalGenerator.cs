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
        string regsiterExports()
        {
            var sb = new StringBuilder();
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
            return sb.ToString();
        }

        var sb = new StringBuilder();
        sb.Append(@$"using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using Temelie.DependencyInjection;

namespace Temelie.DependencyInjection;

internal class StartupConfigurationContext : IDisposable, IStartupConfiguration
{{
    private readonly IEnumerable<IStartupConfiguration> _configurations;

    public StartupConfigurationContext()
    {{
        _configurations = new List<IStartupConfiguration>() {{ {string.Join(", ", symbols.Where(i => i.IsStartupConfig).Select(i => $"new {i.Type}()"))} }};
    }}

    public IConfigurationBuilder Configure(IConfigurationBuilder builder)
    {{
        foreach (var config in _configurations)
        {{
            builder = config.Configure(builder);
        }}
        return builder;
    }}

    public IServiceCollection Configure(IServiceCollection services, IConfiguration configuration)
    {{
        foreach (var config in _configurations)
        {{
            services = config.Configure(services, configuration);
        }}
{regsiterExports()}
        return services;
    }}

    public IServiceProvider Configure(IServiceProvider provider)
    {{
        foreach (var config in _configurations)
        {{
            provider = config.Configure(provider);
        }}
        return provider;
    }}

    public void Dispose()
    {{

    }}
}}
");

        return sb.ToString();
    }

}
