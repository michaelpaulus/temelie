using Temelie.Entities;
using Microsoft.EntityFrameworkCore;

namespace Temelie.Repository.EntityFrameworkCore;

public interface IRepositoryContext<Entity> where Entity : class, IEntity<Entity>
{

    DbContext DbContext { get; }

    DbSet<Entity> DbSet { get; }

}
