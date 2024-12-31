#nullable enable

using Temelie.Entities;

namespace AdventureWorks.Entities;

[System.ComponentModel.DataAnnotations.Schema.Table("Address1")]
public record Address1 : EntityBase, IEntity<Address1>, IProjectEntity
{
    [System.ComponentModel.DataAnnotations.Key]
    [System.ComponentModel.DataAnnotations.Schema.Column(Order = 0)]
    [System.ComponentModel.DataAnnotations.Schema.DatabaseGenerated(System.ComponentModel.DataAnnotations.Schema.DatabaseGeneratedOption.Identity)]
    public Guid Address1Id { get; set; } = default;
    public string AddressLine1 { get; set; } = "";
    public string? AddressLine2 { get; set; }
    public string City { get; set; } = "";
    public int StateProvinceId { get; set; }
    public string PostalCode { get; set; } = "";
    public string? SpatialLocation { get; set; }
    public System.Guid rowguid { get; set; }
    public System.DateTime ModifiedDate { get; set; }

}
