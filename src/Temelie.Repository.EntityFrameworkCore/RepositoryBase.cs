using Temelie.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
namespace Temelie.Repository.EntityFrameworkCore;

public abstract partial class RepositoryBase<Entity> : IRepository<Entity> where Entity : EntityBase, IEntity<Entity>
{
    private readonly IServiceProvider _serviceProvider;

    public RepositoryBase(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public virtual async Task<Entity?> GetSingleAsync(IQuerySpec<Entity> spec)
    {
        using var context = _serviceProvider.GetRequiredService<IRepositoryContext<Entity>>();
        var query = context.DbSet.AsNoTracking();
        query = await OnQueryAsync(query, spec).ConfigureAwait(false);
        return await query.FirstOrDefaultAsync().ConfigureAwait(false);
    }

    public virtual async Task<IEnumerable<Entity>> GetListAsync(IQuerySpec<Entity> spec)
    {
        using var context = _serviceProvider.GetRequiredService<IRepositoryContext<Entity>>();
        var query = context.DbSet.AsNoTracking();
        query = await OnQueryAsync(query, spec).ConfigureAwait(false);
        return await query.ToListAsync().ConfigureAwait(false);
    }

    public virtual async Task<int> GetCountAsync(IQuerySpec<Entity> spec)
    {
        using var context = _serviceProvider.GetRequiredService<IRepositoryContext<Entity>>();
        var query = context.DbSet.AsNoTracking();
        query = await OnQueryAsync(query, spec).ConfigureAwait(false);
        return await query.CountAsync().ConfigureAwait(false);
    }

    public virtual async Task AddAsync(Entity entity)
    {
        using var context = _serviceProvider.GetRequiredService<IRepositoryContext<Entity>>();
        await OnAddingAsync(entity).ConfigureAwait(false);
        context.DbContext.Entry(entity).State = EntityState.Added;
        await context.DbContext.SaveChangesAsync().ConfigureAwait(false);
        await OnAddedAsync(entity).ConfigureAwait(false);
    }

    public virtual async Task AddRangeAsync(IEnumerable<Entity> entities)
    {
        using var context = _serviceProvider.GetRequiredService<IRepositoryContext<Entity>>();
        foreach (var entity in entities)
        {
            await OnAddingAsync(entity).ConfigureAwait(false);
            context.DbContext.Entry(entity).State = EntityState.Added;
        }
        await context.DbContext.SaveChangesAsync().ConfigureAwait(false);
        foreach (var entity in entities)
        {
            await OnAddedAsync(entity).ConfigureAwait(false);
        }
    }

    public virtual async Task DeleteAsync(Entity entity)
    {
        using var context = _serviceProvider.GetRequiredService<IRepositoryContext<Entity>>();
        await OnDeletingAsync(entity).ConfigureAwait(false);
        context.DbContext.Entry(entity).State = EntityState.Deleted;
        await context.DbContext.SaveChangesAsync().ConfigureAwait(false);
        await OnDeletedAsync(entity).ConfigureAwait(false);
    }

    public virtual async Task DeleteRangeAsync(IEnumerable<Entity> entities)
    {
        using var context = _serviceProvider.GetRequiredService<IRepositoryContext<Entity>>();
        foreach (var entity in entities)
        {
            await OnDeletingAsync(entity).ConfigureAwait(false);
            context.DbContext.Entry(entity).State = EntityState.Deleted;
        }
        await context.DbContext.SaveChangesAsync().ConfigureAwait(false);
        foreach (var entity in entities)
        {
            await OnDeletedAsync(entity).ConfigureAwait(false);
        }
    }

    public virtual async Task UpdateAsync(Entity entity)
    {
        using var context = _serviceProvider.GetRequiredService<IRepositoryContext<Entity>>();
        await OnUpdatingAsync(entity).ConfigureAwait(false);
        context.DbContext.Entry(entity).State = EntityState.Modified;
        await context.DbContext.SaveChangesAsync().ConfigureAwait(false);
        await OnUpdatedAsync(entity).ConfigureAwait(false);
    }

    public virtual async Task UpdateRangeAsync(IEnumerable<Entity> entities)
    {
        using var context = _serviceProvider.GetRequiredService<IRepositoryContext<Entity>>();
        foreach (var entity in entities)
        {
            await OnUpdatingAsync(entity).ConfigureAwait(false);
            context.DbContext.Entry(entity).State = EntityState.Modified;
        }
        await context.DbContext.SaveChangesAsync().ConfigureAwait(false);
        foreach (var entity in entities)
        {
            await OnUpdatedAsync(entity).ConfigureAwait(false);
        }
    }

    protected virtual Task<IQueryable<Entity>> OnQueryAsync(IQueryable<Entity> query, IQuerySpec<Entity> spec)
    {
        query = spec.Apply(query);
        return Task.FromResult(query);
    }

    protected virtual Task OnAddingAsync(Entity entity)
    {
        var identityResolver = _serviceProvider.GetService<IIdentityResolver>();
        if (entity is ICreatedByEntity createdByEntity)
        {
            createdByEntity.CreatedDate = DateTime.UtcNow;
            createdByEntity.CreatedBy = identityResolver?.GetIdentity();
        }
        if (entity is IModifiedByEntity modifiedByEntity)
        {
            modifiedByEntity.ModifiedDate = DateTime.UtcNow;
            modifiedByEntity.ModifiedBy = identityResolver?.GetIdentity();
        }
        return Task.CompletedTask;
    }

    protected virtual Task OnAddedAsync(Entity entity)
    {
        return Task.CompletedTask;
    }

    protected virtual Task OnDeletingAsync(Entity entity)
    {
        return Task.CompletedTask;
    }

    protected virtual Task OnDeletedAsync(Entity entity)
    {
        return Task.CompletedTask;
    }

    protected virtual Task OnUpdatingAsync(Entity entity)
    {
        var identityResolver = _serviceProvider.GetService<IIdentityResolver>();
        if (entity is IModifiedByEntity modifiedByEntity)
        {
            modifiedByEntity.ModifiedDate = DateTime.UtcNow;
            modifiedByEntity.ModifiedBy = identityResolver?.GetIdentity();
        }
        return Task.CompletedTask;
    }

    protected virtual Task OnUpdatedAsync(Entity entity)
    {
        return Task.CompletedTask;
    }

    public virtual void Dispose()
    {
    }
}
