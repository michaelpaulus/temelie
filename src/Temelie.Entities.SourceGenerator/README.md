# Temelie.Entities.SourceGenerator

Roslyn source generator that creates entity records from the `.sql.json` files produced by [Temelie.Database.Services](https://github.com/michaelpaulus/temelie/blob/main/src/Temelie.Database.Services/README.md) `CreateScripts`.

Entities are regenerated from the database scripts on every build. When a database change renames or removes a column, or changes its type or nullability, every piece of code affected by that change becomes a compiler error or warning, which makes database refactoring safer.

```shell
dotnet add package Temelie.Entities
dotnet add package Temelie.Entities.SourceGenerator
```

## Setup

Add the database project's table and view models as `AdditionalFiles`:

```xml
<ItemGroup>
  <AdditionalFiles Include="..\MyApp.Database\02_Tables\**\*.sql.json" />
  <AdditionalFiles Include="..\MyApp.Database\05_Views\**\*.sql.json" />
</ItemGroup>
```

Generated types use the project's `RootNamespace`.

## What is generated

For every table and view:

- **`I<ClassName>`**: a partial interface that extends `IEntity<ClassName>` and `IProjectEntity`. Columns named `CreatedDate`, `CreatedBy`, `ModifiedDate`, or `ModifiedBy` are exposed through the matching `Temelie.Entities` audit interface.
- **`<ClassName>`**: a partial record deriving from `EntityBase`, with `[Table]`, `[Key]`, `[Column]`, `[DatabaseGenerated]`, `[ColumnPrecision]`, `[MaxLength]`, and `[HasTrigger]` attributes, and nullable reference types that match the column nullability.

The generator also emits an empty `IProjectEntity` marker interface for the project.

## Naming

Class and property names come from the database names, normalized for C# (for example, `ID` becomes `Id`). Override them with database extended properties:

| Extended property | Effect |
| --- | --- |
| `name` / `dynamicName` | Class or property name to use. |
| `isIgnored` / `dynamicIsIgnored` = `true` | Leave the column out of the entity. |

## Customizing

Entities are `partial`. Add members in your own file, or implement a generated property yourself by declaring it `partial`:

```csharp
public partial record Address
{
    public virtual partial string AddressLine1 { get; set; }
}
```

The generator then emits only the implementation for that property. To change the generated code itself, build your own generator with [Temelie.Entities.SourceGeneratorBase](https://github.com/michaelpaulus/temelie/blob/main/src/Temelie.Entities.SourceGeneratorBase/README.md).

To see or check in the generated files, write them to disk with [Temelie.SourceGenerator](https://github.com/michaelpaulus/temelie/blob/main/src/Temelie.SourceGenerator/README.md).
