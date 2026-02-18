
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;
using Temelie.Entities;

namespace Temelie.Repository;

public abstract class DefaultRepository : RepositoryBase, IDefaultRepository
{
    protected DefaultRepository(IRepositoryEventFactory repositoryEventFactory) : base(repositoryEventFactory)
    {
    }

    public Task AddAsync<Entity>(Entity entity) where Entity : EntityBase, IEntity<Entity>
    {
        return AddInternalAsync(entity);
    }

    public Task AddRangeAsync<Entity>(IEnumerable<Entity> entities) where Entity : EntityBase, IEntity<Entity>
    {
        return AddRangeInternalAsync(entities);
    }

    public Task DeleteAsync<Entity>(Entity entity) where Entity : EntityBase, IEntity<Entity>
    {
        return DeleteInternalAsync(entity);
    }

    public Task DeleteFromQueryAsync<Entity>(IQuerySpec<Entity> spec) where Entity : EntityBase, IEntity<Entity>
    {
        return DeleteFromQueryInternalAsync(spec);
    }

    public Task DeleteFromQueryAsync<Entity>(Expression<Func<Entity, bool>>? filter = null, Func<IQueryable<Entity>, IQueryable<Entity>>? query = null) where Entity : EntityBase, IEntity<Entity>
    {
        return DeleteFromQueryInternalAsync(filter, query);
    }

    public Task DeleteRangeAsync<Entity>(IEnumerable<Entity> entities) where Entity : EntityBase, IEntity<Entity>
    {
        return DeleteRangeInternalAsync(entities);
    }

    public Task<int> GetCountAsync<Entity>(IQuerySpec<Entity> spec) where Entity : EntityBase, IEntity<Entity>
    {
        return GetCountInternalAsync(spec);
    }

    public Task<int> GetCountAsync<Entity, TReturn>(IQueryAndTransformSpec<Entity, TReturn> spec) where Entity : EntityBase, IEntity<Entity>
    {
        return GetCountInternalAsync(spec);
    }

    public Task<int> GetCountAsync<Entity>(Expression<Func<Entity, bool>>? filter = null, Func<IQueryable<Entity>, IQueryable<Entity>>? query = null) where Entity : EntityBase, IEntity<Entity>
    {
        return GetCountInternalAsync(filter, query);
    }

    public Task<IEnumerable<Entity>> GetListAsync<Entity>(IQuerySpec<Entity> spec) where Entity : EntityBase, IEntity<Entity>
    {
        return GetListInternalAsync(spec);
    }

    public Task<IEnumerable<TReturn>> GetListAsync<Entity, TReturn>(IQueryAndTransformSpec<Entity, TReturn> spec) where Entity : EntityBase, IEntity<Entity>
    {
        return GetListInternalAsync(spec);
    }

    public Task<IEnumerable<Entity>> GetListAsync<Entity>(Expression<Func<Entity, bool>>? filter = null, Func<IQueryable<Entity>, IQueryable<Entity>>? query = null) where Entity : EntityBase, IEntity<Entity>
    {
        return GetListInternalAsync(filter, query);
    }

    public Task<Entity?> GetSingleAsync<Entity>(IQuerySpec<Entity> spec) where Entity : EntityBase, IEntity<Entity>
    {
        return GetSingleInternalAsync(spec);
    }

    public Task<Entity?> GetSingleAsync<Entity>(Expression<Func<Entity, bool>>? filter = null, Func<IQueryable<Entity>, IQueryable<Entity>>? query = null) where Entity : EntityBase, IEntity<Entity>
    {
        return GetSingleInternalAsync(filter, query);
    }

    public Task UpdateAsync<Entity>(Entity entity) where Entity : EntityBase, IEntity<Entity>
    {
        return UpdateInternalAsync(entity);
    }

    public Task UpdateFromQueryAsync<Entity>(IQuerySpec<Entity> spec, Expression<Func<SetPropertyCalls<Entity>, SetPropertyCalls<Entity>>> setPropertyCalls) where Entity : EntityBase, IEntity<Entity>
    {
        return UpdateFromQueryInternalAsync(spec, setPropertyCalls);
    }

    public Task UpdateFromQueryAsync<Entity>(Expression<Func<SetPropertyCalls<Entity>, SetPropertyCalls<Entity>>> setPropertyCalls, Expression<Func<Entity, bool>>? filter = null, Func<IQueryable<Entity>, IQueryable<Entity>>? query = null) where Entity : EntityBase, IEntity<Entity>
    {
        return UpdateFromQueryInternalAsync(setPropertyCalls, filter, query);
    }

    public Task UpdateRangeAsync<Entity>(IEnumerable<Entity> entities) where Entity : EntityBase, IEntity<Entity>
    {
        return UpdateRangeInternalAsync(entities);
    }

}
