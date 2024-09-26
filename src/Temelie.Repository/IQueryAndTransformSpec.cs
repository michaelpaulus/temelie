using Temelie.Entities;
namespace Temelie.Repository;
public interface IQueryAndTransformSpec<Entity, TReturn> : IQuerySpec<Entity> where Entity : EntityBase, IEntity<Entity>
{
    IQueryable<TReturn> Transform(IRepositoryContext context, IQueryable<Entity> query);
}
