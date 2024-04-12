namespace Cornerstone.Entities;

public interface IEntity<Entity> where Entity : class, IEntity<Entity>
{

}
