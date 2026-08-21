using System.Linq.Expressions;
using System.Reflection;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Query;
using Temelie.Entities;

namespace Temelie.Repository;

public abstract partial class RepositoryBase
{
    private readonly IRepositoryEventFactory _repositoryEventFactory;

    public RepositoryBase(IRepositoryEventFactory repositoryEventFactory)
    {
        _repositoryEventFactory = repositoryEventFactory;
    }

    protected abstract IRepositoryContext CreateContext();
    protected abstract string GetCreatedModifiedBy();

    protected virtual async Task<Entity?> GetSingleInternalAsync<Entity>(IQuerySpec<Entity> spec) where Entity : EntityBase, IEntity<Entity>
    {
        using var context = CreateContext();
        var query = context.DbContext.Set<Entity>().AsNoTracking();
        query = await OnQueryAsync(context, query, spec.Apply).ConfigureAwait(false);
        return await query.FirstOrDefaultAsync().ConfigureAwait(false);
    }

    protected virtual async Task<IEnumerable<Entity>> GetListInternalAsync<Entity>(IQuerySpec<Entity> spec) where Entity : EntityBase, IEntity<Entity>
    {
        using var context = CreateContext();
        var query = context.DbContext.Set<Entity>().AsNoTracking();
        query = await OnQueryAsync(context, query, spec.Apply).ConfigureAwait(false);
        return await query.ToListAsync().ConfigureAwait(false);
    }

    protected virtual async Task<IEnumerable<TReturn>> GetListInternalAsync<Entity, TReturn>(IQueryAndTransformSpec<Entity, TReturn> spec) where Entity : EntityBase, IEntity<Entity>
    {
        using var context = CreateContext();
        var query = context.DbContext.Set<Entity>().AsNoTracking();
        query = await OnQueryAsync(context, query, spec.Apply).ConfigureAwait(false);
        var query1 = spec.Transform(context, query);
        return await query1.ToListAsync().ConfigureAwait(false);
    }

    protected virtual async Task<int> GetCountInternalAsync<Entity>(IQuerySpec<Entity> spec) where Entity : EntityBase, IEntity<Entity>
    {
        using var context = CreateContext();
        var query = context.DbContext.Set<Entity>().AsNoTracking();
        query = await OnQueryAsync(context, query, spec.Apply).ConfigureAwait(false);
        return await query.CountAsync().ConfigureAwait(false);
    }

    protected virtual async Task<int> GetCountInternalAsync<Entity, TReturn>(IQueryAndTransformSpec<Entity, TReturn> spec) where Entity : EntityBase, IEntity<Entity>
    {
        using var context = CreateContext();
        var query = context.DbContext.Set<Entity>().AsNoTracking();
        query = await OnQueryAsync(context, query, spec.Apply).ConfigureAwait(false);
        var query1 = spec.Transform(context, query);
        return await query1.CountAsync().ConfigureAwait(false);
    }

    protected async Task<Entity?> GetSingleInternalAsync<Entity>(Expression<Func<Entity, bool>>? filter = null, Func<IQueryable<Entity>, IQueryable<Entity>>? query = null) where Entity : EntityBase, IEntity<Entity>
    {
        using var context = CreateContext();
        var query1 = context.DbContext.Set<Entity>().AsNoTracking();
        query1 = await OnQueryAsync(context, query1,
            (context, i) =>
            {
                if (filter is not null)
                {
                    i = i.Where(filter);
                }
                if (query is not null)
                {
                    i = query.Invoke(i);
                }
                return i;
            }
            ).ConfigureAwait(false);
        return await query1.FirstOrDefaultAsync().ConfigureAwait(false);
    }

    protected async Task<IEnumerable<Entity>> GetListInternalAsync<Entity>(Expression<Func<Entity, bool>>? filter = null, Func<IQueryable<Entity>, IQueryable<Entity>>? query = null) where Entity : EntityBase, IEntity<Entity>
    {
        using var context = CreateContext();
        var query1 = context.DbContext.Set<Entity>().AsNoTracking();
        query1 = await OnQueryAsync(context, query1,
            (context, i) =>
            {
                if (filter is not null)
                {
                    i = i.Where(filter);
                }
                if (query is not null)
                {
                    i = query.Invoke(i);
                }
                return i;
            }
            ).ConfigureAwait(false);
        return await query1.ToListAsync().ConfigureAwait(false);
    }

    protected async Task<int> GetCountInternalAsync<Entity>(Expression<Func<Entity, bool>>? filter = null, Func<IQueryable<Entity>, IQueryable<Entity>>? query = null) where Entity : EntityBase, IEntity<Entity>
    {
        using var context = CreateContext();
        var query1 = context.DbContext.Set<Entity>().AsNoTracking();
        query1 = await OnQueryAsync(context, query1,
            (context, i) =>
            {
                if (filter is not null)
                {
                    i = i.Where(filter);
                }
                if (query is not null)
                {
                    i = query.Invoke(i);
                }
                return i;
            }
            ).ConfigureAwait(false);
        return await query1.CountAsync().ConfigureAwait(false);
    }

    protected virtual async Task<bool> GetAnyInternalAsync<Entity>(IQuerySpec<Entity> spec) where Entity : EntityBase, IEntity<Entity>
    {
        using var context = CreateContext();
        var query = context.DbContext.Set<Entity>().AsNoTracking();
        query = await OnQueryAsync(context, query, spec.Apply).ConfigureAwait(false);
        return await query.AnyAsync().ConfigureAwait(false);
    }

    protected virtual async Task<bool> GetAnyInternalAsync<Entity, TReturn>(IQueryAndTransformSpec<Entity, TReturn> spec) where Entity : EntityBase, IEntity<Entity>
    {
        using var context = CreateContext();
        var query = context.DbContext.Set<Entity>().AsNoTracking();
        query = await OnQueryAsync(context, query, spec.Apply).ConfigureAwait(false);
        var query1 = spec.Transform(context, query);
        return await query1.AnyAsync().ConfigureAwait(false);
    }

    protected async Task<bool> GetAnyInternalAsync<Entity>(Expression<Func<Entity, bool>>? filter = null, Func<IQueryable<Entity>, IQueryable<Entity>>? query = null) where Entity : EntityBase, IEntity<Entity>
    {
        using var context = CreateContext();
        var query1 = context.DbContext.Set<Entity>().AsNoTracking();
        query1 = await OnQueryAsync(context, query1,
            (context, i) =>
            {
                if (filter is not null)
                {
                    i = i.Where(filter);
                }
                if (query is not null)
                {
                    i = query.Invoke(i);
                }
                return i;
            }
            ).ConfigureAwait(false);
        return await query1.AnyAsync().ConfigureAwait(false);
    }

    protected virtual async Task AddInternalAsync<Entity>(Entity entity) where Entity : EntityBase, IEntity<Entity>
    {
        using var context = CreateContext();
        await OnAddingAsync(context, entity).ConfigureAwait(false);
        context.DbContext.Entry(entity).State = EntityState.Added;
        await context.DbContext.SaveChangesAsync().ConfigureAwait(false);
        await OnAddedAsync(context, entity).ConfigureAwait(false);
    }

    protected virtual async Task AddRangeInternalAsync<Entity>(IEnumerable<Entity> entities) where Entity : EntityBase, IEntity<Entity>
    {
        using var context = CreateContext();
        foreach (var entity in entities)
        {
            await OnAddingAsync(context, entity).ConfigureAwait(false);
            context.DbContext.Entry(entity).State = EntityState.Added;
        }
        await context.DbContext.SaveChangesAsync().ConfigureAwait(false);
        foreach (var entity in entities)
        {
            await OnAddedAsync(context, entity).ConfigureAwait(false);
        }
    }

    protected virtual async Task DeleteInternalAsync<Entity>(Entity entity) where Entity : EntityBase, IEntity<Entity>
    {
        using var context = CreateContext();
        await OnDeletingAsync(context, entity).ConfigureAwait(false);
        context.DbContext.Entry(entity).State = EntityState.Deleted;
        await context.DbContext.SaveChangesAsync().ConfigureAwait(false);
        await OnDeletedAsync(context, entity).ConfigureAwait(false);
    }

    protected virtual async Task DeleteRangeInternalAsync<Entity>(IEnumerable<Entity> entities) where Entity : EntityBase, IEntity<Entity>
    {
        using var context = CreateContext();
        foreach (var entity in entities)
        {
            await OnDeletingAsync(context, entity).ConfigureAwait(false);
            context.DbContext.Entry(entity).State = EntityState.Deleted;
        }
        await context.DbContext.SaveChangesAsync().ConfigureAwait(false);
        foreach (var entity in entities)
        {
            await OnDeletedAsync(context, entity).ConfigureAwait(false);
        }
    }

    protected virtual async Task DeleteFromQueryInternalAsync<Entity>(IQuerySpec<Entity> spec) where Entity : EntityBase, IEntity<Entity>
    {
        using var context = CreateContext();
        var query = context.DbContext.Set<Entity>().AsNoTracking();
        query = await OnQueryAsync(context, query, spec.Apply).ConfigureAwait(false);
        await query.ExecuteDeleteAsync().ConfigureAwait(false);
    }

    protected async Task DeleteFromQueryInternalAsync<Entity>(Expression<Func<Entity, bool>>? filter = null, Func<IQueryable<Entity>, IQueryable<Entity>>? query = null) where Entity : EntityBase, IEntity<Entity>
    {
        using var context = CreateContext();
        var query1 = context.DbContext.Set<Entity>().AsNoTracking();
        query1 = await OnQueryAsync(context, query1,
            (context, i) =>
            {
                if (filter is not null)
                {
                    i = i.Where(filter);
                }
                if (query is not null)
                {
                    i = query.Invoke(i);
                }
                return i;
            }
            ).ConfigureAwait(false);
        await query1.ExecuteDeleteAsync().ConfigureAwait(false);
    }

    protected virtual async Task UpdateFromQueryInternalAsync<Entity>(IQuerySpec<Entity> spec, Action<UpdateSettersBuilder<Entity>> setPropertyCalls) where Entity : EntityBase, IEntity<Entity>
    {
        using var context = CreateContext();
        var query = context.DbContext.Set<Entity>().AsNoTracking();
        query = await OnQueryAsync(context, query, spec.Apply).ConfigureAwait(false);
        await query.ExecuteUpdateAsync(setPropertyCalls).ConfigureAwait(false);
    }

    protected async Task UpdateFromQueryInternalAsync<Entity>(Action<UpdateSettersBuilder<Entity>> setPropertyCalls, Expression<Func<Entity, bool>>? filter = null, Func<IQueryable<Entity>, IQueryable<Entity>>? query = null) where Entity : EntityBase, IEntity<Entity>
    {
        using var context = CreateContext();
        var query1 = context.DbContext.Set<Entity>().AsNoTracking();
        query1 = await OnQueryAsync(context, query1,
            (context, i) =>
            {
                if (filter is not null)
                {
                    i = i.Where(filter);
                }
                if (query is not null)
                {
                    i = query.Invoke(i);
                }
                return i;
            }
            ).ConfigureAwait(false);
        await query1.ExecuteUpdateAsync(setPropertyCalls).ConfigureAwait(false);
    }

    protected virtual async Task UpdateInternalAsync<Entity>(Entity entity) where Entity : EntityBase, IEntity<Entity>
    {
        using var context = CreateContext();
        await OnUpdatingAsync(context, entity).ConfigureAwait(false);
        context.DbContext.Entry(entity).State = EntityState.Modified;
        await context.DbContext.SaveChangesAsync().ConfigureAwait(false);
        await OnUpdatedAsync(context, entity).ConfigureAwait(false);
    }

    protected virtual async Task UpdateRangeInternalAsync<Entity>(IEnumerable<Entity> entities) where Entity : EntityBase, IEntity<Entity>
    {
        using var context = CreateContext();
        foreach (var entity in entities)
        {
            await OnUpdatingAsync(context, entity).ConfigureAwait(false);
            context.DbContext.Entry(entity).State = EntityState.Modified;
        }
        await context.DbContext.SaveChangesAsync().ConfigureAwait(false);
        foreach (var entity in entities)
        {
            await OnUpdatedAsync(context, entity).ConfigureAwait(false);
        }
    }



    protected virtual async Task<IQueryable<Entity>> OnQueryAsync<Entity>(IRepositoryContext context, IQueryable<Entity> query, Func<IRepositoryContext, IQueryable<Entity>, IQueryable<Entity>> apply) where Entity : EntityBase, IEntity<Entity>
    {
        query = apply.Invoke(context, query);
        foreach (var provider in _repositoryEventFactory.GetEventProviders<Entity>())
        {
            query = await provider.OnQueryAsync(context, query).ConfigureAwait(false);
        }
        return query;
    }

    protected virtual async Task OnAddingAsync<Entity>(IRepositoryContext context, Entity entity) where Entity : EntityBase, IEntity<Entity>
    {
        if (entity is ICreatedDateEntity createdDateEntity)
        {
            createdDateEntity.CreatedDate = DateTime.UtcNow;
        }
        if (entity is ICreatedByEntity createdByEntity)
        {
            createdByEntity.CreatedBy = GetCreatedModifiedBy();
        }
        if (entity is IModifiedDateEntity modifiedDateEntity)
        {
            modifiedDateEntity.ModifiedDate = DateTime.UtcNow;
        }
        if (entity is IModifiedByEntity modifiedByEntity)
        {
            modifiedByEntity.ModifiedBy = GetCreatedModifiedBy();
        }

        foreach (var provider in _repositoryEventFactory.GetEventProviders<Entity>())
        {
            await provider.OnAddingAsync(context, entity).ConfigureAwait(false);
        }
    }

    protected virtual async Task OnAddedAsync<Entity>(IRepositoryContext context, Entity entity) where Entity : EntityBase, IEntity<Entity>
    {
        foreach (var provider in _repositoryEventFactory.GetEventProviders<Entity>())
        {
            await provider.OnAddedAsync(context, entity).ConfigureAwait(false);
        }
    }

    protected virtual async Task OnDeletingAsync<Entity>(IRepositoryContext context, Entity entity) where Entity : EntityBase, IEntity<Entity>
    {
        foreach (var provider in _repositoryEventFactory.GetEventProviders<Entity>())
        {
            await provider.OnDeletingAsync(context, entity).ConfigureAwait(false);
        }
    }

    protected virtual async Task OnDeletedAsync<Entity>(IRepositoryContext context, Entity entity) where Entity : EntityBase, IEntity<Entity>
    {
        foreach (var provider in _repositoryEventFactory.GetEventProviders<Entity>())
        {
            await provider.OnDeletedAsync(context, entity).ConfigureAwait(false);
        }
    }

    protected virtual async Task OnUpdatingAsync<Entity>(IRepositoryContext context, Entity entity) where Entity : EntityBase, IEntity<Entity>
    {
        if (entity is IModifiedDateEntity modifiedDateEntity)
        {
            modifiedDateEntity.ModifiedDate = DateTime.UtcNow;
        }
        if (entity is IModifiedByEntity modifiedByEntity)
        {
            modifiedByEntity.ModifiedBy = GetCreatedModifiedBy();
        }
        foreach (var provider in _repositoryEventFactory.GetEventProviders<Entity>())
        {
            await provider.OnUpdatingAsync(context, entity).ConfigureAwait(false);
        }
    }

    protected virtual async Task OnUpdatedAsync<Entity>(IRepositoryContext context, Entity entity) where Entity : EntityBase, IEntity<Entity>
    {
        foreach (var provider in _repositoryEventFactory.GetEventProviders<Entity>())
        {
            await provider.OnUpdatedAsync(context, entity).ConfigureAwait(false);
        }
    }

    protected async Task<int> InsertFromQueryInternalAsync<TSource, TTarget>(Expression<Func<TSource, bool>>? filter, Expression<Func<TSource, TTarget>> selector)
       where TSource : EntityBase, IEntity<TSource>
       where TTarget : EntityBase, IEntity<TTarget>
    {
        using var context = CreateContext();

        IQueryable<TSource> source = context.DbContext.Set<TSource>();
        if (filter is not null)
        {
            source = source.Where((Expression<Func<TSource, bool>>)QueryConstantInliner.Inline(filter));
        }

        // Inline captured values to constants so the translated SELECT has no parameters and can be
        // embedded after the INSERT prefix.
        var inlinedSelector = (Expression<Func<TSource, TTarget>>)QueryConstantInliner.Inline(selector);
        var parameter = inlinedSelector.Parameters[0];
        var bindings = ((MemberInitExpression)inlinedSelector.Body).Bindings
            .OfType<MemberAssignment>()
            .ToList();

        var entityType = context.DbContext.Model.FindEntityType(typeof(TTarget))
            ?? throw new InvalidOperationException($"No entity type mapped for {typeof(TTarget).Name}.");
        var tableName = entityType.GetTableName()
            ?? throw new InvalidOperationException($"{typeof(TTarget).Name} is not mapped to a table.");
        var storeObject = StoreObjectIdentifier.Table(tableName, entityType.GetSchema());

        // Translate each binding as its own single-column query so that identical constant values are
        // not collapsed by EF Core's projection de-duplication, which would emit fewer SELECT columns
        // than the INSERT column list.
        var columns = new List<string>();
        var projections = new List<string>();
        string? fromClause = null;

        foreach (var binding in bindings)
        {
            var column = entityType.FindProperty(binding.Member.Name)?.GetColumnName(storeObject)
                ?? throw new InvalidOperationException($"No column mapped for {typeof(TTarget).Name}.{binding.Member.Name}.");
            columns.Add($"`{column}`");

            var valueLambda = Expression.Lambda(binding.Expression, parameter);
            var valueQuery = (IQueryable)SelectMethod
                .MakeGenericMethod(typeof(TSource), binding.Expression.Type)
                .Invoke(null, [source, valueLambda])!;

            var (projection, from) = SplitSelect(valueQuery.ToQueryString());
            projections.Add(projection);

            if (fromClause is null)
            {
                fromClause = from;
            }
            else if (!string.Equals(fromClause, from, StringComparison.Ordinal))
            {
                throw new NotSupportedException(
                    "InsertFromQuery does not support selector projections that introduce joins or subqueries.");
            }
        }

        var selectSql = $"SELECT {string.Join(", ", projections)}\n{fromClause}";
        var insertSql = $"INSERT INTO `{tableName}` ({string.Join(", ", columns)})\n{selectSql}";
        return await context.DbContext.Database.ExecuteSqlRawAsync(insertSql).ConfigureAwait(false);
    }

    private static readonly MethodInfo SelectMethod = typeof(Queryable).GetMethods()
        .Single(m => m.Name == nameof(Queryable.Select)
            && m.GetParameters().Length == 2
            && m.GetParameters()[1].ParameterType.GetGenericArguments()[0].GetGenericArguments().Length == 2);

    private static (string Projection, string From) SplitSelect(string sql)
    {
        var match = Regex.Match(sql, @"^SELECT (?<projection>.+?)\r?\n(?<from>FROM .*)$", RegexOptions.Singleline);
        if (!match.Success)
        {
            throw new NotSupportedException($"Unable to build INSERT from the generated query:\n{sql}");
        }
        return (match.Groups["projection"].Value, match.Groups["from"].Value);
    }

}
