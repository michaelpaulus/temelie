using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Temelie.DependencyInjection;

namespace Temelie.Repository.UnitTests;

public class TestBase
{

#pragma warning disable NUnit1032 // An IDisposable field/property should be Disposed in a TearDown method
    protected IServiceProvider ServiceProvider { get; private set; }
#pragma warning restore NUnit1032 // An IDisposable field/property should be Disposed in a TearDown method

    [SetUp]

    public Task Setup()
    {
        using var context = new StartupConfigurationContext();

        IServiceCollection services = new ServiceCollection();

        var configuration = new ConfigurationBuilder();

        context.Configure(configuration);

        var config = configuration.Build();

        services.AddSingleton<IConfiguration>(config);

        context.Configure(services, config);

        ServiceProvider = services.BuildServiceProvider();

        context.Configure(ServiceProvider);

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
