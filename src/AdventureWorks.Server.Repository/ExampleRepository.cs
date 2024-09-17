using Microsoft.Extensions.DependencyInjection;
using Temelie.DependencyInjection;
using Temelie.Repository;

namespace AdventureWorks.Server.Repository;
[ExportTransient(typeof(IRepository), Type = typeof(ExampleRepository))]
public class ExampleRepository : RepositoryBase
{
    private readonly IServiceProvider _serviceProvider;

    public ExampleRepository(IServiceProvider serviceProvider, IIdentityResolver identityResolver) : base(identityResolver)
    {
        _serviceProvider = serviceProvider;
    }

    protected override IRepositoryContext CreateContext()
    {
        return _serviceProvider.GetRequiredService<IRepositoryContext>();
    }
}
