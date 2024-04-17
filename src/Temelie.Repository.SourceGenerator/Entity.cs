namespace Temelie.Repository.SourceGenerator;
public readonly record struct Entity(string FullType, string Name, string TableName, string Schema, bool IsView, EntityProperty[] Properties)
{
}
