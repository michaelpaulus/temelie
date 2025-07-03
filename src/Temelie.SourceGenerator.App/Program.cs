using Microsoft.CodeAnalysis.MSBuild;
using Temelie.SourceGenerator;
using Temelie.SourceGenerator.Configuration;

var fileGenerator = new FileGenerator();

using var workspace = MSBuildWorkspace.Create();

Console.WriteLine($"{DateTime.UtcNow.ToString("yyyy-MM-ddThh:mm:ss")}: Load Started");

_ = await workspace.OpenSolutionAsync(Path.Combine(DirectoryConfig.RepoDirectory, "Temelie.sln")).ConfigureAwait(false);

Console.WriteLine($"{DateTime.UtcNow.ToString("yyyy-MM-ddThh:mm:ss")}: Load Finished");

await fileGenerator.GenerateEntitiesAsync(new Temelie.Entities.SourceGenerator.EntityIncrementalGenerator(), workspace, "AdventureWorks.Database", "AdventureWorks.Entities");
await fileGenerator.GenerateEntityFrameworkCoreAsync(workspace, "AdventureWorks.Entities", "AdventureWorks.Server.Repository", Path.Combine("_Generated", "EntityFrameworkCore"));
await fileGenerator.GenerateRepositoryAsync(workspace, "AdventureWorks.Entities", "AdventureWorks.Server.Repository", Path.Combine("_Generated", "Repository"));
