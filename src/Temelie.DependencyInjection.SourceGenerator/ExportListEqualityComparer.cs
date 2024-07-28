using Temelie.DependencyInjection.SourceGenerator;

namespace Temelie.Repository.SourceGenerator;

public class ExportListEqualityComparer : IEqualityComparer<IEnumerable<Export>>
{
    public bool Equals(IEnumerable<Export> x, IEnumerable<Export> y)
    {
        return new EquatableArray<Export>(x.ToArray()).Equals(new EquatableArray<Export>(y.ToArray()));
    }

    public int GetHashCode(IEnumerable<Export> obj)
    {
        return new EquatableArray<Export>(obj.ToArray()).GetHashCode();
    }
}
