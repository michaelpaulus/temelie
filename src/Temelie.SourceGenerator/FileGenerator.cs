using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.MSBuild;
using Temelie.Entities.SourceGenerator;
using Temelie.Repository.SourceGenerator;

namespace Temelie.SourceGenerator;

public class FileGenerator
{

    static FileGenerator()
    {
        MSBuildLocator.RegisterDefaults();
    }

    public async Task GenerateEntitiesAsync(
       string solutionFile,
       string databaseProjectName,
       string entitiesProjectName,
       string outputPath = "_Generated")
    {
        await GenerateEntitiesAsync(new EntityIncrementalGenerator(),
            solutionFile,
            databaseProjectName,
            entitiesProjectName,
            outputPath).ConfigureAwait(false);
    }

    public async Task GenerateEntitiesAsync(
       EntityIncrementalGeneratorBase entityGenerator,
       string solutionFile,
       string databaseProjectName,
       string entitiesProjectName,
       string outputPath = "_Generated")
    {
        using var workspace = MSBuildWorkspace.Create();

        Console.WriteLine($"{DateTime.UtcNow.ToString("yyyy-MM-ddThh:mm:ss")}: Load Started");

        _ = await workspace.OpenSolutionAsync(solutionFile).ConfigureAwait(false);

        Console.WriteLine($"{DateTime.UtcNow.ToString("yyyy-MM-ddThh:mm:ss")}: Load Finished");

        await GenerateEntitiesAsync(entityGenerator, workspace, databaseProjectName, entitiesProjectName, outputPath).ConfigureAwait(false);
    }

    public async Task GenerateEntitiesAsync(
        EntityIncrementalGeneratorBase entityGenerator,
        MSBuildWorkspace workspace,
        string databaseProjectName,
        string entitiesProjectName,
        string outputPath = "_Generated")
    {
        Console.WriteLine($"{DateTime.UtcNow.ToString("yyyy-MM-ddThh:mm:ss")}: GenerateEntitiesAsync Started");

        IEnumerable<Project> projects = workspace.CurrentSolution.Projects;

        var databaseProject = projects.FirstOrDefault(p => p.Name.Equals(databaseProjectName));
        var entitiesProject = projects.FirstOrDefault(p => p.Name.Equals(entitiesProjectName));

        if (databaseProject is not null && entitiesProject is not null)
        {
            Console.WriteLine($"{DateTime.UtcNow.ToString("yyyy-MM-ddThh:mm:ss")}: Creating Starting");

            var jsonFiles = databaseProject.AdditionalDocuments.Where(i => i.Name.EndsWith(".sql.json")).ToList();

            var pendingFiles = new List<(string Name, string Code)>();

            if (jsonFiles.Count > 0)
            {
                var compilation = await entitiesProject.GetCompilationAsync().ConfigureAwait(false);

                var visitor = new PartialPropertySymbolVisitor(CancellationToken.None);
                visitor.Visit(compilation!.GlobalNamespace);
                var partialProperties = visitor.PartialPropertyModels;

                var files = jsonFiles.Select(i => (i.FilePath, i.GetTextAsync().Result.ToString())).ToList();
                pendingFiles.AddRange(entityGenerator.Generate(entitiesProject.DefaultNamespace!, files, partialProperties));
            }

            await SyncFilesAsync(entitiesProject, outputPath, pendingFiles).ConfigureAwait(false);

            Console.WriteLine($"{DateTime.UtcNow.ToString("yyyy-MM-ddThh:mm:ss")}: Creating Finished");
        }

        Console.WriteLine($"{DateTime.UtcNow.ToString("yyyy-MM-ddThh:mm:ss")}: GenerateEntitiesAsync Finished");
    }

    public async Task GenerateEntityFrameworkCoreAsync(string solutionFile,
        string entitiesProjectName,
        string entityFrameworkCoreProjectName,
         string outputPath = "_Generated")
    {
        using var workspace = MSBuildWorkspace.Create();

        Console.WriteLine($"{DateTime.UtcNow.ToString("yyyy-MM-ddThh:mm:ss")}: Load Started");

        _ = await workspace.OpenSolutionAsync(solutionFile).ConfigureAwait(false);

        Console.WriteLine($"{DateTime.UtcNow.ToString("yyyy-MM-ddThh:mm:ss")}: Load Finished");

        await GenerateEntityFrameworkCoreAsync(workspace, entitiesProjectName, entityFrameworkCoreProjectName, outputPath).ConfigureAwait(false);
    }

    public async Task GenerateEntityFrameworkCoreAsync(MSBuildWorkspace workspace,
        string entitiesProjectName,
        string entityFrameworkCoreProjectName,
         string outputPath = "_Generated")
    {
        Console.WriteLine($"{DateTime.UtcNow.ToString("yyyy-MM-ddThh:mm:ss")}: GenerateEntityFrameworkCoreAsync Started");

        IEnumerable<Project> projects = workspace.CurrentSolution.Projects;

        var entityFrameworkCoreProject = projects.FirstOrDefault(p => p.Name.Equals(entityFrameworkCoreProjectName));
        var entitiesProject = projects.FirstOrDefault(p => p.Name.Equals(entitiesProjectName));

        if (entityFrameworkCoreProject is not null && entitiesProject is not null)
        {
            Console.WriteLine($"{DateTime.UtcNow.ToString("yyyy-MM-ddThh:mm:ss")}: Creating Starting");

            var compilation = await entitiesProject.GetCompilationAsync().ConfigureAwait(false);

            var visitor = new EntitySymbolVisitor(CancellationToken.None);
            visitor.Visit(compilation!.GlobalNamespace);
            var entities = visitor.Entities;

            var pendingFiles = new List<(string Name, string Code)>();

            pendingFiles.AddRange(DbContextIncrementalGenerator.Generate(entityFrameworkCoreProject.DefaultNamespace!, entities));

            await SyncFilesAsync(entityFrameworkCoreProject, outputPath, pendingFiles).ConfigureAwait(false);

            Console.WriteLine($"{DateTime.UtcNow.ToString("yyyy-MM-ddThh:mm:ss")}: Creating Finished");
        }

        Console.WriteLine($"{DateTime.UtcNow.ToString("yyyy-MM-ddThh:mm:ss")}: GenerateEntityFrameworkCoreAsync Finished");
    }

    public async Task GenerateRepositoryAsync(string solutionFile,
    string entitiesProjectName,
    string repositoryProjectName,
    string outputPath = "_Generated")
    {
        using var workspace = MSBuildWorkspace.Create();

        Console.WriteLine($"{DateTime.UtcNow.ToString("yyyy-MM-ddThh:mm:ss")}: Load Started");

        _ = await workspace.OpenSolutionAsync(solutionFile).ConfigureAwait(false);

        Console.WriteLine($"{DateTime.UtcNow.ToString("yyyy-MM-ddThh:mm:ss")}: Load Finished");

        await GenerateRepositoryAsync(workspace, entitiesProjectName, repositoryProjectName, outputPath).ConfigureAwait(false);
    }

    public async Task GenerateRepositoryAsync(MSBuildWorkspace workspace,
      string entitiesProjectName,
      string repositoryProjectName,
      string outputPath = "_Generated")
    {

        Console.WriteLine($"{DateTime.UtcNow.ToString("yyyy-MM-ddThh:mm:ss")}: GenerateRepositoryAsync Started");

        IEnumerable<Project> projects = workspace.CurrentSolution.Projects;

        var repositoryProject = projects.FirstOrDefault(p => p.Name.Equals(repositoryProjectName));
        var entitiesProject = projects.FirstOrDefault(p => p.Name.Equals(entitiesProjectName));

        if (repositoryProject is not null && entitiesProject is not null)
        {
            Console.WriteLine($"{DateTime.UtcNow.ToString("yyyy-MM-ddThh:mm:ss")}: Creating Starting");

            var compilation = await entitiesProject.GetCompilationAsync().ConfigureAwait(false);

            var visitor = new EntitySymbolVisitor(CancellationToken.None);
            visitor.Visit(compilation!.GlobalNamespace);
            var entities = visitor.Entities;

            var pendingFiles = new List<(string Name, string Code)>();

            foreach (var entity in entities)
            {
                pendingFiles.AddRange(SingleQueryIncrementalGenerator.Generate(repositoryProject.DefaultNamespace!, entity));
            }

            await SyncFilesAsync(repositoryProject, outputPath, pendingFiles).ConfigureAwait(false);

            Console.WriteLine($"{DateTime.UtcNow.ToString("yyyy-MM-ddThh:mm:ss")}: Creating Finished");
        }

        Console.WriteLine($"{DateTime.UtcNow.ToString("yyyy-MM-ddThh:mm:ss")}: GenerateRepositoryAsync Finished");
    }

    private async Task SyncFilesAsync(Project project, string outputPath, IEnumerable<(string Name, string Code)> pendingFiles)
    {
        var outDirectory = new DirectoryInfo(Path.Combine(new FileInfo(project.FilePath!).Directory!.FullName, outputPath));

        if (!outDirectory.Exists)
        {
            outDirectory.Create();
        }

        var existingFiles = project.Documents.Where(i => !string.IsNullOrEmpty(i.FilePath) && i.FilePath.StartsWith(outDirectory.FullName)).ToList();

        foreach (var (name, code) in pendingFiles)
        {
            var csName = name + ".cs";

            var file = existingFiles.FirstOrDefault(i => i.Name.Equals(csName));
            var filePath = Path.Combine(outDirectory.FullName, csName);

            if (file is not null)
            {
                existingFiles.Remove(file);
#pragma warning disable RS1035 // Do not use APIs banned for analyzers
                var existingFile = System.IO.File.ReadAllText(filePath);
#pragma warning restore RS1035 // Do not use APIs banned for analyzers
                if (existingFile.Equals(code))
                {
                    continue;
                }
            }

#pragma warning disable RS1035 // Do not use APIs banned for analyzers
            await System.IO.File.WriteAllTextAsync(filePath, code, System.Text.Encoding.UTF8).ConfigureAwait(false);
#pragma warning restore RS1035 // Do not use APIs banned for analyzers
        }

        foreach (var file in existingFiles)
        {
#pragma warning disable RS1035 // Do not use APIs banned for analyzers
            System.IO.File.Delete(file.FilePath!);
#pragma warning restore RS1035 // Do not use APIs banned for analyzers
        }

    }

}
