# Temelie.Database.Providers.Mssql

SQL Server provider for [Temelie.Database.Services](https://github.com/michaelpaulus/temelie/blob/main/src/Temelie.Database.Services/README.md). It covers schema discovery, script generation, bulk copy, and change capture.

```shell
dotnet add package Temelie.Database.Providers.Mssql
```

Use `DatabaseProviderName = "SqlConnection"` (`DatabaseProvider.ProviderName`) on a `ConnectionStringModel`.

## What's included

All of these are registered automatically through `[Export...]` attributes:

| Type | Purpose |
| --- | --- |
| `DatabaseProvider` | `IDatabaseProvider` for SQL Server: schema queries, `CREATE`/`DROP` scripts for every object type, renames, column scripts, and `SqlBulkCopy`. |
| `DefaultAzureCredentialConnectionCreatedNotification` | When a connection string has no integrated security and no user name/password, it signs in with `DefaultAzureCredential` (Microsoft Entra ID) and sets the access token. |
| `MssqlChangeTrackingSourceProvider` | Change source `mssql_ct`, based on SQL Server Change Tracking. |
| `MssqlChangeDataCaptureSourceProvider` | Change source `mssql_cdc`, based on SQL Server Change Data Capture. |
| `MssqlChangeTrackingTargetProvider` | Applies synced changes to a SQL Server target and stores mappings and synced versions. |

Generated scripts are idempotent (`IF NOT EXISTS` / `DROP ... IF EXISTS`) and separate batches with `GO`.
