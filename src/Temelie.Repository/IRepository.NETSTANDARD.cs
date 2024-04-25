namespace Temelie.Repository;

#if NETSTANDARD 

public partial interface IRepository<Entity>
{
    Entity? GetSingle(IQuerySpec<Entity> spec);
    IEnumerable<Entity> GetList(IQuerySpec<Entity> spec);
    int GetCount(IQuerySpec<Entity> spec);

    void Add(Entity entity);
    void AddRange(IEnumerable<Entity> entities);
    void Update(Entity entity);
    void UpdateRange(IEnumerable<Entity> entities);
    void Delete(Entity entity);
    void DeleteRange(IEnumerable<Entity> entities);
}

#endif
