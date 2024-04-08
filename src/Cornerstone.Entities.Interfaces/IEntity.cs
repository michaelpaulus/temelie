namespace Cornerstone.Repository;

public interface IEntity<Entity> where Entity : class, IEntity<Entity>
{

}
