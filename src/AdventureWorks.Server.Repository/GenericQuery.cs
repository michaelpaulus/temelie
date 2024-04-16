using Temelie.Entities;
using Temelie.Repository;

namespace AdventureWorks.Server.Repository;
public class GenericQuery<T> : IQuerySpec<T> where T : class, IEntity<T>
{
    private readonly Func<IQueryable<T>, IQueryable<T>> _query;

    public GenericQuery(Func<IQueryable<T>, IQueryable<T>> query)
    {
        _query = query;
    }

    public IQueryable<T> Apply(IQueryable<T> query)
    {
        return _query.Invoke(query);
    }
}
