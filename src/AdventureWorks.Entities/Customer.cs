using Cornerstone.Repository;

namespace AdventureWorks.Entities;

public record Customer : IEntity<Customer>
{
    public CustomerId CustomerId { get; set; }
    public string Name { get; set; } = "";

}
