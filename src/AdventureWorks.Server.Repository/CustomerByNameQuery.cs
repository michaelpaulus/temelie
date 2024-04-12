using AdventureWorks.Entities;
using Cornerstone.Repository;

namespace AdventureWorks.Repository;
public class CustomerByNameQuery(string name) : IQuerySpec<Customer>
{
    public IQueryable<Customer> Apply(IQueryable<Customer> query)
    {
        return query.Where(i => i.Name == name).OrderBy(i => i.CustomerId);
    }
}
