# Temelie

[![build](https://github.com/michaelpaulus/temelie/actions/workflows/build.yml/badge.svg)](https://github.com/michaelpaulus/temelie/actions/workflows/build.yml)

Temelie (Romanian for *foundation*) is a set of .NET 10 libraries, source generators, and tools for database-first applications:

1. **Script your database.** Generate a folder of `.sql` scripts and `.sql.json` models from a live SQL Server or MySQL database.
2. **Deploy those scripts.** Apply the scripts to any environment. Changed objects are recreated, tables are evolved through tracked migrations, and one-time and always-run scripts are supported.
3. **Generate code from the scripts.** Source generators turn the `.sql.json` models into entity records, EF Core model configuration, and primary-key queries.
4. **Use a lightweight repository.** A query-specification repository on EF Core with lifecycle events, audit fields, and bulk operations.
5. **Wire it up with attributes.** A source generator builds dependency-injection registrations from `[Export...]` attributes, with no reflection at runtime.

Each layer can be used on its own.

## Why database first

With Temelie, the database is the source of truth for the schema, and the code is generated from it:

- **The schema is reviewable.** It lives in source control as scripts, alongside the code that depends on it.
- **The compiler checks database changes.** Entities, EF Core mappings, and primary-key queries are regenerated from the scripts on every build. When a table or column is renamed, removed, or changes type or nullability, the compiler reports every place that depends on it. You find the breakage at build time, not at runtime.
- **Refactoring is safer.** Database changes can be made with confidence because the build verifies the code against the new schema.
- **Code and schema can't drift apart.** The generated code is never hand-edited. Customizations go in `partial` types and extended properties, so they survive regeneration.

## Packages

| Package | Description |
| --- | --- |
| [Temelie.Database.Models](src/Temelie.Database.Models/README.md) | Database object models (tables, columns, indexes, views, ...) and their JSON format. |
| [Temelie.Database.Services](src/Temelie.Database.Services/README.md) | Read database schemas, generate and execute scripts, copy data between databases, and sync changes. |
| [Temelie.Database.Providers.Mssql](src/Temelie.Database.Providers.Mssql/README.md) | SQL Server provider, including Azure AD authentication, Change Tracking, and Change Data Capture. |
| [Temelie.Database.Providers.MySql](src/Temelie.Database.Providers.MySql/README.md) | MySQL provider. |
| [Temelie.Database.UI](src/Temelie.Database.UI/README.md) | WPF screens for the database tools. |
| [Temelie.DependencyInjection](src/Temelie.DependencyInjection/README.md) | `[Export...]` attributes and startup configuration contracts. |
| [Temelie.DependencyInjection.SourceGenerator](src/Temelie.DependencyInjection.SourceGenerator/README.md) | Generates service registrations and the startup pipeline from the export attributes. |
| [Temelie.Entities](src/Temelie.Entities/README.md) | Entity base types, audit interfaces, and mapping attributes. |
| [Temelie.Entities.SourceGenerator](src/Temelie.Entities.SourceGenerator/README.md) | Generates entity records from `.sql.json` models. |
| [Temelie.Entities.SourceGeneratorBase](src/Temelie.Entities.SourceGeneratorBase/README.md) | Base for building a customized entity generator. |
| [Temelie.Repository](src/Temelie.Repository/README.md) | EF Core repository with query specifications, events, audit fields, and bulk operations. |
| [Temelie.Repository.SourceGenerator](src/Temelie.Repository.SourceGenerator/README.md) | Generates the EF Core `DbContextBase` model and primary-key query specifications. |
| [Temelie.SourceGenerator](src/Temelie.SourceGenerator/README.md) | Runs the generators outside the compiler and writes the generated files to disk. |

All packages target `net10.0` (the UI targets `net10.0-windows`). The Roslyn source generators target `netstandard2.0`, so they load in Visual Studio as well as the .NET SDK.

## Tools

- [Temelie.Database.App](src/Temelie.Database.App/README.md) is a Windows desktop app for creating and executing scripts, copying tables between databases, and building table-mapping scripts.

## How the pieces fit together

```text
 live database ──CreateScripts──▶ MyApp.Database/            (.sql + .sql.json per object)
                                      │        │
           ExecuteScripts ◀───────────┘        │ AdditionalFiles
   (deploy to any environment)                 ▼
                                 Temelie.Entities.SourceGenerator ─▶ entity records
                                                                        │
                                 Temelie.Repository.SourceGenerator ─▶ DbContextBase + *SingleQuery
                                                                        │
                                 Temelie.Repository ─────────────────▶ your repository
                                                                        │
                                 Temelie.DependencyInjection(.SourceGenerator) ─▶ service registrations
```

## Samples

The `AdventureWorks.*` projects show the whole flow:

| Project | Shows |
| --- | --- |
| `src/AdventureWorks.Database` | Generated scripts and a custom `IDatabaseModelProvider` that formats SQL definitions. |
| `src/AdventureWorks.Entities` | Entities generated from the database's `.sql.json` files, plus hand-written partials. |
| `src/AdventureWorks.Server.Repository` | A `DbContext` built on the generated `DbContextBase`, a repository, query specifications, and an event provider. |
| `src/Temelie.Repository.UnitTests` | End-to-end repository tests against SQLite in memory. |
| `src/Temelie.Database.UnitTests` | How script generation and deployment behave, run against a fake database. |
| `src/Temelie.Database.IntegrationTests` | Script generation and deployment checked against real database script folders (opt-in). |

## Building

Requirements: the .NET 10 SDK (see `global.json`).

```shell
dotnet build Temelie.sln
dotnet test Temelie.sln
```

Repository conventions:

- **Central package management.** All package versions live in `Directory.Packages.props`. The source generators intentionally stay on an older `Microsoft.CodeAnalysis` so they load in older compilers.
- **Shared build settings.** `Directory.Build.props` enables nullable reference types and treats warnings as errors. NuGet vulnerability advisories (NU1901–NU1904) stay warnings.
- **CI publishing.** Every push to `main` builds, tests, and publishes packages to nuget.org using Trusted Publishing.

## Background

The database projects here have been in use for over 15 years and have been refactored to separate the layers and to be DI-friendly. The rest grew out of a 2024 job change: rather than rebuild lost automation from scratch again, it is built here once and published to nuget.org for use in any project.

## License

[MIT](LICENSE)