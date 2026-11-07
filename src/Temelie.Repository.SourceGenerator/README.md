# Temelie.Repository.SourceGenerator

Roslyn source generator that builds the EF Core model and primary-key queries for every `IEntity<T>` in your project and its references. Use it with [Temelie.Repository](https://github.com/michaelpaulus/temelie/blob/main/src/Temelie.Repository/README.md).

```shell
dotnet add package Temelie.Repository.SourceGenerator
```

Reference it from the project that holds your `DbContext`. Generated types use that project's `RootNamespace`.

## What is generated

- **`DbContextBase`**: an abstract `DbContext` with a `DbSet<T>` for every entity. Its `OnModelCreating` applies every registered `IModelBuilderProvider`.
- **One `IModelBuilderProvider` per entity** (exported automatically). Each one configures:
  - the table or view and schema
  - keys, including composite keys
  - column names and order
  - identity and computed columns
  - `[ColumnPrecision]` precision and scale
  - `[HasTrigger]`
- **`<Entity>SingleQuery`** for every table with a key: an `IQuerySpec<T>` whose constructor takes the key values.

  ```csharp
  var person = await repository.GetSingleAsync(new PersonSingleQuery(businessEntityId));
  ```

The generator reads standard data annotations on the entities: `[Table]`, `[Key]`, `[Column]`, and `[DatabaseGenerated]`. These are exactly what [Temelie.Entities.SourceGenerator](https://github.com/michaelpaulus/temelie/blob/main/src/Temelie.Entities.SourceGenerator/README.md) produces, but hand-written entities work too.
