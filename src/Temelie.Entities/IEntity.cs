namespace Temelie.Entities;

public interface IEntity<Entity> where Entity : EntityBase, IEntity<Entity>
{

}
