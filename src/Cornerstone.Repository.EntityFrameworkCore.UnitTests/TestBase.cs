using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Cornerstone.Repository.EntityFrameworkCore.UnitTests;

public class TestBase
{

    private readonly Lazy<IServiceProvider> _serviceProvider = new Lazy<IServiceProvider>(() =>
    {
        IServiceCollection services = new ServiceCollection();

        var configuration = new ConfigurationBuilder();

        configuration.ConfigureStartup();

        services.AddSingleton<IConfiguration>(configuration.Build());

        services.RegisterExports();

        services.ConfigureStartup();

        var provider = services.BuildServiceProvider();

        provider.ConfigureStartup();

        return provider;
    });

    protected IServiceProvider ServiceProvider => _serviceProvider.Value;

}
