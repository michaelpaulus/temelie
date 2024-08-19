using Temelie.Entities;

namespace Temelie.Repository;

public partial interface IRepository : IDisposable
{
    Task<Entity?> GetSingleAsync<Entity>(IQuerySpec<Entity> spec) where Entity : EntityBase, IEntity<Entity>;
    Task<Entity?> GetSingleAsync<Entity>(Func<IQueryable<Entity>, IQueryable<Entity>> query) where Entity : EntityBase, IEntity<Entity>;
    Task<IEnumerable<Entity>> GetListAsync<Entity>(IQuerySpec<Entity> spec) where Entity : EntityBase, IEntity<Entity>;
    Task<IEnumerable<Entity>> GetListAsync<Entity>(Func<IQueryable<Entity>, IQueryable<Entity>> query) where Entity : EntityBase, IEntity<Entity>;
    Task<int> GetCountAsync<Entity>(IQuerySpec<Entity> spec) where Entity : EntityBase, IEntity<Entity>;
    Task<int> GetCountAsync<Entity>(Func<IQueryable<Entity>, IQueryable<Entity>> query) where Entity : EntityBase, IEntity<Entity>;

    Task AddAsync<Entity>(Entity entity) where Entity : EntityBase, IEntity<Entity>;
    Task AddRangeAsync<Entity>(IEnumerable<Entity> entities) where Entity : EntityBase, IEntity<Entity>;
    Task UpdateAsync<Entity>(Entity entity) where Entity : EntityBase, IEntity<Entity>;
    Task UpdateRangeAsync<Entity>(IEnumerable<Entity> entities) where Entity : EntityBase, IEntity<Entity>;
    Task DeleteAsync<Entity>(Entity entity) where Entity : EntityBase, IEntity<Entity>;
    Task DeleteRangeAsync<Entity>(IEnumerable<Entity> entities) where Entity : EntityBase, IEntity<Entity>;
}
