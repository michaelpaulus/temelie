using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Temelie.DependencyInjection;
public interface IStartupConfiguration
{
    IConfigurationBuilder Configure(IConfigurationBuilder builder);
    IServiceCollection Configure(IServiceCollection services);
    IServiceProvider Configure(IServiceProvider provider);
}
