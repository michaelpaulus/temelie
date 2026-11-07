# Temelie.SourceGenerator

Runs the Temelie generators outside the compiler and writes their output to disk. It opens a solution with Roslyn's `MSBuildWorkspace`, generates the code, and keeps a folder of `.cs` files in sync. Use it when you want generated code checked in, reviewed in pull requests, or available without building.

```shell
dotnet add package Temelie.SourceGenerator
```

```csharp
var generator = new FileGenerator();

await generator.GenerateEntitiesAsync("MyApp.sln", "MyApp.Database", "MyApp.Entities");                          // entities from .sql.json
await generator.GenerateEntityFrameworkCoreAsync("MyApp.sln", "MyApp.Entities", "MyApp.Data", "_Generated/EntityFrameworkCore");
await generator.GenerateRepositoryAsync("MyApp.sln", "MyApp.Entities", "MyApp.Data", "_Generated/Repository");
```

Each method has three variants:

- **Solution path:** pass the path to the solution, as shown above.
- **Open workspace:** pass an already-open `MSBuildWorkspace` to share one solution load across calls.
- **Custom entity generator:** pass your own `EntityIncrementalGeneratorBase` (see [Temelie.Entities.SourceGeneratorBase](https://github.com/michaelpaulus/temelie/blob/main/src/Temelie.Entities.SourceGeneratorBase/README.md)).

Output defaults to a `_Generated` folder in the target project. Changed files are rewritten and stale files are deleted.

`src/Temelie.SourceGenerator.App` in the repository is a runnable example that generates the AdventureWorks sample.
