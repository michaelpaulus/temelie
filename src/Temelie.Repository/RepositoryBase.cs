using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Temelie.Entities;

namespace Temelie.Repository;

public abstract partial class RepositoryBase
{
    private readonly IRepositoryEventFactory _repositoryEventFactory;

    public RepositoryBase(IRepositoryEventFactory repositoryEventFactory)
    {
        _repositoryEventFactory = repositoryEventFactory;
    }

    protected abstract IRepositoryContext CreateContext();
    protected abstract string GetCreatedModifiedBy();

    protected virtual async Task<Entity?> GetSingleInternalAsync<Entity>(IQuerySpec<Entity> spec) where Entity : EntityBase, IEntity<Entity>
    {
        using var context = CreateContext();
        var query = context.DbContext.Set<Entity>().AsNoTracking();
        query = await OnQueryAsync(context, query, spec.Apply).ConfigureAwait(false);
        return await query.FirstOrDefaultAsync().ConfigureAwait(false);
    }

    protected virtual async Task<IEnumerable<Entity>> GetListInternalAsync<Entity>(IQuerySpec<Entity> spec) where Entity : EntityBase, IEntity<Entity>
    {
        using var context = CreateContext();
        var query = context.DbContext.Set<Entity>().AsNoTracking();
        query = await OnQueryAsync(context, query, spec.Apply).ConfigureAwait(false);
        return await query.ToListAsync().ConfigureAwait(false);
    }

    protected virtual async Task<IEnumerable<TReturn>> GetListInternalAsync<Entity, TReturn>(IQueryAndTransformSpec<Entity, TReturn> spec) where Entity : EntityBase, IEntity<Entity>
    {
        using var context = CreateContext();
        var query = context.DbContext.Set<Entity>().AsNoTracking();
        query = await OnQueryAsync(context, query, spec.Apply).ConfigureAwait(false);
        var query1 = spec.Transform(context, query);
        return await query1.ToListAsync().ConfigureAwait(false);
    }

    protected virtual async Task<int> GetCountInternalAsync<Entity>(IQuerySpec<Entity> spec) where Entity : EntityBase, IEntity<Entity>
    {
        using var context = CreateContext();
        var query = context.DbContext.Set<Entity>().AsNoTracking();
        query = await OnQueryAsync(context, query, spec.Apply).ConfigureAwait(false);
        return await query.CountAsync().ConfigureAwait(false);
    }

    protected virtual async Task<int> GetCountInternalAsync<Entity, TReturn>(IQueryAndTransformSpec<Entity, TReturn> spec) where Entity : EntityBase, IEntity<Entity>
    {
        using var context = CreateContext();
        var query = context.DbContext.Set<Entity>().AsNoTracking();
        query = await OnQueryAsync(context, query, spec.Apply).ConfigureAwait(false);
        var query1 = spec.Transform(context, query);
        return await query1.CountAsync().ConfigureAwait(false);
    }

    protected async Task<Entity?> GetSingleInternalAsync<Entity>(Expression<Func<Entity, bool>>? filter = null, Func<IQueryable<Entity>, IQueryable<Entity>>? query = null) where Entity : EntityBase, IEntity<Entity>
    {
        using var context = CreateContext();
        var query1 = context.DbContext.Set<Entity>().AsNoTracking();
        query1 = await OnQueryAsync(context, query1,
            (context, i) =>
            {
                if (filter is not null)
                {
                    i = i.Where(filter);
                }
                if (query is not null)
                {
                    i = query.Invoke(i);
                }
                return i;
            }
            ).ConfigureAwait(false);
        return await query1.FirstOrDefaultAsync().ConfigureAwait(false);
    }

    protected async Task<IEnumerable<Entity>> GetListInternalAsync<Entity>(Expression<Func<Entity, bool>>? filter = null, Func<IQueryable<Entity>, IQueryable<Entity>>? query = null) where Entity : EntityBase, IEntity<Entity>
    {
        using var context = CreateContext();
        var query1 = context.DbContext.Set<Entity>().AsNoTracking();
        query1 = await OnQueryAsync(context, query1,
            (context, i) =>
            {
                if (filter is not null)
                {
                    i = i.Where(filter);
                }
                if (query is not null)
                {
                    i = query.Invoke(i);
                }
                return i;
            }
            ).ConfigureAwait(false);
        return await query1.ToListAsync().ConfigureAwait(false);
    }

    protected async Task<int> GetCountInternalAsync<Entity>(Expression<Func<Entity, bool>>? filter = null, Func<IQueryable<Entity>, IQueryable<Entity>>? query = null) where Entity : EntityBase, IEntity<Entity>
    {
        using var context = CreateContext();
        var query1 = context.DbContext.Set<Entity>().AsNoTracking();
        query1 = await OnQueryAsync(context, query1,
            (context, i) =>
            {
                if (filter is not null)
                {
                    i = i.Where(filter);
                }
                if (query is not null)
                {
                    i = query.Invoke(i);
                }
                return i;
            }
            ).ConfigureAwait(false);
        return await query1.CountAsync().ConfigureAwait(false);
    }

    protected virtual async Task AddInternalAsync<Entity>(Entity entity) where Entity : EntityBase, IEntity<Entity>
    {
        using var context = CreateContext();
        await OnAddingAsync(context, entity).ConfigureAwait(false);
        context.DbContext.Entry(entity).State = EntityState.Added;
        await context.DbContext.SaveChangesAsync().ConfigureAwait(false);
        await OnAddedAsync(context, entity).ConfigureAwait(false);
    }

    protected virtual async Task AddRangeInternalAsync<Entity>(IEnumerable<Entity> entities) where Entity : EntityBase, IEntity<Entity>
    {
        using var context = CreateContext();
        foreach (var entity in entities)
        {
            await OnAddingAsync(context, entity).ConfigureAwait(false);
            context.DbContext.Entry(entity).State = EntityState.Added;
        }
        await context.DbContext.SaveChangesAsync().ConfigureAwait(false);
        foreach (var entity in entities)
        {
            await OnAddedAsync(context, entity).ConfigureAwait(false);
        }
    }

    protected virtual async Task DeleteInternalAsync<Entity>(Entity entity) where Entity : EntityBase, IEntity<Entity>
    {
        using var context = CreateContext();
        await OnDeletingAsync(context, entity).ConfigureAwait(false);
        context.DbContext.Entry(entity).State = EntityState.Deleted;
        await context.DbContext.SaveChangesAsync().ConfigureAwait(false);
        await OnDeletedAsync(context, entity).ConfigureAwait(false);
    }

    protected virtual async Task DeleteRangeInternalAsync<Entity>(IEnumerable<Entity> entities) where Entity : EntityBase, IEntity<Entity>
    {
        using var context = CreateContext();
        foreach (var entity in entities)
        {
            await OnDeletingAsync(context, entity).ConfigureAwait(false);
            context.DbContext.Entry(entity).State = EntityState.Deleted;
        }
        await context.DbContext.SaveChangesAsync().ConfigureAwait(false);
        foreach (var entity in entities)
        {
            await OnDeletedAsync(context, entity).ConfigureAwait(false);
        }
    }

    protected virtual async Task DeleteFromQueryInternalAsync<Entity>(IQuerySpec<Entity> spec) where Entity : EntityBase, IEntity<Entity>
    {
        using var context = CreateContext();
        var query = context.DbContext.Set<Entity>().AsNoTracking();
        query = await OnQueryAsync(context, query, spec.Apply).ConfigureAwait(false);
        await query.ExecuteDeleteAsync().ConfigureAwait(false);
    }

    protected async Task DeleteFromQueryInternalAsync<Entity>(Expression<Func<Entity, bool>>? filter = null, Func<IQueryable<Entity>, IQueryable<Entity>>? query = null) where Entity : EntityBase, IEntity<Entity>
    {
        using var context = CreateContext();
        var query1 = context.DbContext.Set<Entity>().AsNoTracking();
        query1 = await OnQueryAsync(context, query1,
            (context, i) =>
            {
                if (filter is not null)
                {
                    i = i.Where(filter);
                }
                if (query is not null)
                {
                    i = query.Invoke(i);
                }
                return i;
            }
            ).ConfigureAwait(false);
        await query1.ExecuteDeleteAsync().ConfigureAwait(false);
    }

    protected virtual async Task UpdateFromQueryInternalAsync<Entity>(IQuerySpec<Entity> spec, Expression<Func<SetPropertyCalls<Entity>, SetPropertyCalls<Entity>>> setPropertyCalls) where Entity : EntityBase, IEntity<Entity>
    {
        using var context = CreateContext();
        var query = context.DbContext.Set<Entity>().AsNoTracking();
        query = await OnQueryAsync(context, query, spec.Apply).ConfigureAwait(false);
        await query.ExecuteUpdateAsync(setPropertyCalls).ConfigureAwait(false);
    }

    protected async Task UpdateFromQueryInternalAsync<Entity>(Expression<Func<SetPropertyCalls<Entity>, SetPropertyCalls<Entity>>> setPropertyCalls, Expression<Func<Entity, bool>>? filter = null, Func<IQueryable<Entity>, IQueryable<Entity>>? query = null) where Entity : EntityBase, IEntity<Entity>
    {
        using var context = CreateContext();
        var query1 = context.DbContext.Set<Entity>().AsNoTracking();
        query1 = await OnQueryAsync(context, query1,
            (context, i) =>
            {
                if (filter is not null)
                {
                    i = i.Where(filter);
                }
                if (query is not null)
                {
                    i = query.Invoke(i);
                }
                return i;
            }
            ).ConfigureAwait(false);
        await query1.ExecuteUpdateAsync(setPropertyCalls).ConfigureAwait(false);
    }

    protected virtual async Task UpdateInternalAsync<Entity>(Entity entity) where Entity : EntityBase, IEntity<Entity>
    {
        using var context = CreateContext();
        await OnUpdatingAsync(context, entity).ConfigureAwait(false);
        context.DbContext.Entry(entity).State = EntityState.Modified;
        await context.DbContext.SaveChangesAsync().ConfigureAwait(false);
        await OnUpdatedAsync(context, entity).ConfigureAwait(false);
    }

    protected virtual async Task UpdateRangeInternalAsync<Entity>(IEnumerable<Entity> entities) where Entity : EntityBase, IEntity<Entity>
    {
        using var context = CreateContext();
        foreach (var entity in entities)
        {
            await OnUpdatingAsync(context, entity).ConfigureAwait(false);
            context.DbContext.Entry(entity).State = EntityState.Modified;
        }
        await context.DbContext.SaveChangesAsync().ConfigureAwait(false);
        foreach (var entity in entities)
        {
            await OnUpdatedAsync(context, entity).ConfigureAwait(false);
        }
    }

    protected virtual async Task<IQueryable<Entity>> OnQueryAsync<Entity>(IRepositoryContext context, IQueryable<Entity> query, Func<IRepositoryContext, IQueryable<Entity>, IQueryable<Entity>> apply) where Entity : EntityBase, IEntity<Entity>
    {
        query = apply.Invoke(context, query);
        foreach (var provider in _repositoryEventFactory.GetEventProviders<Entity>())
        {
            query = await provider.OnQueryAsync(context, query).ConfigureAwait(false);
        }
        return query;
    }

    protected virtual async Task OnAddingAsync<Entity>(IRepositoryContext context, Entity entity) where Entity : EntityBase, IEntity<Entity>
    {
        if (entity is ICreatedDateEntity createdDateEntity)
        {
            createdDateEntity.CreatedDate = DateTime.UtcNow;
        }
        if (entity is ICreatedByEntity createdByEntity)
        {
            createdByEntity.CreatedBy = GetCreatedModifiedBy();
        }
        if (entity is IModifiedDateEntity modifiedDateEntity)
        {
            modifiedDateEntity.ModifiedDate = DateTime.UtcNow;
        }
        if (entity is IModifiedByEntity modifiedByEntity)
        {
            modifiedByEntity.ModifiedBy = GetCreatedModifiedBy();
        }

        foreach (var provider in _repositoryEventFactory.GetEventProviders<Entity>())
        {
            await provider.OnAddingAsync(context, entity).ConfigureAwait(false);
        }
    }

    protected virtual async Task OnAddedAsync<Entity>(IRepositoryContext context, Entity entity) where Entity : EntityBase, IEntity<Entity>
    {
        foreach (var provider in _repositoryEventFactory.GetEventProviders<Entity>())
        {
            await provider.OnAddedAsync(context, entity).ConfigureAwait(false);
        }
    }

    protected virtual async Task OnDeletingAsync<Entity>(IRepositoryContext context, Entity entity) where Entity : EntityBase, IEntity<Entity>
    {
        foreach (var provider in _repositoryEventFactory.GetEventProviders<Entity>())
        {
            await provider.OnDeletingAsync(context, entity).ConfigureAwait(false);
        }
    }

    protected virtual async Task OnDeletedAsync<Entity>(IRepositoryContext context, Entity entity) where Entity : EntityBase, IEntity<Entity>
    {
        foreach (var provider in _repositoryEventFactory.GetEventProviders<Entity>())
        {
            await provider.OnDeletedAsync(context, entity).ConfigureAwait(false);
        }
    }

    protected virtual async Task OnUpdatingAsync<Entity>(IRepositoryContext context, Entity entity) where Entity : EntityBase, IEntity<Entity>
    {
        if (entity is IModifiedDateEntity modifiedDateEntity)
        {
            modifiedDateEntity.ModifiedDate = DateTime.UtcNow;
        }
        if (entity is IModifiedByEntity modifiedByEntity)
        {
            modifiedByEntity.ModifiedBy = GetCreatedModifiedBy();
        }
        foreach (var provider in _repositoryEventFactory.GetEventProviders<Entity>())
        {
            await provider.OnUpdatingAsync(context, entity).ConfigureAwait(false);
        }
    }

    protected virtual async Task OnUpdatedAsync<Entity>(IRepositoryContext context, Entity entity) where Entity : EntityBase, IEntity<Entity>
    {
        foreach (var provider in _repositoryEventFactory.GetEventProviders<Entity>())
        {
            await provider.OnUpdatedAsync(context, entity).ConfigureAwait(false);
        }
    }

}
