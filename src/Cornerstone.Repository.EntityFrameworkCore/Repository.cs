using Microsoft.EntityFrameworkCore;

namespace Cornerstone.Repository.EntityFrameworkCore;

public class Repository<Entity> : IRepository<Entity> where Entity : class, IEntity<Entity>
{
    private readonly IRepositoryContext<Entity> _context;

    public Repository(IRepositoryContext<Entity> context)
    {
        _context = context;
    }

    public async Task<Entity?> GetSingleAsync(IQuerySpec<Entity> spec)
    {
        var query = _context.DbSet.AsNoTracking();
        query = spec.Apply(query);
        return await query.FirstOrDefaultAsync().ConfigureAwait(false);
    }

    public async Task<IEnumerable<Entity>> GetListAsync(IQuerySpec<Entity> spec)
    {
        var query = _context.DbSet.AsNoTracking();
        query = spec.Apply(query);
        return await query.ToListAsync().ConfigureAwait(false);
    }

    public async Task<int> GetCountAsync(IQuerySpec<Entity> spec)
    {
        var query = _context.DbSet.AsNoTracking();
        query = spec.Apply(query);
        return await query.CountAsync().ConfigureAwait(false);
    }

    public virtual async Task AddAsync(Entity entity)
    {
        await _context.DbSet.AddAsync(entity).ConfigureAwait(false);
        await _context.DbContext.SaveChangesAsync().ConfigureAwait(false);
    }

    public virtual async Task AddRangeAsync(IEnumerable<Entity> entities)
    {
        await _context.DbSet.AddRangeAsync(entities).ConfigureAwait(false);
        await _context.DbContext.SaveChangesAsync().ConfigureAwait(false);
    }

    public virtual async Task DeleteAsync(Entity entity)
    {
        _context.DbSet.Remove(entity);
        await _context.DbContext.SaveChangesAsync().ConfigureAwait(false);
    }

    public virtual async Task DeleteRangeAsync(IEnumerable<Entity> entities)
    {
        _context.DbSet.RemoveRange(entities);
        await _context.DbContext.SaveChangesAsync().ConfigureAwait(false);
    }

    public virtual async Task UpdateAsync(Entity entity)
    {
        _context.DbSet.Update(entity);
        await _context.DbContext.SaveChangesAsync().ConfigureAwait(false);
    }

    public virtual async Task UpdateRangeAsync(IEnumerable<Entity> entities)
    {
        _context.DbSet.UpdateRange(entities);
        await _context.DbContext.SaveChangesAsync().ConfigureAwait(false);
    }

    public void Dispose()
    {
        _context.DbContext.Dispose();
    }
}
