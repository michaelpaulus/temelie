# Temelie.DependencyInjection.SourceGenerator

Roslyn source generator that turns [Temelie.DependencyInjection](https://github.com/michaelpaulus/temelie/blob/main/src/Temelie.DependencyInjection/README.md) `[Export...]` attributes into service registrations and a startup pipeline.

```shell
dotnet add package Temelie.DependencyInjection
dotnet add package Temelie.DependencyInjection.SourceGenerator
```

Reference the generator from your **application** (the composition root). It discovers exported classes in your project **and in every referenced assembly**, so libraries such as `Temelie.Database.Services` only need the attributes, not the generator.

In-repo project reference:

```xml
<ProjectReference Include="..\Temelie.DependencyInjection.SourceGenerator\Temelie.DependencyInjection.SourceGenerator.csproj"
                  OutputItemType="Analyzer" ReferenceOutputAssembly="false" />
```

## Usage

The generator emits an internal `Temelie.DependencyInjection.StartupConfigurationContext`. Call it at the three stages of host startup:

```csharp
using Temelie.DependencyInjection;

using var context = new StartupConfigurationContext();
var builder = Host.CreateApplicationBuilder(args);

context.Configure(builder.Configuration);                    // IStartupConfiguration.Configure(IConfigurationBuilder)
context.Configure(builder.Services, builder.Configuration);  // startup services, then all [Export...] registrations

using var host = builder.Build();
context.Configure(host.Services);                             // IStartupConfiguration.Configure(IServiceProvider)

await host.RunAsync();
```

## Generated registrations

| Export | Generated registration |
| --- | --- |
| `[ExportTransient]`, `[ExportProvider]` | `services.AddTransient(typeof(ForType), typeof(Type))` |
| `[ExportScoped]` | `services.AddScoped(...)` |
| `[ExportSingleton]` | `services.AddSingleton(...)` |
| `[ExportHostedService]` | `services.AddHostedService<T>()` |

- **Competing exports:** for non-provider exports of the same service type, only the lowest `Priority` is registered. Providers are all registered.
- **Abstract classes** are ignored.
- **Startup configurations** are created with `new()` in ascending `Priority` order.

The generator targets `netstandard2.0` and is packaged under `analyzers/dotnet/cs`, so it works in both Visual Studio and the .NET SDK.
