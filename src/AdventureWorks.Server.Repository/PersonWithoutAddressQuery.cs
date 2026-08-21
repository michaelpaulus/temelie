using AdventureWorks.Entities;
using Temelie.Repository;

namespace AdventureWorks.Server.Repository;

public class PersonWithoutAddressQuery : IQuerySpec<Person>
{
    public IQueryable<Person> Apply(IRepositoryContext context, IQueryable<Person> query)
    {
        return query.Where(p =>
            !context.DbContext.Set<BusinessEntityAddress>().Any(a => a.BusinessEntityId == p.BusinessEntityId));
    }
}
