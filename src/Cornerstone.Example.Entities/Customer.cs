using Cornerstone.Repository;

namespace Cornerstone.Example.Entities;

public record Customer : IEntity<Customer>
{
    public CustomerId CustomerId { get; set; }
    public string Name { get; set; } = "";

}
