using Cornerstone.Repository;

namespace AdventureWorks.Entities;

public record CustomerOrder : IEntity<CustomerOrder>
{
    public CustomerId CustomerId { get; set; }
    public OrderId OrderId { get; set; }
    public string Name { get; set; } = "";

}
