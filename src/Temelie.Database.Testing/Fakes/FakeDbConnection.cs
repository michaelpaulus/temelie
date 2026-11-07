using System.Data;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;

namespace Temelie.Database.Testing.Fakes;

/// <summary>
/// Minimal ADO.NET connection whose commands are answered by delegates.
/// </summary>
public sealed class FakeDbConnection(Func<string, int> executeNonQuery, Func<string, DataTable>? executeQuery = null) : DbConnection
{
    private ConnectionState _state;

    [AllowNull]
    public override string ConnectionString { get; set; } = "";
    public override string Database => "Fake";
    public override string DataSource => "Fake";
    public override string ServerVersion => "1.0";
    public override ConnectionState State => _state;

    public override void ChangeDatabase(string databaseName)
    {
    }

    public override void Close() => _state = ConnectionState.Closed;

    public override void Open() => _state = ConnectionState.Open;

    protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel) => throw new NotSupportedException();

    protected override DbCommand CreateDbCommand() => new FakeDbCommand(this);

    internal int RunNonQuery(string sql) => executeNonQuery(sql);

    internal DataTable RunQuery(string sql) => executeQuery?.Invoke(sql) ?? throw new NotSupportedException($"Unexpected query: {sql}");

    private sealed class FakeDbCommand(FakeDbConnection connection) : DbCommand
    {
        [AllowNull]
        public override string CommandText { get; set; } = "";
        public override int CommandTimeout { get; set; }
        public override CommandType CommandType { get; set; } = CommandType.Text;
        public override bool DesignTimeVisible { get; set; }
        public override UpdateRowSource UpdatedRowSource { get; set; }
        protected override DbConnection? DbConnection { get; set; } = connection;
        protected override DbParameterCollection DbParameterCollection => throw new NotSupportedException();
        protected override DbTransaction? DbTransaction { get; set; }

        public override void Cancel()
        {
        }

        public override int ExecuteNonQuery() => connection.RunNonQuery(CommandText);

        public override object? ExecuteScalar() => throw new NotSupportedException();

        public override void Prepare()
        {
        }

        protected override DbParameter CreateDbParameter() => throw new NotSupportedException();

        protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior) => connection.RunQuery(CommandText).CreateDataReader();
    }
}
