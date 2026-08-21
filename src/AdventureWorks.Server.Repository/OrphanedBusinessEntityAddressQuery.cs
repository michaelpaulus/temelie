using AdventureWorks.Entities;
using Temelie.Repository;

namespace AdventureWorks.Server.Repository;

public class OrphanedBusinessEntityAddressQuery : IQuerySpec<BusinessEntityAddress>
{
    public IQueryable<BusinessEntityAddress> Apply(IRepositoryContext context, IQueryable<BusinessEntityAddress> query)
    {
        return query.Where(a =>
            !context.DbContext.Set<Person>().Any(p => p.BusinessEntityId == a.BusinessEntityId));
    }
}
