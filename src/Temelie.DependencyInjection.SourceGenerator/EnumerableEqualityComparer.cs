
namespace Temelie.Repository.SourceGenerator;
internal class EnumerableEqualityComparer<T> : IEqualityComparer<IEnumerable<T>>
{
    public bool Equals(IEnumerable<T> x, IEnumerable<T> y)
    {
        return x.Count() == y.Count() &&
            x.Zip(y, (left, right) => (left, right)).All(i => i.left.Equals(i.right));
    }

    public int GetHashCode(IEnumerable<T> obj)
    {
        return obj.GetHashCode();
    }
}
