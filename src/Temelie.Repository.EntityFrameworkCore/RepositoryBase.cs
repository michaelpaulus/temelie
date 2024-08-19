using Temelie.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
namespace Temelie.Repository.EntityFrameworkCore;

public abstract partial class RepositoryBase : IRepository
{
    private readonly IServiceProvider _serviceProvider;

    public RepositoryBase(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public virtual async Task<Entity?> GetSingleAsync<Entity>(IQuerySpec<Entity> spec) where Entity : EntityBase, IEntity<Entity>
    {
        using var context = _serviceProvider.GetRequiredService<IRepositoryContext<Entity>>();
        var query = context.DbSet.AsNoTracking();
        query = await OnQueryAsync(query, spec.Apply).ConfigureAwait(false);
        return await query.FirstOrDefaultAsync().ConfigureAwait(false);
    }

    public virtual async Task<IEnumerable<Entity>> GetListAsync<Entity>(IQuerySpec<Entity> spec) where Entity : EntityBase, IEntity<Entity>
    {
        using var context = _serviceProvider.GetRequiredService<IRepositoryContext<Entity>>();
        var query = context.DbSet.AsNoTracking();
        query = await OnQueryAsync(query, spec.Apply).ConfigureAwait(false);
        return await query.ToListAsync().ConfigureAwait(false);
    }

    public virtual async Task<int> GetCountAsync<Entity>(IQuerySpec<Entity> spec) where Entity : EntityBase, IEntity<Entity>
    {
        using var context = _serviceProvider.GetRequiredService<IRepositoryContext<Entity>>();
        var query = context.DbSet.AsNoTracking();
        query = await OnQueryAsync(query, spec.Apply).ConfigureAwait(false);
        return await query.CountAsync().ConfigureAwait(false);
    }

    public async Task<Entity?> GetSingleAsync<Entity>(Func<IQueryable<Entity>, IQueryable<Entity>> apply) where Entity : EntityBase, IEntity<Entity>
    {
        using var context = _serviceProvider.GetRequiredService<IRepositoryContext<Entity>>();
        var query = context.DbSet.AsNoTracking();
        query = await OnQueryAsync(query, apply).ConfigureAwait(false);
        return await query.FirstOrDefaultAsync().ConfigureAwait(false);
    }

    public async Task<IEnumerable<Entity>> GetListAsync<Entity>(Func<IQueryable<Entity>, IQueryable<Entity>> apply) where Entity : EntityBase, IEntity<Entity>
    {
        using var context = _serviceProvider.GetRequiredService<IRepositoryContext<Entity>>();
        var query = context.DbSet.AsNoTracking();
        query = await OnQueryAsync(query, apply).ConfigureAwait(false);
        return await query.ToListAsync().ConfigureAwait(false);
    }

    public async Task<int> GetCountAsync<Entity>(Func<IQueryable<Entity>, IQueryable<Entity>> apply) where Entity : EntityBase, IEntity<Entity>
    {
        using var context = _serviceProvider.GetRequiredService<IRepositoryContext<Entity>>();
        var query = context.DbSet.AsNoTracking();
        query = await OnQueryAsync(query, apply).ConfigureAwait(false);
        return await query.CountAsync().ConfigureAwait(false);
    }

    public virtual async Task AddAsync<Entity>(Entity entity) where Entity : EntityBase, IEntity<Entity>
    {
        using var context = _serviceProvider.GetRequiredService<IRepositoryContext<Entity>>();
        await OnAddingAsync(entity).ConfigureAwait(false);
        context.DbContext.Entry(entity).State = EntityState.Added;
        await context.DbContext.SaveChangesAsync().ConfigureAwait(false);
        await OnAddedAsync(entity).ConfigureAwait(false);
    }

    public virtual async Task AddRangeAsync<Entity>(IEnumerable<Entity> entities) where Entity : EntityBase, IEntity<Entity>
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

    public virtual async Task DeleteAsync<Entity>(Entity entity) where Entity : EntityBase, IEntity<Entity>
    {
        using var context = _serviceProvider.GetRequiredService<IRepositoryContext<Entity>>();
        await OnDeletingAsync(entity).ConfigureAwait(false);
        context.DbContext.Entry(entity).State = EntityState.Deleted;
        await context.DbContext.SaveChangesAsync().ConfigureAwait(false);
        await OnDeletedAsync(entity).ConfigureAwait(false);
    }

    public virtual async Task DeleteRangeAsync<Entity>(IEnumerable<Entity> entities) where Entity : EntityBase, IEntity<Entity>
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

    public virtual async Task UpdateAsync<Entity>(Entity entity) where Entity : EntityBase, IEntity<Entity>
    {
        using var context = _serviceProvider.GetRequiredService<IRepositoryContext<Entity>>();
        await OnUpdatingAsync(entity).ConfigureAwait(false);
        context.DbContext.Entry(entity).State = EntityState.Modified;
        await context.DbContext.SaveChangesAsync().ConfigureAwait(false);
        await OnUpdatedAsync(entity).ConfigureAwait(false);
    }

    public virtual async Task UpdateRangeAsync<Entity>(IEnumerable<Entity> entities) where Entity : EntityBase, IEntity<Entity>
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

    protected virtual Task<IQueryable<Entity>> OnQueryAsync<Entity>(IQueryable<Entity> query, Func<IQueryable<Entity>, IQueryable<Entity>> apply) where Entity : EntityBase, IEntity<Entity>
    {
        return Task.FromResult(apply.Invoke(query));
    }

    protected virtual Task OnAddingAsync<Entity>(Entity entity) where Entity : EntityBase, IEntity<Entity>
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

    protected virtual Task OnAddedAsync<Entity>(Entity entity) where Entity : EntityBase, IEntity<Entity>
    {
        return Task.CompletedTask;
    }

    protected virtual Task OnDeletingAsync<Entity>(Entity entity) where Entity : EntityBase, IEntity<Entity>
    {
        return Task.CompletedTask;
    }

    protected virtual Task OnDeletedAsync<Entity>(Entity entity) where Entity : EntityBase, IEntity<Entity>
    {
        return Task.CompletedTask;
    }

    protected virtual Task OnUpdatingAsync<Entity>(Entity entity) where Entity : EntityBase, IEntity<Entity>
    {
        var identityResolver = _serviceProvider.GetService<IIdentityResolver>();
        if (entity is IModifiedByEntity modifiedByEntity)
        {
            modifiedByEntity.ModifiedDate = DateTime.UtcNow;
            modifiedByEntity.ModifiedBy = identityResolver?.GetIdentity();
        }
        return Task.CompletedTask;
    }

    protected virtual Task OnUpdatedAsync<Entity>(Entity entity) where Entity : EntityBase, IEntity<Entity>
    {
        return Task.CompletedTask;
    }

    public virtual void Dispose()
    {
    }

}
