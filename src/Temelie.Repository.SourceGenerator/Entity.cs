namespace Temelie.Repository.SourceGenerator;
public class Entity(string fullType, string name, string tableName, string schema, bool isView, EntityProperty[] properties) : IEquatable<Entity>
{
    public string FullType { get; } = fullType;
    public string Name { get; } = name;
    public string TableName { get; } = tableName;
    public string Schema { get; } = schema;
    public bool IsView { get; } = isView;
    public EntityProperty[] Properties { get; } = properties;

    public bool Equals(Entity other)
    {
        if (ReferenceEquals(this, other))
        {
            return true;
        }
        return FullType == other.FullType
            && Name == other.Name
            && TableName == other.TableName
            && Schema == other.Schema
            && IsView == other.IsView
            && new EnumerableEqualityComparer<EntityProperty>().Equals(Properties, other.Properties);
    }

    public override bool Equals(object obj)
    {
        return obj is Entity entity
             && Equals(entity);
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }

}
