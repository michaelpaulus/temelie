using System.Text.Json.Nodes;
using Temelie.Database.Models;


namespace Temelie.Database.IntegrationTests;

/// <summary>
/// Runs the script process against real database script folders, configured with
/// <c>TEMELIE_REAL_WORLD_DATABASES</c> as <c>provider=path</c> entries separated by <c>;</c>.
/// </summary>
[Category("RealWorld")]
public class RealWorldScriptTests
{
    private const string Variable = "TEMELIE_REAL_WORLD_DATABASES";

    public static IEnumerable<TestCaseData> Databases()
    {
        var found = false;
        foreach (var entry in (Environment.GetEnvironmentVariable(Variable) ?? "").Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var parts = entry.Split('=', 2);
            if (parts.Length == 2 && Directory.Exists(parts[1]))
            {
                found = true;
                yield return new TestCaseData(parts[0], parts[1]).SetArgDisplayNames(parts[0], Path.GetFileName(parts[1].TrimEnd('\\', '/')));
            }
        }

        if (!found)
        {
            yield return new TestCaseData("", "").Ignore($"Set {Variable} to run against real database script folders.");
        }
    }

    [TestCaseSource(nameof(Databases))]
    public void GeneratingFromTheCommittedModelsReproducesTheCommittedScripts(string provider, string path)
    {
        using var context = ScriptTestContext.For(provider);
        foreach (var file in SourceFiles(path))
        {
            context.Folder.Write(Path.GetRelativePath(path, file), File.ReadAllText(file));
        }

        var before = Snapshot(context.Folder);
        var database = new FakeDatabase(DatabaseState.From(LoadModel(path)));

        context.CreateService(database).CreateScripts(context.Connection, context.Folder.Directory, null);

        var after = Snapshot(context.Folder);
        after.Keys.Except(before.Keys).Should().BeEmpty("no scripts should be added");
        before.Keys.Except(after.Keys).Should().BeEmpty("no scripts should be removed");
        var changed = before.Keys
            .Where(i => !Equivalent(i, before[i], after[i]))
            .Order(StringComparer.Ordinal)
            .ToList();
        changed.Should().BeEmpty("regenerating from the committed models should not change any script");
    }

    [TestCaseSource(nameof(Databases))]
    public void UpToDateDatabaseOnlyRunsScriptFolders(string provider, string path)
    {
        using var context = ScriptTestContext.For(provider);
        var database = new FakeDatabase(DatabaseState.From(LoadModel(path))).WithMigrations([.. MigrationIds(path)]);

        var run = context.Execute(database, new DirectoryInfo(path));

        run.Error.Should().BeNull();
        run.TableChanges.Should().BeEmpty();
        run.Steps.Should().Equal(ScriptFolderSteps(path));
        database.Migrations.Should().HaveCount(MigrationIds(path).Count);
    }

    [TestCaseSource(nameof(Databases))]
    public void FreshDeploymentRunsEveryMigrationOnceInFolderOrder(string provider, string path)
    {
        using var context = ScriptTestContext.For(provider);
        var database = new FakeDatabase(DatabaseState.From(LoadModel(path)));
        var migrations = MigrationIds(path);

        var run = context.Execute(database, new DirectoryInfo(path));

        run.Error.Should().BeNull();
        run.Steps.Where(migrations.Contains).Should().Equal(migrations);
        database.Migrations.Select(i => i.Id).Should().Equal(migrations);
        run.Steps.Where(i => i.StartsWith("CREATE ", StringComparison.Ordinal) || i.StartsWith("DROP ", StringComparison.Ordinal)).Should().BeEmpty();
    }

    private static IEnumerable<string> SourceFiles(string path) => Directory
        .GetFiles(path, "*", SearchOption.AllDirectories)
        .Where(i => Path.GetRelativePath(path, i).Split(Path.DirectorySeparatorChar)[0] is not ("bin" or "obj"));

    private static DatabaseModel LoadModel(string path) => DatabaseModel.CreateFromFiles(SourceFiles(path)
        .Where(i => i.EndsWith(".sql.json", StringComparison.OrdinalIgnoreCase))
        .Select(i => (i, File.ReadAllText(i))));

    private static Dictionary<string, string> Snapshot(ScriptFolder folder) => folder.Files()
        .Where(i => i.EndsWith(".sql", StringComparison.OrdinalIgnoreCase) || i.EndsWith(".sql.json", StringComparison.OrdinalIgnoreCase))
        .ToDictionary(i => i, folder.Read, StringComparer.Ordinal);

    private static bool Equivalent(string file, string before, string after)
    {
        if (file.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
        {
            return JsonNode.DeepEquals(JsonNode.Parse(before), JsonNode.Parse(after));
        }

        return before.Replace("\r\n", "\n").Trim('\n') == after.Replace("\r\n", "\n").Trim('\n');
    }

    /// <summary>Migration ids in the order ExecuteScripts runs them.</summary>
    private static List<string> MigrationIds(string path) => new DirectoryInfo(path).GetDirectories()
        .OrderBy(i => i.Name)
        .Where(i => i.Name.EndsWith("_Migrations", StringComparison.Ordinal))
        .SelectMany(folder => folder.GetDirectories()
            .OrderBy(i => i.Name)
            .SelectMany(migration => migration.GetFiles("*.sql")
                .OrderBy(i => i.FullName)
                .Select(file => $"{folder.Name}/{migration.Name}/{file.Name}")))
        .ToList();

    /// <summary>Scripts in folders that hold plain scripts (not migrations or object models), in run order.</summary>
    private static List<string> ScriptFolderSteps(string path) => new DirectoryInfo(path).GetDirectories()
        .OrderBy(i => i.Name)
        .Where(i => !i.Name.EndsWith("_Migrations", StringComparison.Ordinal) && i.GetFiles("*.sql.json").Length == 0)
        .SelectMany(folder => folder.GetFiles("*.sql")
            .OrderBy(i => i.FullName)
            .Select(file => $"{folder.Name}/{file.Name}"))
        .ToList();
}
