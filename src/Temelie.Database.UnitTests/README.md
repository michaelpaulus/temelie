# Temelie.Database.UnitTests

NUnit tests for [Temelie.Database.Services](https://github.com/michaelpaulus/temelie/blob/main/src/Temelie.Database.Services/README.md) script generation (`CreateScripts`), deployment (`ExecuteScripts`), and `GO` batch splitting. They run against an in-memory fake database with the real SQL Server and MySQL script providers.

```shell
dotnet test src/Temelie.Database.UnitTests
```

Tests against real database script folders are in [Temelie.Database.IntegrationTests](https://github.com/michaelpaulus/temelie/blob/main/src/Temelie.Database.IntegrationTests/README.md). Both projects share the fakes and helpers in [Temelie.Database.Testing](https://github.com/michaelpaulus/temelie/blob/main/src/Temelie.Database.Testing/README.md).
