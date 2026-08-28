
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

    public Task<int> DeleteFromQueryAsync<Entity>(IQuerySpec<Entity> spec) where Entity : EntityBase, IEntity<Entity>
    {
        return DeleteFromQueryInternalAsync(spec);
    }

    public Task<int> DeleteFromQueryAsync<Entity>(Expression<Func<Entity, bool>>? filter = null, Func<IQueryable<Entity>, IQueryable<Entity>>? query = null) where Entity : EntityBase, IEntity<Entity>
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

    public Task<bool> GetAnyAsync<Entity>(IQuerySpec<Entity> spec) where Entity : EntityBase, IEntity<Entity>
    {
        return GetAnyInternalAsync(spec);
    }

    public Task<bool> GetAnyAsync<Entity, TReturn>(IQueryAndTransformSpec<Entity, TReturn> spec) where Entity : EntityBase, IEntity<Entity>
    {
        return GetAnyInternalAsync(spec);
    }

    public Task<bool> GetAnyAsync<Entity>(Expression<Func<Entity, bool>>? filter = null, Func<IQueryable<Entity>, IQueryable<Entity>>? query = null) where Entity : EntityBase, IEntity<Entity>
    {
        return GetAnyInternalAsync(filter, query);
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

    public Task UpdateFromQueryAsync<Entity>(IQuerySpec<Entity> spec, Action<UpdateSettersBuilder<Entity>> setPropertyCalls) where Entity : EntityBase, IEntity<Entity>
    {
        return UpdateFromQueryInternalAsync(spec, setPropertyCalls);
    }

    public Task UpdateFromQueryAsync<Entity>(Action<UpdateSettersBuilder<Entity>> setPropertyCalls, Expression<Func<Entity, bool>>? filter = null, Func<IQueryable<Entity>, IQueryable<Entity>>? query = null) where Entity : EntityBase, IEntity<Entity>
    {
        return UpdateFromQueryInternalAsync(setPropertyCalls, filter, query);
    }

    public Task UpdateRangeAsync<Entity>(IEnumerable<Entity> entities) where Entity : EntityBase, IEntity<Entity>
    {
        return UpdateRangeInternalAsync(entities);
    }

    public Task<int> InsertFromQueryAsync<TSource, TTarget>(Expression<Func<TSource, bool>>? filter, Expression<Func<TSource, TTarget>> selector)
        where TSource : EntityBase, IEntity<TSource>
        where TTarget : EntityBase, IEntity<TTarget>
    {
        return InsertFromQueryInternalAsync(filter, selector);
    }

    public Task<int> InsertFromQueryAsync<TSource, TTarget>(IQuerySpec<TSource> spec, Expression<Func<TSource, TTarget>> selector)
        where TSource : EntityBase, IEntity<TSource>
        where TTarget : EntityBase, IEntity<TTarget>
    {
        return InsertFromQueryInternalAsync(spec, selector);
    }

    public Task<MergeResult> MergeFromQueryAsync<TSource, TTarget>(
        Expression<Func<TSource, TTarget, bool>> match,
        Expression<Func<TSource, TTarget>> insertSelector,
        Action<UpdateSettersBuilder<TTarget>>? updateSetters = null,
        bool deleteMissing = false)
        where TSource : EntityBase, IEntity<TSource>
        where TTarget : EntityBase, IEntity<TTarget>
    {
        return MergeFromQueryInternalAsync(match, insertSelector, updateSetters, deleteMissing);
    }

    public Task<MergeResult> MergeFromQueryAsync<TSource, TTarget, TKey>(
        Expression<Func<TSource, TKey>> sourceKey,
        Expression<Func<TTarget, TKey>> targetKey,
        Expression<Func<TSource, TTarget>> insertSelector,
        Action<UpdateSettersBuilder<TTarget>>? updateSetters = null,
        bool deleteMissing = false)
        where TSource : EntityBase, IEntity<TSource>
        where TTarget : EntityBase, IEntity<TTarget>
    {
        return MergeFromQueryInternalAsync(sourceKey, targetKey, insertSelector, updateSetters, deleteMissing);
    }
}
