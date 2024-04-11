using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Cornerstone.DependencyInjection;
public interface IStartupConfiguration
{
    IConfigurationBuilder Configure(IConfigurationBuilder builder);
    IServiceCollection Configure(IServiceCollection services);
    IServiceProvider Configure(IServiceProvider provider);
}
