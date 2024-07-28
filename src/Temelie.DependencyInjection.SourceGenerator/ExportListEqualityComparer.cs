using Temelie.DependencyInjection.SourceGenerator;

namespace Temelie.Repository.SourceGenerator;

public class ExportListEqualityComparer : IEqualityComparer<IEnumerable<Export>>
{
    public bool Equals(IEnumerable<Export> x, IEnumerable<Export> y)
    {
        return new EnumerableEqualityComparer<Export>().Equals(x, y);
    }

    public int GetHashCode(IEnumerable<Export> obj)
    {
        return obj.GetHashCode();
    }
}
