using Microsoft.Extensions.DependencyInjection;
using Temelie.DependencyInjection;
using Temelie.Entities;

namespace Temelie.Repository;

[ExportSingleton(typeof(IRepositoryEventFactory))]
public class DefaultRepositoryEventFactory : IRepositoryEventFactory
{
    private readonly IServiceProvider _serviceProvider;

    public DefaultRepositoryEventFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public IEnumerable<IRepositoryEventProvider<Entity>> GetEventProviders<Entity>() where Entity : EntityBase, IEntity<Entity>
    {
        return _serviceProvider.GetServices<IRepositoryEventProvider<Entity>>();
    }
}
