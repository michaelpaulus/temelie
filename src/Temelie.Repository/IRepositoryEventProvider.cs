using Temelie.Entities;

namespace Temelie.Repository;
public interface IRepositoryEventProvider<Entity> where Entity : EntityBase, IEntity<Entity>
{
    Task<IQueryable<Entity>> OnQueryAsync(IRepositoryContext context, IQueryable<Entity> query);
    Task OnAddingAsync(IRepositoryContext context, Entity entity);
    Task OnAddedAsync(IRepositoryContext context, Entity entity);
    Task OnDeletingAsync(IRepositoryContext context, Entity entity);
    Task OnDeletedAsync(IRepositoryContext context, Entity entity);
    Task OnUpdatingAsync(IRepositoryContext context, Entity entity);
    Task OnUpdatedAsync(IRepositoryContext context, Entity entity);

}
