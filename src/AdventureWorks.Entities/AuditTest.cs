#nullable enable

using Temelie.Entities;

namespace AdventureWorks.Entities;

[System.ComponentModel.DataAnnotations.Schema.Table("AuditTest")]
public record AuditTest : EntityBase, IEntity<AuditTest>, IProjectEntity, ICreatedDateEntity, ICreatedByEntity, IModifiedDateEntity, IModifiedByEntity
{
    [System.ComponentModel.DataAnnotations.Key]
    [System.ComponentModel.DataAnnotations.Schema.Column(Order = 0)]
    public int AuditTestId { get; set; }
    public System.DateTime CreatedDate { get; set; }
    public string CreatedBy { get; set; } = "";
    public System.DateTime ModifiedDate { get; set; }
    public string ModifiedBy { get; set; } = "";
}
