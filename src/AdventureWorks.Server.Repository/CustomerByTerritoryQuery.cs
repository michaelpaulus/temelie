using AdventureWorks.Entities;
using Temelie.Repository;

namespace AdventureWorks.Server.Repository;
public class CustomerByTerritoryQuery : IQueryAndTransformSpec<Customer, CustomerByTerritory>
{
    public IQueryable<CustomerByTerritory> Transform(IRepositoryContext context, IQueryable<Customer> query)
    {
        return query
            .GroupBy(i => i.TerritoryId.GetValueOrDefault())
            .Select(i => new CustomerByTerritory
            {
                TerritoryId = i.Key,
                Count = i.Count()
            });
    }

    public IQueryable<Customer> Apply(IRepositoryContext context, IQueryable<Customer> query)
    {
        return query
           .Where(i => i.TerritoryId.HasValue);
    }
}

