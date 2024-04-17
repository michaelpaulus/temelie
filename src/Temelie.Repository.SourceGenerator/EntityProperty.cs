namespace Temelie.Repository.SourceGenerator;
public readonly record struct EntityProperty(string FullType, string Name, int Order, string ColumnName, bool IsPrimaryKey, bool IsIdentity, bool IsCommputed, bool IsEntityId, bool IsNullable)
{
}
