using Temelie.Entities;
using Microsoft.EntityFrameworkCore;

namespace Temelie.Repository.EntityFrameworkCore;

public interface IRepositoryContext<Entity> : IDisposable where Entity : EntityBase, IEntity<Entity>
{

    DbContext DbContext { get; }

    DbSet<Entity> DbSet { get; }

}
