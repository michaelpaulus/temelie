using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Cornerstone.Repository.EntityFrameworkCore.UnitTests;

public partial class TestDbContext : ExampleDbContext
{
    private static readonly ILoggerFactory _loggerFactory = LoggerFactory.Create(builder => { builder.AddConsole(); });
    private readonly SqliteConnection _connection;

#pragma warning disable CS8618
    public TestDbContext(SqliteConnection connection)
#pragma warning restore CS8618
    {
        _connection = connection;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        options.UseSqlite(_connection)
            .EnableSensitiveDataLogging()
            .UseLoggerFactory(_loggerFactory);
    }

}
