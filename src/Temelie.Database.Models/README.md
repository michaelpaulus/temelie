# Temelie.Database.Models

Plain models for database objects. [Temelie.Database.Services](https://github.com/michaelpaulus/temelie/blob/main/src/Temelie.Database.Services/README.md) produces them, they are stored as `.sql.json` files, and the source generators read them back.

```shell
dotnet add package Temelie.Database.Models
```

## Models

| Type | Describes |
| --- | --- |
| `DatabaseModel` | A whole database: tables, views, indexes, primary keys, triggers, foreign keys, check constraints, definitions, and security policies. |
| `TableModel` / `ColumnModel` | Tables, views (`IsView`), and their columns, including type, nullability, identity, computed definition, defaults, collation, and extended properties. |
| `IndexModel` / `IndexColumnModel` | Indexes and primary keys (`IsPrimaryKey`). |
| `ForeignKeyModel`, `CheckConstraintModel`, `TriggerModel`, `SecurityPolicyModel` | Other table-level objects. |
| `DefinitionModel` | Views, procedures, and functions (`XType`/`Type`); views also carry their `View` table model. |
| `ConnectionStringModel` | A named connection: `DatabaseProviderName` (for example `SqlConnection` or `MySqlConnection`) plus the connection string. |
| `TableMapping` / `ColumnMappingModel` | Source-to-target column mappings used by the table-mapping scripts. |

## JSON

`ModelsJsonSerializerOptions.Default` is the format used for `.sql.json` files. It uses camelCase, indented output, omits null values, and relies on a source-generated serializer context.

Load a model from generated script files, either from disk or from embedded resources:

```csharp
var model = DatabaseModel.CreateFromFiles(
    Directory.GetFiles(scriptsPath, "*.sql.json", SearchOption.AllDirectories)
        .Select(file => (file, File.ReadAllText(file))));

var embedded = DatabaseModel.CreateFromAssembly(typeof(SomeTypeInTheDatabaseProject).Assembly);
```

The folder each file lives in (`02_Tables`, `03_Indexes`, `05_Views`, ...) determines which model type it is.

## Naming from extended properties

`TableModel.ClassName` and `ColumnModel.PropertyName` default to a normalized form of the database name. A `name` or `dynamicName` extended property overrides it, and `isIgnored`/`dynamicIsIgnored` excludes a column from generated entities.
