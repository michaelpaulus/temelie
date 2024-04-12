using Cornerstone.Entities;
using Microsoft.EntityFrameworkCore;

namespace Cornerstone.Repository.EntityFrameworkCore;

public interface IRepositoryContext<Entity> where Entity : class, IEntity<Entity>
{

    DbContext DbContext { get; }

    DbSet<Entity> DbSet { get; }

}
