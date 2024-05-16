namespace Temelie.Entities;
public interface ICreatedByEntity
{
    DateTime CreatedDate { get; set; }
    string CreatedBy { get; set; }
}
