namespace Temelie.Database.Models.ChangeTracking;
public record ChangeTrackingTableAndMapping
{
    public required ChangeTrackingTable Table { get; set; }
    public required ChangeTrackingMapping Mapping { get; set; }
}
