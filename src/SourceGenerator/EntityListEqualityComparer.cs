namespace Temelie.Repository.SourceGenerator;

public class EntityListEqualityComparer : IEqualityComparer<(string Namespace, IEnumerable<Entity> Entities)>
{
    public bool Equals((string Namespace, IEnumerable<Entity> Entities) x, (string Namespace, IEnumerable<Entity> Entities) y)
    {
        return x.Namespace.Equals(y.Namespace) &&
            new EquatableArray<Entity>(x.Entities.ToArray()).Equals(new EquatableArray<Entity>(y.Entities.ToArray()));
    }

    public int GetHashCode((string Namespace, IEnumerable<Entity> Entities) obj)
    {
        return new EquatableArray<Entity>(obj.Entities.ToArray()).GetHashCode();
    }
}
