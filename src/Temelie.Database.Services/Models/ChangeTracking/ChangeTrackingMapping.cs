#nullable enable
namespace Temelie.Database.Models.ChangeTracking;

public partial record ChangeTrackingMapping
{

    public virtual System.Int64 ChangeTrackingMappingId { get; set; }

    public virtual string SourceSchemaName { get; set; } = "";
    public virtual string SourceTableName { get; set; } = "";
    public virtual string TargetSchemaName { get; set; } = "";
    public virtual string TargetTableName { get; set; } = "";

    public virtual long VersionId { get; set; }

    public virtual System.DateTime CreatedDate { get; set; }

    public virtual string CreatedBy { get; set; } = "";

    public virtual System.DateTime ModifiedDate { get; set; }

    public virtual string ModifiedBy { get; set; } = "";

}

