using AdventureWorks.Entities;
using Temelie.Repository;

namespace AdventureWorks.Repository;
public class CustomerByAccountNumberQuery(string accountNumber) : IQuerySpec<Customer>
{
    public IQueryable<Customer> Apply(IQueryable<Customer> query)
    {
        return query.Where(i => i.AccountNumber == accountNumber).OrderBy(i => i.CustomerId);
    }
}
