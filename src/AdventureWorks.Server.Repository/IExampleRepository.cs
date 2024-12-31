using System.Linq.Expressions;
using Temelie.Entities;
using Temelie.Repository;
using AdventureWorks.Entities;

namespace AdventureWorks.Server;

public partial interface IExampleRepository
{
    Task<Entity?> GetSingleAsync<Entity>(IQuerySpec<Entity> spec) where Entity : EntityBase, IEntity<Entity>, IProjectEntity;
    Task<Entity?> GetSingleAsync<Entity>(Expression<Func<Entity, bool>>? filter = null, Func<IQueryable<Entity>, IQueryable<Entity>>? query = null) where Entity : EntityBase, IEntity<Entity>, IProjectEntity;
    Task<IEnumerable<Entity>> GetListAsync<Entity>(IQuerySpec<Entity> spec) where Entity : EntityBase, IEntity<Entity>, IProjectEntity;
    Task<IEnumerable<TReturn>> GetListAsync<Entity, TReturn>(IQueryAndTransformSpec<Entity, TReturn> spec) where Entity : EntityBase, IEntity<Entity>, IProjectEntity;
    Task<IEnumerable<Entity>> GetListAsync<Entity>(Expression<Func<Entity, bool>>? filter = null, Func<IQueryable<Entity>, IQueryable<Entity>>? query = null) where Entity : EntityBase, IEntity<Entity>, IProjectEntity;
    Task<int> GetCountAsync<Entity>(IQuerySpec<Entity> spec) where Entity : EntityBase, IEntity<Entity>, IProjectEntity;
    Task<int> GetCountAsync<Entity, TReturn>(IQueryAndTransformSpec<Entity, TReturn> spec) where Entity : EntityBase, IEntity<Entity>, IProjectEntity;
    Task<int> GetCountAsync<Entity>(Expression<Func<Entity, bool>>? filter = null, Func<IQueryable<Entity>, IQueryable<Entity>>? query = null) where Entity : EntityBase, IEntity<Entity>, IProjectEntity;

    Task AddAsync<Entity>(Entity entity) where Entity : EntityBase, IEntity<Entity>, IProjectEntity;
    Task AddRangeAsync<Entity>(IEnumerable<Entity> entities) where Entity : EntityBase, IEntity<Entity>, IProjectEntity;
    Task UpdateAsync<Entity>(Entity entity) where Entity : EntityBase, IEntity<Entity>, IProjectEntity;
    Task UpdateRangeAsync<Entity>(IEnumerable<Entity> entities) where Entity : EntityBase, IEntity<Entity>, IProjectEntity;
    Task DeleteAsync<Entity>(Entity entity) where Entity : EntityBase, IEntity<Entity>, IProjectEntity;
    Task DeleteRangeAsync<Entity>(IEnumerable<Entity> entities) where Entity : EntityBase, IEntity<Entity>, IProjectEntity;

}
