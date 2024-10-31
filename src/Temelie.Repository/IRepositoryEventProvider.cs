using Temelie.Entities;

namespace Temelie.Repository;
public interface IRepositoryEventProvider<Entity> where Entity : EntityBase, IEntity<Entity>
{
    Task<IQueryable<Entity>> OnQueryAsync(IRepositoryContext context, IQueryable<Entity> query) => Task.FromResult(query);
    Task OnAddingAsync(IRepositoryContext context, Entity entity) => Task.CompletedTask;
    Task OnAddedAsync(IRepositoryContext context, Entity entity) => Task.CompletedTask;
    Task OnDeletingAsync(IRepositoryContext context, Entity entity) => Task.CompletedTask;
    Task OnDeletedAsync(IRepositoryContext context, Entity entity) => Task.CompletedTask;
    Task OnUpdatingAsync(IRepositoryContext context, Entity entity) => Task.CompletedTask;
    Task OnUpdatedAsync(IRepositoryContext context, Entity entity) => Task.CompletedTask;

}
