using Temelie.DependencyInjection;
using Temelie.Repository;

namespace AdventureWorks.Server.Repository;
[ExportTransient(typeof(IRepository), Type = typeof(ExampleRepository))]
public class ExampleRepository : RepositoryBase
{
    public ExampleRepository(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

}
