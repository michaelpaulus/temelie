using Temelie.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
namespace Temelie.Repository;

public abstract partial class RepositoryBase : IRepository
{
    private readonly IRepositoryEventFactory _repositoryEventFactory;

    public RepositoryBase(IRepositoryEventFactory repositoryEventFactory)
    {
        _repositoryEventFactory = repositoryEventFactory;
    }

    protected abstract IRepositoryContext CreateContext();
    protected abstract string GetCreatedModifiedBy();

    public virtual async Task<Entity?> GetSingleAsync<Entity>(IQuerySpec<Entity> spec) where Entity : EntityBase, IEntity<Entity>
    {
        using var context = CreateContext();
        var query = context.DbContext.Set<Entity>().AsNoTracking();
        query = await OnQueryAsync(context, query, spec.Apply).ConfigureAwait(false);
        return await query.FirstOrDefaultAsync().ConfigureAwait(false);
    }

    public virtual async Task<IEnumerable<Entity>> GetListAsync<Entity>(IQuerySpec<Entity> spec) where Entity : EntityBase, IEntity<Entity>
    {
        using var context = CreateContext();
        var query = context.DbContext.Set<Entity>().AsNoTracking();
        query = await OnQueryAsync(context, query, spec.Apply).ConfigureAwait(false);
        return await query.ToListAsync().ConfigureAwait(false);
    }

    public virtual async Task<IEnumerable<TReturn>> GetListAsync<Entity, TReturn>(IQueryAndTransformSpec<Entity, TReturn> spec) where Entity : EntityBase, IEntity<Entity>
    {
        using var context = CreateContext();
        var query = context.DbContext.Set<Entity>().AsNoTracking();
        query = await OnQueryAsync(context, query, spec.Apply).ConfigureAwait(false);
        var query1 = spec.Transform(context, query);
        return await query1.ToListAsync().ConfigureAwait(false);
    }

    public virtual async Task<int> GetCountAsync<Entity>(IQuerySpec<Entity> spec) where Entity : EntityBase, IEntity<Entity>
    {
        using var context = CreateContext();
        var query = context.DbContext.Set<Entity>().AsNoTracking();
        query = await OnQueryAsync(context, query, spec.Apply).ConfigureAwait(false);
        return await query.CountAsync().ConfigureAwait(false);
    }

    public virtual async Task<int> GetCountAsync<Entity, TReturn>(IQueryAndTransformSpec<Entity, TReturn> spec) where Entity : EntityBase, IEntity<Entity>
    {
        using var context = CreateContext();
        var query = context.DbContext.Set<Entity>().AsNoTracking();
        query = await OnQueryAsync(context, query, spec.Apply).ConfigureAwait(false);
        var query1 = spec.Transform(context, query);
        return await query1.CountAsync().ConfigureAwait(false);
    }

    public async Task<Entity?> GetSingleAsync<Entity>(Expression<Func<Entity, bool>>? filter = null, Func<IQueryable<Entity>, IQueryable<Entity>>? query = null) where Entity : EntityBase, IEntity<Entity>
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

    public async Task<IEnumerable<Entity>> GetListAsync<Entity>(Expression<Func<Entity, bool>>? filter = null, Func<IQueryable<Entity>, IQueryable<Entity>>? query = null) where Entity : EntityBase, IEntity<Entity>
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

    public async Task<int> GetCountAsync<Entity>(Expression<Func<Entity, bool>>? filter = null, Func<IQueryable<Entity>, IQueryable<Entity>>? query = null) where Entity : EntityBase, IEntity<Entity>
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

    public virtual async Task AddAsync<Entity>(Entity entity) where Entity : EntityBase, IEntity<Entity>
    {
        using var context = CreateContext();
        await OnAddingAsync(context, entity).ConfigureAwait(false);
        context.DbContext.Entry(entity).State = EntityState.Added;
        await context.DbContext.SaveChangesAsync().ConfigureAwait(false);
        await OnAddedAsync(context, entity).ConfigureAwait(false);
    }

    public virtual async Task AddRangeAsync<Entity>(IEnumerable<Entity> entities) where Entity : EntityBase, IEntity<Entity>
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

    public virtual async Task DeleteAsync<Entity>(Entity entity) where Entity : EntityBase, IEntity<Entity>
    {
        using var context = CreateContext();
        await OnDeletingAsync(context, entity).ConfigureAwait(false);
        context.DbContext.Entry(entity).State = EntityState.Deleted;
        await context.DbContext.SaveChangesAsync().ConfigureAwait(false);
        await OnDeletedAsync(context, entity).ConfigureAwait(false);
    }

    public virtual async Task DeleteRangeAsync<Entity>(IEnumerable<Entity> entities) where Entity : EntityBase, IEntity<Entity>
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

    public virtual async Task UpdateAsync<Entity>(Entity entity) where Entity : EntityBase, IEntity<Entity>
    {
        using var context = CreateContext();
        await OnUpdatingAsync(context, entity).ConfigureAwait(false);
        context.DbContext.Entry(entity).State = EntityState.Modified;
        await context.DbContext.SaveChangesAsync().ConfigureAwait(false);
        await OnUpdatedAsync(context, entity).ConfigureAwait(false);
    }

    public virtual async Task UpdateRangeAsync<Entity>(IEnumerable<Entity> entities) where Entity : EntityBase, IEntity<Entity>
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
        if (entity is ICreatedByEntity createdByEntity)
        {
            createdByEntity.CreatedDate = DateTime.UtcNow;
            createdByEntity.CreatedBy = GetCreatedModifiedBy();
        }
        if (entity is IModifiedByEntity modifiedByEntity)
        {
            modifiedByEntity.ModifiedDate = DateTime.UtcNow;
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
        if (entity is IModifiedByEntity modifiedByEntity)
        {
            modifiedByEntity.ModifiedDate = DateTime.UtcNow;
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
