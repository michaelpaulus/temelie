namespace Temelie.Database.Models.ChangeTracking;
public record ChangeTrackingColumn
{
    public int ColumnId { get; set; }
    public required string Name { get; set; }
    public int SystemTypeId { get; set; }
    public int? MaxLength { get; set; }
    public int? Precision { get; set; }
    public int? Scale { get; set; }
    public bool IsNullable { get; set; }
    public bool IsComputed { get; set; }
}
