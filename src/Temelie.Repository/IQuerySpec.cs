using Temelie.Entities;
namespace Temelie.Repository;
public interface IQuerySpec<Entity> where Entity : EntityBase, IEntity<Entity>
{
    IQueryable<Entity> Apply(IRepositoryContext context, IQueryable<Entity> query);
}
