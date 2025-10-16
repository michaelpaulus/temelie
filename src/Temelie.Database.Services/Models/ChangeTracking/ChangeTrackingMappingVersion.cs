#nullable enable
namespace Temelie.Database.Models.ChangeTracking;

public partial record ChangeTrackingMappingVersion 
{

    public virtual System.Int64 ChangeTrackingPullVersionId { get; set; } 

    public virtual System.Int64 ChangeTrackingPullTableId { get; set; } 

    public virtual System.Int64 VersionId { get; set; } 

    public virtual int RowsInserted { get; set; }

    public virtual int RowsUpdated { get; set; }

    public virtual int RowsDeleted { get; set; }

    public virtual System.DateTime CreatedDate { get; set; }

    public virtual string CreatedBy { get; set; } = "";

    public virtual System.DateTime ModifiedDate { get; set; }

    public virtual string ModifiedBy { get; set; } = "";

}

