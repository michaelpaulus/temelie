using Temelie.DependencyInjection;
using Temelie.Entities;
using Temelie.Repository;
using Temelie.Repository.EntityFrameworkCore;

namespace AdventureWorks.Server.Repository.EntityFrameworkCore;
[ExportTransient(typeof(IRepository<>), Type = typeof(ExampleRepository<>))]
public class ExampleRepository<Entity> : RepositoryBase<Entity> where Entity : EntityBase, IEntity<Entity>
{
    public ExampleRepository(IServiceProvider serviceProvider) : base(serviceProvider)
    {
    }

}
