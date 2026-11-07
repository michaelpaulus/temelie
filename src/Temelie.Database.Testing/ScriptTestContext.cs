using Temelie.Database.Models;
using Temelie.Database.Providers;
using Temelie.Database.Services;
using Temelie.Database.Testing.Fakes;
using MssqlProvider = Temelie.Database.Providers.Mssql.DatabaseProvider;
using MySqlProvider = Temelie.Database.Providers.MySql.DatabaseProvider;

namespace Temelie.Database.Testing;

/// <summary>The result of an <c>ExecuteScripts</c> run.</summary>
public sealed record ScriptRun(IReadOnlyList<ScriptProgress> Progress, Exception? Error)
{
    /// <summary>
    /// Scripts in the order they were started (plus renames and retries), excluding
    /// error reports, table-change and migration-required notices, and the final "Complete".
    /// </summary>
    public IReadOnlyList<string> Steps => Progress
        .Where(i => i.ErrorMessage is null && i.ProgressStatus != "Complete" &&
            !i.ProgressStatus.StartsWith("Table Change: ", StringComparison.Ordinal) &&
            !i.ProgressStatus.StartsWith("Migration Required: ", StringComparison.Ordinal))
        .Select(i => i.ProgressStatus)
        .ToList();

    public IReadOnlyList<string> Failed => Progress.Where(i => i.ErrorMessage is not null).Select(i => i.ProgressStatus).ToList();

    public IReadOnlyList<string> TableChanges => Progress
        .Where(i => i.ProgressStatus.StartsWith("Table Change: ", StringComparison.Ordinal))
        .Select(i => i.ProgressStatus["Table Change: ".Length..])
        .Distinct()
        .ToList();

    /// <summary>Schema-bound or indexed-view changes that were not applied because they need a migration.</summary>
    public IReadOnlyList<string> MigrationsRequired => Progress
        .Where(i => i.ProgressStatus.StartsWith("Migration Required: ", StringComparison.Ordinal))
        .Select(i => i.ProgressStatus["Migration Required: ".Length..])
        .ToList();
}

/// <summary>Runs <see cref="ScriptService"/> against a fake database and a temporary script folder.</summary>
public sealed class ScriptTestContext : IDisposable
{
    private ScriptTestContext(IDatabaseProvider provider, string connectionString)
    {
        Provider = provider;
        Connection = new ConnectionStringModel { Name = "Test", DatabaseProviderName = provider.Name, ConnectionString = connectionString };
    }

    public static ScriptTestContext SqlServer() => new(new MssqlProvider([], []), "Data Source=.;Initial Catalog=Test;Integrated Security=True");

    public static ScriptTestContext MySql() => new(new MySqlProvider([], []), "Server=localhost;Database=test");

    public static ScriptTestContext For(string providerName) => providerName switch
    {
        _ when providerName == MssqlProvider.ProviderName => SqlServer(),
        _ when providerName == MySqlProvider.ProviderName => MySql(),
        _ => throw new ArgumentException($"Unknown provider '{providerName}'.", nameof(providerName))
    };

    public IDatabaseProvider Provider { get; }

    public ConnectionStringModel Connection { get; }

    public ScriptFolder Folder { get; } = new();

    public ScriptService CreateService(FakeDatabase database) => new(new DatabaseFactory([Provider], []), database, database);

    /// <summary>Runs <c>CreateScripts</c> for a database described by <paramref name="describe"/>.</summary>
    public FakeDatabase Generate(Action<DatabaseState> describe, string objectFilter = "", IDatabaseProvider? createScriptsProvider = null)
    {
        var database = new FakeDatabase();
        describe(database.State);
        CreateService(database).CreateScripts(Connection, Folder.Directory, null, objectFilter, createScriptsProvider);
        return database;
    }

    /// <summary>Runs <c>ExecuteScripts</c> against <paramref name="database"/>.</summary>
    public ScriptRun Execute(FakeDatabase database, bool continueOnError = true, bool skipMigrations = false) =>
        Execute(database, Folder.Directory, continueOnError, skipMigrations);

    /// <summary>Runs <c>ExecuteScripts</c> for <paramref name="directory"/> against <paramref name="database"/>.</summary>
    public ScriptRun Execute(FakeDatabase database, DirectoryInfo directory, bool continueOnError = true, bool skipMigrations = false)
    {
        var progress = new List<ScriptProgress>();
        Exception? error = null;
        try
        {
            CreateService(database).ExecuteScripts(Connection, directory, progress.Add, continueOnError, skipMigrations);
        }
        catch (Exception ex)
        {
            error = ex;
        }

        return new ScriptRun(progress, error);
    }

    /// <summary>Creates a fake database whose current schema is described by <paramref name="describe"/>.</summary>
    public static FakeDatabase Database(Action<DatabaseState>? describe = null)
    {
        var database = new FakeDatabase();
        describe?.Invoke(database.State);
        return database;
    }

    public void Dispose() => Folder.Dispose();
}
