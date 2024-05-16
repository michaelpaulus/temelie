using Temelie.Entities;
#nullable enable
namespace AdventureWorks.Entities;
public record struct Address1Id(Guid Value) : IEntityId, IComparable<Address1Id>
{
    public int CompareTo(Address1Id other)
    {
        return Value.CompareTo(other.Value);
    }
}


