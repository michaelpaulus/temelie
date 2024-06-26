#if NETSTANDARD 
using Temelie.Entities;
namespace Temelie.Repository;
public partial interface IRepository
{
    Entity? GetSingle<Entity>(IQuerySpec<Entity> spec) where Entity : EntityBase, IEntity<Entity>;
    IEnumerable<Entity> GetList<Entity>(IQuerySpec<Entity> spec) where Entity : EntityBase, IEntity<Entity>;
    int GetCount<Entity>(IQuerySpec<Entity> spec) where Entity : EntityBase, IEntity<Entity>;

    void Add<Entity>(Entity entity) where Entity : EntityBase, IEntity<Entity>;
    void AddRange<Entity>(IEnumerable<Entity> entities) where Entity : EntityBase, IEntity<Entity>;
    void Update<Entity>(Entity entity) where Entity : EntityBase, IEntity<Entity>;
    void UpdateRange<Entity>(IEnumerable<Entity> entities) where Entity : EntityBase, IEntity<Entity>;
    void Delete<Entity>(Entity entity) where Entity : EntityBase, IEntity<Entity>;
    void DeleteRange<Entity>(IEnumerable<Entity> entities) where Entity : EntityBase, IEntity<Entity>;
}
#endif
