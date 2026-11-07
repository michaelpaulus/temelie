using System.Data;
using System.Data.Common;
using System.Text.RegularExpressions;
using Temelie.Database.Models;
using Temelie.Database.Services;

namespace Temelie.Database.Testing.Fakes;

public sealed record MigrationRecord(string Id, bool Skipped);

/// <summary>
/// In-memory database used as both the model service (what the database looks like)
/// and the execution service (what gets run against it).
/// </summary>
public sealed partial class FakeDatabase : IDatabaseModelService, IDatabaseExecutionService
{
    private readonly List<(string Sql, Action<DatabaseState> Apply)> _effects = [];
    private readonly List<(Func<string, DatabaseState, bool> Fails, string Message)> _failures = [];

    public FakeDatabase(DatabaseState? state = null)
    {
        State = state ?? new DatabaseState();
    }

    public DatabaseState State { get; }

    /// <summary>Every script passed to <c>ExecuteFile</c>, in order.</summary>
    public List<string> Executed { get; } = [];

    public List<MigrationRecord> Migrations { get; } = [];

    /// <summary>Options passed to each <c>CreateModel</c> call (one entry per schema read).</summary>
    public List<DatabaseModelOptions?> ModelReads { get; } = [];

    /// <summary>Changes <see cref="State"/> when exactly this script is executed.</summary>
    public FakeDatabase WhenExecuted(string sql, Action<DatabaseState> apply)
    {
        _effects.Add((sql, apply));
        return this;
    }

    /// <summary>Makes matching scripts throw, based on the script and the current state.</summary>
    public FakeDatabase FailWhen(Func<string, DatabaseState, bool> fails, string message = "Simulated failure")
    {
        _failures.Add((fails, message));
        return this;
    }

    public FakeDatabase WithMigrations(params string[] ids)
    {
        Migrations.AddRange(ids.Select(i => new MigrationRecord(i, false)));
        return this;
    }

    public DatabaseModel CreateModel(ConnectionStringModel connectionString, DatabaseModelOptions? options = null)
    {
        ModelReads.Add(options);
        return State.ToModel();
    }

    public DatabaseModel CreateModel(DbConnection connection, DatabaseModelOptions? options = null) => CreateModel(new ConnectionStringModel(), options);

    public IEnumerable<TableModel> GetTables(DbConnection connection, DatabaseModelOptions options, IEnumerable<ColumnModel> columns) => State.Tables;

    public IEnumerable<ColumnModel> GetTableColumns(DbConnection connection) => State.Tables.SelectMany(i => i.Columns);

    public DbConnection CreateDbConnection(ConnectionStringModel connectionString)
    {
        var connection = new FakeDbConnection(RunMigrationCommand, RunMigrationQuery);
        connection.Open();
        return connection;
    }

    public DbCommand CreateDbCommand(DbConnection dbConnection) => dbConnection.CreateCommand();

    public void ExecuteFile(ConnectionStringModel connectionString, string sqlCommand)
    {
        Executed.Add(sqlCommand);
        foreach (var (fails, message) in _failures)
        {
            if (fails(sqlCommand, State))
            {
                throw new InvalidOperationException(message);
            }
        }

        foreach (var (sql, apply) in _effects)
        {
            if (sql == sqlCommand)
            {
                apply(State);
            }
        }
    }

    public void ExecuteFile(DbConnection connection, string sqlCommand) => ExecuteFile(new ConnectionStringModel(), sqlCommand);

    public void ExecuteNonQuery(DbConnection connection, string sqlCommand) => ExecuteFile(connection, sqlCommand);

    public DataSet Execute(DbConnection connection, string sqlCommand) => throw new NotSupportedException();

    public Task<IEnumerable<T>> GetRecordsAsync<T>(ConnectionStringModel connectionString, string query, params DbParameter[] parameters) => throw new NotSupportedException();

    private int RunMigrationCommand(string sql)
    {
        var match = InsertMigration().Match(sql);
        if (!match.Success)
        {
            throw new NotSupportedException($"Unexpected command: {sql}");
        }

        Migrations.Add(new MigrationRecord(match.Groups["id"].Value, match.Groups["skipped"].Value == "1"));
        return 1;
    }

    private DataTable RunMigrationQuery(string sql)
    {
        if (sql != "SELECT Id FROM Migrations")
        {
            throw new NotSupportedException($"Unexpected query: {sql}");
        }

        var table = new DataTable();
        table.Columns.Add("Id", typeof(string));
        foreach (var migration in Migrations)
        {
            table.Rows.Add(migration.Id);
        }

        return table;
    }

    [GeneratedRegex(@"^INSERT INTO Migrations \(Id, Date, Skipped\) VALUES \('(?<id>.*)', '[^']*', (?<skipped>[01])\)$")]
    private static partial Regex InsertMigration();
}
