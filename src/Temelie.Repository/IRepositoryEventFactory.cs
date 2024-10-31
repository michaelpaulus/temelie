using Temelie.Entities;

namespace Temelie.Repository;
public interface IRepositoryEventFactory
{
    IEnumerable<IRepositoryEventProvider<Entity>> GetEventProviders<Entity>() where Entity : EntityBase, IEntity<Entity>;
}
