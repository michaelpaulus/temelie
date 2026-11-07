# Temelie.Database.Providers.MySql

MySQL provider for [Temelie.Database.Services](https://github.com/michaelpaulus/temelie/blob/main/src/Temelie.Database.Services/README.md), built on [MySqlConnector](https://mysqlconnector.net/).

```shell
dotnet add package Temelie.Database.Providers.MySql
```

Use `DatabaseProviderName = "MySqlConnection"` (`DatabaseProvider.ProviderName`) on a `ConnectionStringModel`.

## Supported objects

| Object | Read | Scripted |
| --- | --- | --- |
| Tables (including engine, character set, collation, and generated columns) | ✓ | ✓ (primary keys inline) |
| Indexes and foreign keys | ✓ | ✓ |
| Views and stored procedures | ✓ | ✓ |
| Triggers | ✓ | ✓ |
| Check constraints, security policies | – | – |

MySQL has no schemas, so file and object names have no schema prefix (for example `02_Tables/customers.sql`).
