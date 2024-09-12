namespace Temelie.Repository.SourceGenerator;
public record Entity(string FullType, string Name, string TableName, string Schema, bool IsView, bool HasTrigger, EquatableArray<EntityProperty> Properties)
{
   
}
