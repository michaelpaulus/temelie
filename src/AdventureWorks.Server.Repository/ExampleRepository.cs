using Microsoft.Extensions.DependencyInjection;
using Temelie.DependencyInjection;
using Temelie.Repository;

namespace AdventureWorks.Server.Repository;
[ExportTransient(typeof(IRepository), Type = typeof(ExampleRepository))]
public class ExampleRepository : RepositoryBase
{
    private readonly IServiceProvider _serviceProvider;

    public ExampleRepository(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    protected override IRepositoryContext CreateContext()
    {
        return _serviceProvider.GetRequiredService<IRepositoryContext>();
    }

    protected override string GetCreatedModifiedBy()
    {
        return "";
    }

    protected override IEnumerable<IRepositoryEventProvider<Entity>> GetEventProviders<Entity>()
    {
        return _serviceProvider.GetServices<IRepositoryEventProvider<Entity>>();
    }
}
