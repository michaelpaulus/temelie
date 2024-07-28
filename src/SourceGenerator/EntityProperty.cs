namespace Temelie.Repository.SourceGenerator;
public readonly record struct EntityProperty(string FullType, string PropertyType, string Name, int Order, string ColumnName, int? Precision, int? Scale, bool IsPrimaryKey, bool IsIdentity, bool IsCommputed, bool IsNullable)
{
}
