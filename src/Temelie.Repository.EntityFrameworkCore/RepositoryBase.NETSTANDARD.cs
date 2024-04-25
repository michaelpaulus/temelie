namespace Temelie.Repository.EntityFrameworkCore;

#if NETSTANDARD
public partial class RepositoryBase<Entity>
{

    public virtual Entity? GetSingle(IQuerySpec<Entity> spec)
    {
        var query = OnQueryAsync(spec).GetAwaiter().GetResult();
        return query.FirstOrDefault();
    }

    public virtual IEnumerable<Entity> GetList(IQuerySpec<Entity> spec)
    {
        var query = OnQueryAsync(spec).GetAwaiter().GetResult();
        return query.ToList();
    }

    public virtual int GetCount(IQuerySpec<Entity> spec)
    {
        var query = OnQueryAsync(spec).GetAwaiter().GetResult();
        return query.Count();
    }

    public virtual void Add(Entity entity)
    {
        OnAddingAsync(entity).GetAwaiter().GetResult();
        _context.DbSet.Add(entity);
        _context.DbContext.SaveChanges();
        OnAddedAsync(entity).GetAwaiter().GetResult();
    }

    public virtual void AddRange(IEnumerable<Entity> entities)
    {
        foreach (var entity in entities)
        {
            OnAddingAsync(entity).GetAwaiter().GetResult();
        }
        _context.DbSet.AddRange(entities);
        _context.DbContext.SaveChanges();
        foreach (var entity in entities)
        {
            OnAddedAsync(entity).GetAwaiter().GetResult();
        }
    }

    public virtual void Delete(Entity entity)
    {
        OnDeletingAsync(entity).GetAwaiter().GetResult();
        _context.DbSet.Remove(entity);
        _context.DbContext.SaveChanges();
        OnDeletedAsync(entity).GetAwaiter().GetResult();
    }

    public virtual void DeleteRange(IEnumerable<Entity> entities)
    {
        foreach (var entity in entities)
        {
            OnDeletingAsync(entity).GetAwaiter().GetResult();
        }
        _context.DbSet.RemoveRange(entities);
        _context.DbContext.SaveChanges();
        foreach (var entity in entities)
        {
            OnDeletedAsync(entity).GetAwaiter().GetResult();
        }
    }

    public virtual void Update(Entity entity)
    {
        OnUpdatingAsync(entity).GetAwaiter().GetResult();
        _context.DbSet.Update(entity);
        _context.DbContext.SaveChanges();
        OnUpdatedAsync(entity).GetAwaiter().GetResult();
    }

    public virtual void UpdateRange(IEnumerable<Entity> entities)
    {
        foreach (var entity in entities)
        {
            OnUpdatingAsync(entity).GetAwaiter().GetResult();
        }
        _context.DbSet.UpdateRange(entities);
        _context.DbContext.SaveChanges();
        foreach (var entity in entities)
        {
            OnUpdatedAsync(entity).GetAwaiter().GetResult();
        }
    }

}
#endif
