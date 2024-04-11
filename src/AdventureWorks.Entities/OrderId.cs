using Cornerstone.Repository;

namespace Cornerstone.Example.Entities;
public record struct OrderId(int Value = 0) : IEntityId, IComparable<OrderId>
{
    public int CompareTo(OrderId other)
    {
        return Value.CompareTo(other.Value);
    }
}
