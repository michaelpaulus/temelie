using AdventureWorks.Server.Repository.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Temelie.DependencyInjection;

namespace Temelie.Repository.EntityFrameworkCore.UnitTests;
[ExportStartupConfiguration]
public class TestStartupConfiguration : IStartupConfiguration
{
    private bool _log;

    public IConfigurationBuilder Configure(IConfigurationBuilder builder)
    {
        return builder;
    }

    public IServiceCollection Configure(IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton(provider =>
        {
            var builder = new DbContextOptionsBuilder<ExampleDbContext>();
            var connection = new SqliteConnection("Filename=:memory:");
            connection.Open();
            builder.UseSqlite(connection)
                .EnableSensitiveDataLogging()
                .UseLoggerFactory(provider.GetRequiredService<ILoggerFactory>());
            return builder.Options;
        });

        services.AddDbContext<ExampleDbContext>();

        services.AddLogging(options =>
        {
            options.AddFilter((filter, filter1, level) =>
            {
                return _log && level == LogLevel.Information;
            });
            options.AddConsole();
        });

        return services;
    }

    public IServiceProvider Configure(IServiceProvider provider)
    {
        using (var context = provider.GetRequiredService<ExampleDbContext>())
        {
            context.Database.EnsureCreated();
        }
        _log = true;
        return provider;
    }
}
