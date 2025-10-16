namespace Temelie.Database.Models.ChangeTracking;
public record ChangeTrackingPkColumn
{
    public required string Name { get; set; }
    public int ColumnId { get; set; }
    public int KeyOrdinal { get; set; }
}
