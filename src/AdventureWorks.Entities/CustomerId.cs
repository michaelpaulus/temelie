using Cornerstone.Repository;

namespace Cornerstone.Example.Entities;
public record struct CustomerId(int Value = 0) : IEntityId, IComparable<CustomerId>
{
    public int CompareTo(CustomerId other)
    {
        return Value.CompareTo(other.Value);
    }
}
