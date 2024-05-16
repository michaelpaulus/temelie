using Temelie.Entities;
using Microsoft.EntityFrameworkCore;

namespace Temelie.Repository.EntityFrameworkCore;

public interface IRepositoryContext<Entity> where Entity : EntityBase, IEntity<Entity>
{

    DbContext DbContext { get; }

    DbSet<Entity> DbSet { get; }

}
