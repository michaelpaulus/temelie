using Temelie.DependencyInjection;
using Temelie.Repository;
using Temelie.Repository.EntityFrameworkCore;

namespace AdventureWorks.Server.Repository.EntityFrameworkCore;
[ExportTransient(typeof(IRepository), Type = typeof(ExampleRepository))]
public class ExampleRepository : RepositoryBase
{
    public ExampleRepository(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

}
