using AdventureWorks.Entities;
using Temelie.Repository;

namespace AdventureWorks.Server.Repository;
public class TestExistsQuery(int storeId, string territoryName) : IQuerySpec<Customer>
{

    public IQueryable<Customer> Apply(IRepositoryContext context, IQueryable<Customer> query)
    {
        query = query.Where(i => i.StoreId == storeId);

        query = query.Where(i => context.DbContext.Set<SalesTerritory>().Any(i2 => i2.TerritoryId == i.TerritoryId && i2.Name == territoryName));

        return query.OrderBy(i => i.CustomerId);
    }
}
