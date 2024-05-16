using Temelie.Entities;

namespace Temelie.Repository;

public partial interface IRepository<Entity> : IDisposable where Entity : class, IEntity<Entity>
{
    Task<Entity?> GetSingleAsync(IQuerySpec<Entity> spec);
    Task<IEnumerable<Entity>> GetListAsync(IQuerySpec<Entity> spec);
    Task<int> GetCountAsync(IQuerySpec<Entity> spec);

    Task AddAsync(Entity entity);
    Task AddRangeAsync(IEnumerable<Entity> entities);
    Task UpdateAsync(Entity entity);
    Task UpdateRangeAsync(IEnumerable<Entity> entities);
    Task DeleteAsync(Entity entity);
    Task DeleteRangeAsync(IEnumerable<Entity> entities);

    Task MergeAsync(IEnumerable<Entity> originalEntities, IEnumerable<Entity> newEntities, Func<Entity, IEnumerable<Entity>, Entity> keySelector);

}
