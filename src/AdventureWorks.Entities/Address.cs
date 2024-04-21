using Temelie.Entities;
#nullable enable
namespace AdventureWorks.Entities;

[System.ComponentModel.DataAnnotations.Schema.Table("Address1")]
public record Address1 : IEntity<Address1>
{
    [System.ComponentModel.DataAnnotations.Key]
    [System.ComponentModel.DataAnnotations.Schema.Column(Order = 0)]
    [System.ComponentModel.DataAnnotations.Schema.DatabaseGenerated(System.ComponentModel.DataAnnotations.Schema.DatabaseGeneratedOption.Identity)]
    public AddressId AddressId { get; set; } = default;
    public string AddressLine1 { get; set; } = "";
    public string? AddressLine2 { get; set; }
    public string City { get; set; } = "";
    public int StateProvinceId { get; set; } = 0;
    public string PostalCode { get; set; } = "";
    public string? SpatialLocation { get; set; }
    public System.Guid rowguid { get; set; } = default;
    public System.DateTime ModifiedDate { get; set; } = default;

}
