using Microsoft.EntityFrameworkCore;
using Temelie.DependencyInjection;
using Temelie.Repository;

namespace AdventureWorks.Server.Repository;
[ExportTransient(typeof(IRepository), Type = typeof(ExampleRepository))]
public class ExampleRepository : RepositoryBase
{
    private readonly IDbContextFactory<ExampleDbContext> _dbContextFactory;
    private readonly IRepositoryEventFactory _repositoryEventFactory;

    public ExampleRepository(
        IDbContextFactory<ExampleDbContext> dbContextFactory,
        IRepositoryEventFactory repositoryEventFactory) : base(repositoryEventFactory)
    {
        _dbContextFactory = dbContextFactory;
        _repositoryEventFactory = repositoryEventFactory;
    }

    protected override IRepositoryContext CreateContext()
    {
        return _dbContextFactory.CreateDbContext();
    }

    protected override string GetCreatedModifiedBy()
    {
        return "";
    }

}
