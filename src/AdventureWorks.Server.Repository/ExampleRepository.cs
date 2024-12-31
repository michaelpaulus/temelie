using System.Linq.Expressions;
using AdventureWorks.Entities;
using Microsoft.EntityFrameworkCore;
using Temelie.DependencyInjection;
using Temelie.Entities;
using Temelie.Repository;

namespace AdventureWorks.Server.Repository;
[ExportTransient(typeof(IExampleRepository), Type = typeof(ExampleRepository))]
public class ExampleRepository : RepositoryBase, IExampleRepository
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

    public Task AddAsync<Entity>(Entity entity) where Entity : EntityBase, IEntity<Entity>, IProjectEntity
    {
        return AddInternalAsync(entity);
    }

    public Task AddRangeAsync<Entity>(IEnumerable<Entity> entities) where Entity : EntityBase, IEntity<Entity>, IProjectEntity
    {
        return AddRangeInternalAsync(entities);
    }

    public Task DeleteAsync<Entity>(Entity entity) where Entity : EntityBase, IEntity<Entity>, IProjectEntity
    {
        return DeleteInternalAsync(entity);
    }

    public Task DeleteRangeAsync<Entity>(IEnumerable<Entity> entities) where Entity : EntityBase, IEntity<Entity>, IProjectEntity
    {
        return DeleteRangeInternalAsync(entities);
    }

    public Task<int> GetCountAsync<Entity>(IQuerySpec<Entity> spec) where Entity : EntityBase, IEntity<Entity>, IProjectEntity
    {
        return GetCountInternalAsync(spec);
    }

    public Task<int> GetCountAsync<Entity, TReturn>(IQueryAndTransformSpec<Entity, TReturn> spec) where Entity : EntityBase, IEntity<Entity>, IProjectEntity
    {
        return GetCountInternalAsync(spec);
    }

    public Task<int> GetCountAsync<Entity>(Expression<Func<Entity, bool>>? filter = null, Func<IQueryable<Entity>, IQueryable<Entity>>? query = null) where Entity : EntityBase, IEntity<Entity>, IProjectEntity
    {
        return GetCountInternalAsync(filter, query);
    }

    public Task<IEnumerable<Entity>> GetListAsync<Entity>(IQuerySpec<Entity> spec) where Entity : EntityBase, IEntity<Entity>, IProjectEntity
    {
        return GetListInternalAsync(spec);
    }

    public Task<IEnumerable<TReturn>> GetListAsync<Entity, TReturn>(IQueryAndTransformSpec<Entity, TReturn> spec) where Entity : EntityBase, IEntity<Entity>, IProjectEntity
    {
        return GetListInternalAsync(spec);
    }

    public Task<IEnumerable<Entity>> GetListAsync<Entity>(Expression<Func<Entity, bool>>? filter = null, Func<IQueryable<Entity>, IQueryable<Entity>>? query = null) where Entity : EntityBase, IEntity<Entity>, IProjectEntity
    {
        return GetListInternalAsync(filter, query);
    }

    public Task<Entity?> GetSingleAsync<Entity>(IQuerySpec<Entity> spec) where Entity : EntityBase, IEntity<Entity>, IProjectEntity
    {
        return GetSingleInternalAsync(spec);
    }

    public Task<Entity?> GetSingleAsync<Entity>(Expression<Func<Entity, bool>>? filter = null, Func<IQueryable<Entity>, IQueryable<Entity>>? query = null) where Entity : EntityBase, IEntity<Entity>, IProjectEntity
    {
        return GetSingleInternalAsync(filter, query);
    }

    public Task UpdateAsync<Entity>(Entity entity) where Entity : EntityBase, IEntity<Entity>, IProjectEntity
    {
        return UpdateInternalAsync(entity);
    }

    public Task UpdateRangeAsync<Entity>(IEnumerable<Entity> entities) where Entity : EntityBase, IEntity<Entity>, IProjectEntity
    {
        return UpdateRangeInternalAsync(entities);
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
