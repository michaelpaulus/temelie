using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Temelie.Repository.EntityFrameworkCore;

#if NETSTANDARD
public partial class RepositoryBase<Entity>
{

    public virtual Entity? GetSingle(IQuerySpec<Entity> spec)
    {
        using var context = _serviceProvider.GetRequiredService<IRepositoryContext<Entity>>();
        var query = context.DbSet.AsNoTracking();
        query = OnQueryAsync(query, spec).GetAwaiter().GetResult();
        return query.FirstOrDefault();
    }

    public virtual IEnumerable<Entity> GetList(IQuerySpec<Entity> spec)
    {
        using var context = _serviceProvider.GetRequiredService<IRepositoryContext<Entity>>();
        var query = context.DbSet.AsNoTracking();
        query = OnQueryAsync(query, spec).GetAwaiter().GetResult();
        return query.ToList();
    }

    public virtual int GetCount(IQuerySpec<Entity> spec)
    {
        using var context = _serviceProvider.GetRequiredService<IRepositoryContext<Entity>>();
        var query = context.DbSet.AsNoTracking();
        query = OnQueryAsync(query, spec).GetAwaiter().GetResult();
        return query.Count();
    }

    public virtual void Add(Entity entity)
    {
        using var context = _serviceProvider.GetRequiredService<IRepositoryContext<Entity>>();
        OnAddingAsync(entity).GetAwaiter().GetResult();
        context.DbContext.Entry(entity).State = EntityState.Added;
        context.DbContext.SaveChanges();
        OnAddedAsync(entity).GetAwaiter().GetResult();
    }

    public virtual void AddRange(IEnumerable<Entity> entities)
    {
        using var context = _serviceProvider.GetRequiredService<IRepositoryContext<Entity>>();
        foreach (var entity in entities)
        {
            OnAddingAsync(entity).GetAwaiter().GetResult();
            context.DbContext.Entry(entity).State = EntityState.Added;
        }
        context.DbContext.SaveChanges();
        foreach (var entity in entities)
        {
            OnAddedAsync(entity).GetAwaiter().GetResult();
        }
    }

    public virtual void Delete(Entity entity)
    {
        using var context = _serviceProvider.GetRequiredService<IRepositoryContext<Entity>>();
        OnDeletingAsync(entity).GetAwaiter().GetResult();
        context.DbContext.Entry(entity).State = EntityState.Deleted;
        context.DbContext.SaveChanges();
        OnDeletedAsync(entity).GetAwaiter().GetResult();
    }

    public virtual void DeleteRange(IEnumerable<Entity> entities)
    {
        using var context = _serviceProvider.GetRequiredService<IRepositoryContext<Entity>>();
        foreach (var entity in entities)
        {
            OnDeletingAsync(entity).GetAwaiter().GetResult();
            context.DbContext.Entry(entity).State = EntityState.Deleted;
        }
        context.DbContext.SaveChanges();
        foreach (var entity in entities)
        {
            OnDeletedAsync(entity).GetAwaiter().GetResult();
        }
    }

    public virtual void Update(Entity entity)
    {
        using var context = _serviceProvider.GetRequiredService<IRepositoryContext<Entity>>();
        OnUpdatingAsync(entity).GetAwaiter().GetResult();
        context.DbContext.Entry(entity).State = EntityState.Modified;
        context.DbContext.SaveChanges();
        OnUpdatedAsync(entity).GetAwaiter().GetResult();
    }

    public virtual void UpdateRange(IEnumerable<Entity> entities)
    {
        using var context = _serviceProvider.GetRequiredService<IRepositoryContext<Entity>>();
        foreach (var entity in entities)
        {
            OnUpdatingAsync(entity).GetAwaiter().GetResult();
            context.DbContext.Entry(entity).State = EntityState.Modified;
        }
        context.DbContext.SaveChanges();
        foreach (var entity in entities)
        {
            OnUpdatedAsync(entity).GetAwaiter().GetResult();
        }
    }

}
#endif
