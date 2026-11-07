# Temelie.Repository

A lightweight repository on EF Core for [Temelie.Entities](https://github.com/michaelpaulus/temelie/blob/main/src/Temelie.Entities/README.md). It provides query specifications, lifecycle events, automatic audit fields, and set-based bulk operations.

```shell
dotnet add package Temelie.Repository
dotnet add package Temelie.Repository.SourceGenerator
```

## Setup

1. **Context.** Create a `DbContext` from the generated `DbContextBase` (see [Temelie.Repository.SourceGenerator](https://github.com/michaelpaulus/temelie/blob/main/src/Temelie.Repository.SourceGenerator/README.md)):

   ```csharp
   [ExportTransient(typeof(IRepositoryContext))]
   public partial class AppDbContext(IModelBuilderContext context, DbContextOptions<AppDbContext> options)
       : DbContextBase(context, options), IRepositoryContext
   {
       public DbContext DbContext => this;
   }
   ```

2. **Repository.** Create a repository by deriving from `DefaultRepository`, which implements `IDefaultRepository`. Tell it how to create a context and who the current user is:

   ```csharp
   [ExportTransient(typeof(IDefaultRepository))]
   public class AppRepository(IDbContextFactory<AppDbContext> factory, IRepositoryEventFactory events)
       : DefaultRepository(events)
   {
       protected override IRepositoryContext CreateContext() => factory.CreateDbContext();
       protected override string GetCreatedModifiedBy() => "system";   // e.g. the signed-in user
   }
   ```

   To restrict a repository to one project's entities, derive from `RepositoryBase` and constrain to the generated `IProjectEntity`. See `AdventureWorks.Server.Repository` for an example.

3. **EF Core provider.** Register the provider and the context factory, for example in an `IStartupConfiguration`:

   ```csharp
   services.AddDbContextFactory<AppDbContext>(options => options.UseSqlServer(connectionString));
   ```

## Querying

Queries can be inline predicates, or reusable specifications (`IQuerySpec<T>`, `IQueryAndTransformSpec<T, TResult>`):

```csharp
var person = await repository.GetSingleAsync(new PersonSingleQuery(id));        // generated primary-key query
var active = await repository.GetListAsync<Person>(p => p.IsActive, q => q.OrderBy(p => p.LastName));
var count  = await repository.GetCountAsync<Person>(p => p.IsActive);

public class CustomersByTerritory : IQueryAndTransformSpec<Customer, CustomerByTerritory>
{
    public IQueryable<Customer> Apply(IRepositoryContext context, IQueryable<Customer> query) =>
        query.Where(c => c.TerritoryId.HasValue);

    public IQueryable<CustomerByTerritory> Transform(IRepositoryContext context, IQueryable<Customer> query) =>
        query.GroupBy(c => c.TerritoryId!.Value).Select(g => new CustomerByTerritory { TerritoryId = g.Key, Count = g.Count() });
}
```

## Saving

- **Entities:** `AddAsync`, `UpdateAsync`, `DeleteAsync`, and their `...RangeAsync` versions.
- **Bulk operations** are set-based and run in the database without loading entities:
  - `DeleteFromQueryAsync` returns the number of deleted rows.
  - `UpdateFromQueryAsync` also sets the modified audit fields.
  - `InsertFromQueryAsync` copies rows from one entity type into another. It quotes names with backticks, so it works with MySQL and SQLite but not SQL Server, and its selector can't introduce joins or subqueries.
  - `MergeFromQueryAsync` inserts, updates, and optionally deletes rows, and returns a `MergeResult`.

## Audit fields and events

- **Audit fields:** on add, `CreatedDate` and `CreatedBy` are set, and on add and update, `ModifiedDate` and `ModifiedBy` are set. This applies to any entity implementing the `Temelie.Entities` audit interfaces. The user name comes from `GetCreatedModifiedBy()`.
- **Event providers:** an `IRepositoryEventProvider<T>` can filter queries (`OnQueryAsync`) and react before and after add, update, and delete:

  ```csharp
  [ExportProvider(typeof(IRepositoryEventProvider<Person>))]
  public class PersonEvents : IRepositoryEventProvider<Person> { ... }
  ```

## Provider-specific model configuration

`IModelBuilderExtensions` (`UseIdentityColumn`, `HasTrigger`) isolates database-specific model configuration. To customize it for your EF Core provider, export your own implementation with a lower `Priority`.
