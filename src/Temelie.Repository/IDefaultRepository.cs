using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Query;
using Temelie.Entities;

namespace Temelie.Repository;

public partial interface IDefaultRepository
{
    Task<Entity?> GetSingleAsync<Entity>(IQuerySpec<Entity> spec) where Entity : EntityBase, IEntity<Entity>;
    Task<Entity?> GetSingleAsync<Entity>(Expression<Func<Entity, bool>>? filter = null, Func<IQueryable<Entity>, IQueryable<Entity>>? query = null) where Entity : EntityBase, IEntity<Entity>;
    Task<IEnumerable<Entity>> GetListAsync<Entity>(IQuerySpec<Entity> spec) where Entity : EntityBase, IEntity<Entity>;
    Task<IEnumerable<TReturn>> GetListAsync<Entity, TReturn>(IQueryAndTransformSpec<Entity, TReturn> spec) where Entity : EntityBase, IEntity<Entity>;
    Task<IEnumerable<Entity>> GetListAsync<Entity>(Expression<Func<Entity, bool>>? filter = null, Func<IQueryable<Entity>, IQueryable<Entity>>? query = null) where Entity : EntityBase, IEntity<Entity>;
    Task<int> GetCountAsync<Entity>(IQuerySpec<Entity> spec) where Entity : EntityBase, IEntity<Entity>;
    Task<int> GetCountAsync<Entity, TReturn>(IQueryAndTransformSpec<Entity, TReturn> spec) where Entity : EntityBase, IEntity<Entity>;
    Task<int> GetCountAsync<Entity>(Expression<Func<Entity, bool>>? filter = null, Func<IQueryable<Entity>, IQueryable<Entity>>? query = null) where Entity : EntityBase, IEntity<Entity>;
    Task<bool> GetAnyAsync<Entity>(IQuerySpec<Entity> spec) where Entity : EntityBase, IEntity<Entity>;
    Task<bool> GetAnyAsync<Entity, TReturn>(IQueryAndTransformSpec<Entity, TReturn> spec) where Entity : EntityBase, IEntity<Entity>;
    Task<bool> GetAnyAsync<Entity>(Expression<Func<Entity, bool>>? filter = null, Func<IQueryable<Entity>, IQueryable<Entity>>? query = null) where Entity : EntityBase, IEntity<Entity>;

    Task AddAsync<Entity>(Entity entity) where Entity : EntityBase, IEntity<Entity>;
    Task AddRangeAsync<Entity>(IEnumerable<Entity> entities) where Entity : EntityBase, IEntity<Entity>;
    Task UpdateAsync<Entity>(Entity entity) where Entity : EntityBase, IEntity<Entity>;
    Task UpdateRangeAsync<Entity>(IEnumerable<Entity> entities) where Entity : EntityBase, IEntity<Entity>;
    Task DeleteAsync<Entity>(Entity entity) where Entity : EntityBase, IEntity<Entity>;
    Task DeleteRangeAsync<Entity>(IEnumerable<Entity> entities) where Entity : EntityBase, IEntity<Entity>;
    Task<int> DeleteFromQueryAsync<Entity>(IQuerySpec<Entity> spec) where Entity : EntityBase, IEntity<Entity>;
    Task<int> DeleteFromQueryAsync<Entity>(Expression<Func<Entity, bool>>? filter = null, Func<IQueryable<Entity>, IQueryable<Entity>>? query = null) where Entity : EntityBase, IEntity<Entity>;
    Task UpdateFromQueryAsync<Entity>(IQuerySpec<Entity> spec, Action<UpdateSettersBuilder<Entity>> setPropertyCalls) where Entity : EntityBase, IEntity<Entity>;
    Task UpdateFromQueryAsync<Entity>(Action<UpdateSettersBuilder<Entity>> setPropertyCalls, Expression<Func<Entity, bool>>? filter = null, Func<IQueryable<Entity>, IQueryable<Entity>>? query = null) where Entity : EntityBase, IEntity<Entity>;
    Task<int> InsertFromQueryAsync<TSource, TTarget>(Expression<Func<TSource, bool>>? filter, Expression<Func<TSource, TTarget>> selector)
        where TSource : EntityBase, IEntity<TSource>
        where TTarget : EntityBase, IEntity<TTarget>;
    Task<int> InsertFromQueryAsync<TSource, TTarget>(IQuerySpec<TSource> spec, Expression<Func<TSource, TTarget>> selector)
        where TSource : EntityBase, IEntity<TSource>
        where TTarget : EntityBase, IEntity<TTarget>;
    Task<MergeResult> MergeFromQueryAsync<TSource, TTarget>(
        Expression<Func<TSource, TTarget, bool>> match,
        Expression<Func<TSource, TTarget>> insertSelector,
        Action<UpdateSettersBuilder<TTarget>>? updateSetters = null,
        bool deleteMissing = false)
        where TSource : EntityBase, IEntity<TSource>
        where TTarget : EntityBase, IEntity<TTarget>;
    Task<MergeResult> MergeFromQueryAsync<TSource, TTarget, TKey>(
        Expression<Func<TSource, TKey>> sourceKey,
        Expression<Func<TTarget, TKey>> targetKey,
        Expression<Func<TSource, TTarget>> insertSelector,
        Action<UpdateSettersBuilder<TTarget>>? updateSetters = null,
        bool deleteMissing = false)
        where TSource : EntityBase, IEntity<TSource>
        where TTarget : EntityBase, IEntity<TTarget>;

}
