using Temelie.Entities;
using Temelie.Repository;

namespace AdventureWorks.Server.Repository;

public class AllRowsQuery<Entity> : IQuerySpec<Entity> where Entity : EntityBase, IEntity<Entity>
{
    public IQueryable<Entity> Apply(IRepositoryContext context, IQueryable<Entity> query)
    {
        return query;
    }
}
