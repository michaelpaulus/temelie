using Temelie.Entities;

namespace Temelie.Repository;
public interface IRepositoryEventProvider<Entity> where Entity : EntityBase, IEntity<Entity>
{
    Task<IQueryable<Entity>> OnQueryAsync(IRepositoryContext context, IQueryable<Entity> query);
    Task OnAddingAsync(Entity entity);
    Task OnAddedAsync(Entity entity);
    Task OnDeletingAsync(Entity entity);
    Task OnDeletedAsync(Entity entity);
    Task OnUpdatingAsync(Entity entity);
    Task OnUpdatedAsync(Entity entity);

}
