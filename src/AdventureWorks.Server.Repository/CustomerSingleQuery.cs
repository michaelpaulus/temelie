using AdventureWorks.Entities;
using Temelie.Repository;

namespace AdventureWorks.Repository;
public class CustomerSingleQuery(CustomerId customerId) : IQuerySpec<Customer>
{
    public IQueryable<Customer> Apply(IQueryable<Customer> query)
    {
        return query.Where(i => i.CustomerId == customerId);
    }
}
