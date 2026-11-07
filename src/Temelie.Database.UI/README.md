# Temelie.Database.UI

WPF screens and view models for the Temelie database tools (`net10.0-windows`). [Temelie.Database.App](https://github.com/michaelpaulus/temelie/blob/main/src/Temelie.Database.App/README.md) hosts them.

| Screen | Purpose |
| --- | --- |
| **Convert** | Copy selected tables from a source database to a target, optionally across providers. You can set thread count, batch size, transactions, and string trimming. |
| **Create Scripts** | Script a database to a folder ([`IScriptService.CreateScripts`](https://github.com/michaelpaulus/temelie/blob/main/src/Temelie.Database.Services/README.md#scripting-a-database)), optionally filtered by object name. |
| **Execute Scripts** | Deploy a script folder to a database ([`IScriptService.ExecuteScripts`](https://github.com/michaelpaulus/temelie/blob/main/src/Temelie.Database.Services/README.md#deploying-scripts)). |
| **Table Mapping Script** | Map columns between two tables and generate an `INSERT ... SELECT` script or a mapping XML file. |

Connections are managed in a dialog and saved to `%APPDATA%\Temelie.Database\Temelie.Database.json`.
