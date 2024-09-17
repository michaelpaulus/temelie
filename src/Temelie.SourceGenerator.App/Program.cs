using Temelie.SourceGenerator;
using Temelie.SourceGenerator.Configuration;

var fileGenerator = new FileGenerator(); 

await fileGenerator.GenerateEntitiesAsync(Path.Combine(DirectoryConfig.RepoDirectory, "Temelie.sln"), "AdventureWorks.Database", "AdventureWorks.Entities");
await fileGenerator.GenerateEntityFrameworkCoreAsync(Path.Combine(DirectoryConfig.RepoDirectory, "Temelie.sln"), "AdventureWorks.Entities", "AdventureWorks.Server.Repository");
await fileGenerator.GenerateRepositoryAsync(Path.Combine(DirectoryConfig.RepoDirectory, "Temelie.sln"), "AdventureWorks.Entities", "AdventureWorks.Server.Repository");
