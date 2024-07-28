namespace Temelie.Entities.SourceGenerator;

internal class ColumnProperty
{
    public string PropertyName { get; set; }
    public string PropertyType { get; set; }
    public string Default { get; set; }
    public bool IsNullable { get; set; }
    public bool IsPrimaryKey { get; set; }
    public bool IsComputed { get; set; }
    public bool IsIdentity { get; set; }
    public string ColumnName { get; set; }
    public int ColumnId { get; set; }
    public int? Precision { get; set; }
    public int? Scale { get; set; }
    public bool IsForeignKey { get; internal set; }
    public string SystemTypeString { get; internal set; }
}

