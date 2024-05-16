namespace Temelie.Entities;
public interface IModifiedByEntity
{
    DateTime ModifiedDate { get; set; }
    string ModifiedBy { get; set; }
}
