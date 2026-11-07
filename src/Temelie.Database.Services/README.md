# Temelie.Database.Services

Database-first tooling for reading schemas, scripting databases to source control, deploying those scripts, copying data between databases, and syncing changes.

```shell
dotnet add package Temelie.Database.Services
dotnet add package Temelie.Database.Providers.Mssql   # and/or Temelie.Database.Providers.MySql
```

Services are exported with `[Export...]` attributes. Reference [Temelie.DependencyInjection.SourceGenerator](https://github.com/michaelpaulus/temelie/blob/main/src/Temelie.DependencyInjection.SourceGenerator/README.md) in your application to register them, along with the providers you reference.

## Services

| Service | Purpose |
| --- | --- |
| `IScriptService` | `CreateScripts` scripts a database to a folder; `ExecuteScripts` deploys a folder to a database. |
| `IDatabaseModelService` | Reads a live database into a `DatabaseModel`, with optional `DatabaseModelOptions` filtering. |
| `IDatabaseExecutionService` | Creates connections and runs commands; `ExecuteFile` runs scripts split on `GO`. |
| `ITableConverterService` | Copies table data between databases or providers using bulk copy, in parallel. |
| `IChangeTrackingService` | Syncs incremental changes from a source database (SQL Server Change Tracking or CDC) to a target. |
| `IDatabaseFactory` | Resolves the `IDatabaseProvider` for a provider name or connection. |

## Extension points

| Interface | Use |
| --- | --- |
| `IDatabaseProvider` / `DatabaseProviderBase` | Add a database engine. |
| `IDatabaseModelProvider` | Adjust models as they are read, for example to format SQL or rename objects (all `Initialize` methods are optional). |
| `IConnectionCreatedNotification` | Change connections before they open, for example to set an access token. |
| `ITableConverterReaderColumnValueProvider` | Transform values while copying tables. |

## Scripting a database

```csharp
var connection = new ConnectionStringModel
{
    Name = "Dev",
    DatabaseProviderName = "SqlConnection",
    ConnectionString = "Data Source=.;Initial Catalog=MyApp;Integrated Security=True;Encrypt=False"
};

scriptService.CreateScripts(connection, new DirectoryInfo("src/MyApp.Database"), progress => Console.WriteLine(progress));
```

Every object is written as a `.sql` script plus a `.sql.json` model:

| Folder | Contents |
| --- | --- |
| `02_Tables` | Tables |
| `03_Indexes` | Primary keys and indexes |
| `04_CheckConstraints` | Check constraints |
| `05_Programmability` | Procedures and functions |
| `05_Views` | Views |
| `06_Triggers` | Triggers |
| `08_ForeignKeys` | Foreign keys |
| `09_SecurityPolicies` | Row-level security policies |

Generation details:

- **File names** are `schema.name.sql`.
- **Unchanged files** are left untouched, and files for objects that no longer exist are deleted.
- **Table changes** only update the table's script and model. Write the migration that changes an existing table yourself, in a `*_Migrations/<name>/` folder.

## Deploying scripts

```csharp
scriptService.ExecuteScripts(connection, new DirectoryInfo("src/MyApp.Database"), progress => Console.WriteLine(progress), continueOnError: true);
```

Top-level folders run in name order, which lets you interleave your own folders with the generated ones:

| Folder kind | Behavior |
| --- | --- |
| Object folders (contain `.sql.json`) | Compared with the live database; only differences are applied. Missing objects are created, changed objects are dropped and recreated (with everything that depends on them, in dependency order), and removed objects are dropped. Tables themselves are never recreated: change them with migrations. See [Dependency-aware deployment](#dependency-aware-deployment). |
| `*_Migrations/<name>/*.sql` | Each file runs once, in order, and is recorded in a `Migrations` table (created automatically) with the id `<folder>/<name>/<file>`. With `skipMigrations: true` they are recorded without running, for example to baseline an existing database. |
| `*_Always/*.sql` | Run on every deployment. |
| Other folders with only `.sql` files | Each file runs on every deployment. |

More deployment details:

- **Removed tables** are renamed to `__<Table>` instead of being dropped, so no data is lost (only when the folder has a `02_Tables` folder).
- **Schema refresh:** the live schema is read again after renames and after each migration folder, so later folders are compared with what the migrations produced.
- **Failures:** with `continueOnError`, failed non-migration scripts are collected, and the whole run is retried up to 5 times. A failed migration ends the current attempt immediately. Without `continueOnError`, the first failure stops the deployment.
- **Batches:** scripts are split into batches on `GO` lines.

A typical project layout:

```text
MyApp.Database/
  01_Always/              helper procedures used by migrations
  02_Migrations/<date>/   data and schema changes that must run before objects are applied
  02_Tables/ 03_Indexes/ 04_CheckConstraints/
  04_Migrations/<date>/   table changes
  05_Programmability/ 05_Views/ 06_Triggers/
  07_Migrations/<date>/   data fixes that need the new views/procedures
  08_ForeignKeys/ 09_SecurityPolicies/
  09_Always/              post-deployment scripts
```

### Dependency-aware deployment

Dependencies are inferred from the SQL in the `.sql.json` models (SQL Server and MySQL); nothing extra has to be generated.

- **Only what is affected is recreated.** A changed or removed object recreates its direct and transitive dependents, nothing else.
- **Table changes refresh only what can go stale.** When a table differs, or a migration runs `ALTER TABLE` (or similar) on it, the views, procedures, functions, and triggers that use it are refreshed. Indexes, check constraints, foreign keys, and security policies are only recreated when their own definition changes.
- **Schema-bound objects and indexed views are left to migrations.** Existing schema-bound views, functions, and security policies, and indexed views with their indexes, are never dropped or recreated, even when their own definition changed; a `Migration Required: <object>` progress message reports each such change. Missing ones are still created (for example on a new database). Nothing propagates into them either: if something they use is recreated, the database refuses the drop because of the schema binding, and that change has to be made in a migration too.
- **Folders keep their name order.** At each folder, its affected objects are dropped (dependents first) and created (dependencies first) before the folder's own scripts. An object moves to a later folder only when it must, for example a procedure that uses a view runs with `05_Views`. New tables stay in `02_Tables`; anything they need, such as a function used in a default, is created just before them.
- **Re-planning after migrations.** After each migration folder the schema is read again and the remaining folders are re-planned, so objects a migration already created or changed are not applied twice. Folders that already ran are not revisited.
- **Cycles** among scheduled objects fail with the dependency path.

Inference recognizes static table references, function and procedure calls, quoted identifiers, and local database qualification, and ignores comments, strings, temporary tables, and CTE names. Unqualified SQL Server names resolve against the object's schema, then `dbo`; MySQL names resolve within the current database. Dynamic SQL, synonyms, and other databases cannot be inferred; changes that depend on them need migrations.

Generated `.sql.json` files can also feed [Temelie.Entities.SourceGenerator](https://github.com/michaelpaulus/temelie/blob/main/src/Temelie.Entities.SourceGenerator/README.md).

## Copying data

`ITableConverterService.ConvertTables` copies the selected tables from a source connection to a target connection. It matches columns by name, can trim strings, and uses provider bulk copy in batches, optionally inside a transaction.

## Change sync

`IChangeTrackingService` reads changes from a source `IChangeTrackingSourceProvider` (`mssql_ct` or `mssql_cdc`) and applies them through an `IChangeTrackingTargetProvider`. It keeps the last synced version per mapping, so each sync only moves new changes.
