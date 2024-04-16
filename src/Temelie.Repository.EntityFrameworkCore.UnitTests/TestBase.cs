using AdventureWorks.Server.Repository.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Temelie.Repository.EntityFrameworkCore.UnitTests;

public class TestBase
{

#pragma warning disable NUnit1032 // An IDisposable field/property should be Disposed in a TearDown method
    protected IServiceProvider ServiceProvider { get; private set; }
#pragma warning restore NUnit1032 // An IDisposable field/property should be Disposed in a TearDown method

    [SetUp]

    public Task Setup()
    {
        IServiceCollection services = new ServiceCollection();

        var configuration = new ConfigurationBuilder();

        configuration.ConfigureStartup();

        services.AddSingleton<IConfiguration>(configuration.Build());

        services.RegisterExports();

        services.ConfigureStartup();

        ServiceProvider = services.BuildServiceProvider();

        ServiceProvider.ConfigureStartup();

        return Task.CompletedTask;
    }

    [TearDown]
    public Task TearDownAsync()
    {
        if (ServiceProvider is not null && ServiceProvider is IDisposable disposable)
        {
            disposable.Dispose();
        }
        return Task.CompletedTask;
    }
}
