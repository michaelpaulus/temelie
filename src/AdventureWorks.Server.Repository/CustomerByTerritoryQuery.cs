using AdventureWorks.Entities;
using Temelie.Repository;

namespace AdventureWorks.Server.Repository;
public class CustomerByTerritoryQuery : IQuerySpec2<Customer, CustomerByTerritory>
{
    public IQueryable<CustomerByTerritory> Apply(IRepositoryContext context, IQueryable<Customer> query)
    {
        return query
            .Where(i => i.TerritoryId.HasValue)
            .GroupBy(i => i.TerritoryId.GetValueOrDefault())
            .Select(i => new CustomerByTerritory
            {
                TerritoryId = i.Key,
                Count = i.Count()
            });
    }
}

