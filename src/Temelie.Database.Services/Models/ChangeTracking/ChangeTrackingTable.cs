namespace Temelie.Database.Models.ChangeTracking;

public partial record ChangeTrackingTable
{

    public virtual string SchemaName { get; set; } = "";

    public virtual string TableName { get; set; } = "";

    public virtual long? CurrentVersion { get; set; }

}
