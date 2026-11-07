# Temelie.Database.IntegrationTests

Integration tests that run [Temelie.Database.Services](https://github.com/michaelpaulus/temelie/blob/main/src/Temelie.Database.Services/README.md) script generation and deployment against real database script folders.

Point the tests at one or more existing database projects with `TEMELIE_REAL_WORLD_DATABASES`, a `;`-separated list of `provider=path` entries:

```powershell
$env:TEMELIE_REAL_WORLD_DATABASES = 'SqlConnection=C:\src\app\App.Database;MySqlConnection=C:\src\other\Other.Database'
dotnet test src/Temelie.Database.IntegrationTests
```

For each folder, the tests verify that:

- **Regeneration reproduces the scripts.** Regenerating from the committed `.sql.json` models reproduces the committed scripts.
- **Up-to-date deployments do nothing extra.** Deploying to an up-to-date database runs only the plain script folders and changes no objects.
- **Fresh deployments run every migration.** A fresh deployment runs every migration once, in folder order.

Without the variable, the tests are skipped.
