namespace Temelie.Repository.SourceGenerator;

public class EntityListEqualityComparer : IEqualityComparer<(string Namespace, IEnumerable<Entity> Entities)>
{
    public bool Equals((string Namespace, IEnumerable<Entity> Entities) x, (string Namespace, IEnumerable<Entity> Entities) y)
    {
        return x.Namespace.Equals(y.Namespace) &&
            new EnumerableEqualityComparer<Entity>().Equals(x.Entities, y.Entities);
    }

    public int GetHashCode((string Namespace, IEnumerable<Entity> Entities) obj)
    {
        return obj.Namespace.GetHashCode();
    }
}
