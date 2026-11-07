# Temelie.Database.App

Windows desktop app (WPF, `net10.0-windows`) for working with databases using Temelie. It hosts the [Temelie.Database.UI](https://github.com/michaelpaulus/temelie/blob/main/src/Temelie.Database.UI/README.md) screens with the SQL Server and MySQL providers:

- Script a database to source control, and deploy script folders.
- Copy table data between databases and providers.
- Build table-mapping `INSERT ... SELECT` scripts.

Run it from the repository:

```shell
dotnet run --project src/Temelie.Database.App
```

The app is wired with [Temelie.DependencyInjection.SourceGenerator](https://github.com/michaelpaulus/temelie/blob/main/src/Temelie.DependencyInjection.SourceGenerator/README.md), so referenced providers and services register automatically.
