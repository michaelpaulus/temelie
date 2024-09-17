using Temelie.Entities;
namespace Temelie.Repository;
public interface IQuerySpec2<Entity, TReturn> where Entity : EntityBase, IEntity<Entity>
{
    IQueryable<TReturn> Apply(IRepositoryContext context, IQueryable<Entity> query);
}
