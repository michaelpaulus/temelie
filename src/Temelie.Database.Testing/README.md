# Temelie.Database.Testing

Shared test infrastructure for [Temelie.Database.UnitTests](https://github.com/michaelpaulus/temelie/blob/main/src/Temelie.Database.UnitTests/README.md) and [Temelie.Database.IntegrationTests](https://github.com/michaelpaulus/temelie/blob/main/src/Temelie.Database.IntegrationTests/README.md). It is not packaged.

| Type | Purpose |
| --- | --- |
| `Fakes.FakeDatabase` | In-memory `IDatabaseModelService` and `IDatabaseExecutionService`. It records executed scripts, tracks the `Migrations` table, and can change its schema or fail when specific scripts run. |
| `Fakes.DatabaseState` | The fake database's schema. |
| `Fakes.FakeDbConnection` | Minimal ADO.NET connection whose commands are answered by delegates. |
| `ScriptTestContext` / `ScriptRun` | Runs `CreateScripts` and `ExecuteScripts` with the real SQL Server or MySQL provider and captures the steps. |
| `ScriptFolder` | A temporary script folder that is deleted on dispose. |
| `Db` | Builders for tables, columns, views, procedures, indexes, constraints, triggers, and foreign keys. |
