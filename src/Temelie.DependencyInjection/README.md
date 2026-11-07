# Temelie.DependencyInjection

Attribute-based service registration for `Microsoft.Extensions.DependencyInjection`. Mark classes with `[Export...]` attributes, and [Temelie.DependencyInjection.SourceGenerator](https://github.com/michaelpaulus/temelie/blob/main/src/Temelie.DependencyInjection.SourceGenerator/README.md) generates the registrations at compile time, with no reflection or assembly scanning at runtime.

```shell
dotnet add package Temelie.DependencyInjection
```

## Attributes

| Attribute | Registers as |
| --- | --- |
| `[ExportTransient(typeof(IService))]` | `AddTransient` |
| `[ExportScoped(typeof(IService))]` | `AddScoped` |
| `[ExportSingleton(typeof(IService))]` | `AddSingleton` |
| `[ExportProvider(typeof(IService))]` | `AddTransient`, keeping **every** implementation; resolve them with `IEnumerable<IService>` |
| `[ExportHostedService]` | `AddHostedService<T>()` |
| `[ExportStartupConfiguration]` | A startup step (`IStartupConfiguration`) |

Each export attribute takes the service type in its constructor and supports these properties:

- **`Type`**: the implementation to register. Defaults to the attributed class.
- **`Priority`**: how competing registrations are resolved. When several non-provider exports target the same service type, the one with the **lowest** `Priority` wins. The default is `int.MaxValue`, so you can replace any library default by exporting your own implementation with a lower `Priority`.

```csharp
[ExportTransient(typeof(IClock))]
public class SystemClock : IClock { ... }

[ExportProvider(typeof(IChangeTrackingSourceProvider))]
public class MyChangeSource : IChangeTrackingSourceProvider { ... }

[ExportTransient(typeof(IClock), Priority = 1)]   // replaces SystemClock
public class FixedClock : IClock { ... }
```

## Startup configuration

`IStartupConfiguration` lets a library contribute configuration sources, services, and post-build setup:

```csharp
[ExportStartupConfiguration(Priority = 10)]
public class DatabaseStartup : IStartupConfiguration
{
    public IConfigurationBuilder Configure(IConfigurationBuilder builder) => builder.AddJsonFile("database.json", optional: true);

    public IServiceCollection Configure(IServiceCollection services, IConfiguration configuration) =>
        services.AddDbContextFactory<MyDbContext>();

    public IServiceProvider Configure(IServiceProvider provider) => provider;
}
```

Startup configurations need a parameterless constructor and run in ascending `Priority` order. See the [source generator](https://github.com/michaelpaulus/temelie/blob/main/src/Temelie.DependencyInjection.SourceGenerator/README.md) for how to run them.
