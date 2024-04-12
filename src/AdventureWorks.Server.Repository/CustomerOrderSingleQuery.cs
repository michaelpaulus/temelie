using AdventureWorks.Entities;
using Cornerstone.Repository;

namespace AdventureWorks.Repository;
public class CustomerOrderSingleQuery(CustomerId customerId, OrderId orderId) : IQuerySpec<CustomerOrder>
{
    public IQueryable<CustomerOrder> Apply(IQueryable<CustomerOrder> query)
    {
        return query.Where(i => i.CustomerId == customerId && i.OrderId == orderId);
    }
}
