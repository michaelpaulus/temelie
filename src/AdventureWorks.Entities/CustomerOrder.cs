using Cornerstone.Repository;

namespace Cornerstone.Example.Entities;

public record CustomerOrder : IEntity<CustomerOrder>
{
    public CustomerId CustomerId { get; set; }
    public OrderId OrderId { get; set; }
    public string Name { get; set; } = "";

}
