#nullable enable
namespace Temelie.Database.Models.ChangeTracking;
public record ChangeTrackingRow
{
    public required string SchemaName { get; set; }
    public required string TableName { get; set; }
    public required string ChangeOperation { get; set; }
    public Dictionary<string, object?> ColumnValues { get; set; } = new();
}
