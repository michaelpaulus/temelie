#nullable enable
namespace Temelie.Database.Models.ChangeTracking;

public partial record ChangeTrackingMapping
{

    public virtual int ChangeTrackingMappingId { get; set; }

    public virtual string SourceSchemaName { get; set; } = "";
    public virtual string SourceTableName { get; set; } = "";
    public virtual string TargetSchemaName { get; set; } = "";
    public virtual string TargetTableName { get; set; } = "";

    public virtual required byte[] LastSyncedVersion { get; set; }

    public virtual DateTime CreatedDate { get; set; }

    public virtual string CreatedBy { get; set; } = "";

    public virtual DateTime ModifiedDate { get; set; }

    public virtual string ModifiedBy { get; set; } = "";

    public virtual bool IsSyncing { get; set; }

}

