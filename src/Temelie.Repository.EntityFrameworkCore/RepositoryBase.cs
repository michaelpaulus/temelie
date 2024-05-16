using Temelie.Entities;
using Microsoft.EntityFrameworkCore;
namespace Temelie.Repository.EntityFrameworkCore;

public abstract partial class RepositoryBase<Entity> : IRepository<Entity> where Entity : EntityBase, IEntity<Entity>
{
    private readonly IRepositoryContext<Entity> _context;
    private readonly IIdentityResolver _identityResolver;

    public RepositoryBase(IRepositoryContext<Entity> context, IIdentityResolver identityResolver)
    {
        _context = context;
        _identityResolver = identityResolver;
    }

    protected IRepositoryContext<Entity> Context => _context;

    public virtual async Task<Entity?> GetSingleAsync(IQuerySpec<Entity> spec)
    {
        var query = await OnQueryAsync(spec).ConfigureAwait(false);
        return await query.FirstOrDefaultAsync().ConfigureAwait(false);
    }

    public virtual async Task<IEnumerable<Entity>> GetListAsync(IQuerySpec<Entity> spec)
    {
        var query = await OnQueryAsync(spec).ConfigureAwait(false);
        return await query.ToListAsync().ConfigureAwait(false);
    }

    public virtual async Task<int> GetCountAsync(IQuerySpec<Entity> spec)
    {
        var query = await OnQueryAsync(spec).ConfigureAwait(false);
        return await query.CountAsync().ConfigureAwait(false);
    }

    public virtual async Task AddAsync(Entity entity)
    {
        await OnAddingAsync(entity).ConfigureAwait(false);   
        await _context.DbSet.AddAsync(entity).ConfigureAwait(false);
        await _context.DbContext.SaveChangesAsync().ConfigureAwait(false);
        await OnAddedAsync(entity).ConfigureAwait(false);
    }

    public virtual async Task AddRangeAsync(IEnumerable<Entity> entities)
    {
        foreach (var entity in entities)
        {
            await OnAddingAsync(entity).ConfigureAwait(false);
        }
        await _context.DbSet.AddRangeAsync(entities).ConfigureAwait(false);
        await _context.DbContext.SaveChangesAsync().ConfigureAwait(false);
        foreach (var entity in entities)
        {
            await OnAddedAsync(entity).ConfigureAwait(false);
        }
    }

    public virtual async Task DeleteAsync(Entity entity)
    {
        await OnDeletingAsync(entity).ConfigureAwait(false);
        _context.DbSet.Remove(entity);
        await _context.DbContext.SaveChangesAsync().ConfigureAwait(false);
        await OnDeletedAsync(entity).ConfigureAwait(false);
    }

    public virtual async Task DeleteRangeAsync(IEnumerable<Entity> entities)
    {
        foreach (var entity in entities)
        {
            await OnDeletingAsync(entity).ConfigureAwait(false);
        }
        _context.DbSet.RemoveRange(entities);
        await _context.DbContext.SaveChangesAsync().ConfigureAwait(false);
        foreach (var entity in entities)
        {
            await OnDeletedAsync(entity).ConfigureAwait(false);
        }
    }

    public virtual async Task UpdateAsync(Entity entity)
    {
        await OnUpdatingAsync(entity).ConfigureAwait(false);
        _context.DbSet.Update(entity);
        await _context.DbContext.SaveChangesAsync().ConfigureAwait(false);
        await OnUpdatedAsync(entity).ConfigureAwait(false);
    }

    public virtual async Task UpdateRangeAsync(IEnumerable<Entity> entities)
    {
        foreach (var entity in entities)
        {
            await OnUpdatingAsync(entity).ConfigureAwait(false);
        }
        _context.DbSet.UpdateRange(entities);
        await _context.DbContext.SaveChangesAsync().ConfigureAwait(false);
        foreach (var entity in entities)
        {
            await OnUpdatedAsync(entity).ConfigureAwait(false);
        }
    }

    protected virtual Task<IQueryable<Entity>> OnQueryAsync(IQuerySpec<Entity> spec)
    {
        var query = _context.DbSet.AsNoTracking();
        query = spec.Apply(query);
        return Task.FromResult(query);
    }

    protected virtual Task OnAddingAsync(Entity entity)
    {
        if (entity is ICreatedByEntity createdByEntity)
        {
            createdByEntity.CreatedDate = DateTime.UtcNow;
            createdByEntity.CreatedBy = _identityResolver?.GetIdentity();
        }
        if (entity is IModifiedByEntity modifiedByEntity)
        {
            modifiedByEntity.ModifiedDate = DateTime.UtcNow;
            modifiedByEntity.ModifiedBy = _identityResolver?.GetIdentity();
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
        if (entity is IModifiedByEntity modifiedByEntity)
        {
            modifiedByEntity.ModifiedDate = DateTime.UtcNow;
            modifiedByEntity.ModifiedBy = _identityResolver?.GetIdentity();
        }
        return Task.CompletedTask;
    }

    protected virtual Task OnUpdatedAsync(Entity entity)
    {
        return Task.CompletedTask;
    }

    public virtual void Dispose()
    {
        _context.DbContext.Dispose();
    }
}
