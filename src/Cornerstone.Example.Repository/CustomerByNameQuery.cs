using Cornerstone.Example.Entities;
using Cornerstone.Repository;

namespace Cornerstone.Example.Repository;
public class CustomerByNameQuery(string name) : IQuerySpec<Customer>
{
    public IQueryable<Customer> Apply(IQueryable<Customer> query)
    {
        return query.Where(i => i.Name == name).OrderBy(i => i.CustomerId);
    }
}
