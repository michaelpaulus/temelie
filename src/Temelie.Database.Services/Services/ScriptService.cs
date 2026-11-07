using System.Text;
using System.Text.Json;
using Temelie.Database.Extensions;
using Temelie.Database.Models;
using Temelie.Database.Providers;
using Temelie.DependencyInjection;

namespace Temelie.Database.Services;

[ExportTransient(typeof(IScriptService))]
public class ScriptService : IScriptService
{
    private readonly IDatabaseFactory _databaseFactory;
    private readonly IDatabaseExecutionService _databaseExecutionService;
    private readonly IDatabaseModelService _databaseModelService;

    private static readonly IEnumerable<string> _directoryList = new List<string>()
                {
                    "02_Tables",
                    "03_Indexes",
                    "04_CheckConstraints",
                    "05_Views",
                    "05_Programmability",
                    "06_Triggers",
                    "08_ForeignKeys",
                    "09_SecurityPolicies"
                };

    public ScriptService(IDatabaseFactory databaseFactory,
        IDatabaseExecutionService databaseExecutionService,
        IDatabaseModelService databaseModelService)
    {
        _databaseFactory = databaseFactory;
        _databaseExecutionService = databaseExecutionService;
        _databaseModelService = databaseModelService;
    }

    private (FileInfo File, bool Changed, bool New) WriteIfDifferent(string path, string contents)
    {
        var parentDirectory = Path.GetDirectoryName(path) ?? throw new ArgumentException($"'{path}' has no parent directory.", nameof(path));
        if (!Directory.Exists(parentDirectory))
        {
            Directory.CreateDirectory(parentDirectory);
        }

        contents = contents.NormalizeLineEndings("\n").Trim('\n').NormalizeLineEndings(Environment.NewLine);

        var currentContents = "";
        bool changed = false;
        bool isNew = false;
        if (File.Exists(path))
        {
            currentContents = File.ReadAllText(path).NormalizeLineEndings("\n").Trim('\n').NormalizeLineEndings(Environment.NewLine);
        }
        if (currentContents != contents)
        {
            isNew = !File.Exists(path);
            changed = true;
            File.WriteAllText(path, contents, System.Text.Encoding.UTF8);
        }
        return (new FileInfo(path), changed, isNew);
    }

    private bool IsView(IndexModel index, Models.DatabaseModel databaseModel)
    {
        return databaseModel.Views.Any(i => i.SchemaName == index.SchemaName && i.TableName == index.TableName);
    }

    private IEnumerable<FileInfo> CreateTableScripts(IDatabaseProvider provider, Models.DatabaseModel database, System.IO.DirectoryInfo directory, string fileFilter = "")
    {
        var files = new List<(FileInfo File, bool Changed, bool New)>();

        foreach (var table in database.Tables)
        {
            var fileName = MakeValidFileName($"{table.SchemaName}.{table.TableName}.sql");
            if (string.IsNullOrEmpty(fileFilter) ||
                fileFilter.EqualsIgnoreCase(fileName))
            {
                var script = provider.GetScript(table);
                if (script is not null)
                {
                    files.Add(WriteIfDifferent(System.IO.Path.Combine(directory.FullName, fileName), script.CreateScript));

                    var jsonPath = System.IO.Path.Combine(directory.FullName, fileName + ".json");
                    var json = GetJson(table);

                    files.Add(WriteIfDifferent(jsonPath, json));
                }
            }
        }

        return files.Select(i => i.File).ToList();
    }
    private IEnumerable<FileInfo> CreateSecurityPolicyScripts(IDatabaseProvider provider, Models.DatabaseModel database, System.IO.DirectoryInfo directory, string fileFilter = "")
    {
        var files = new List<FileInfo>();

        foreach (var securityPolicy in database.SecurityPolicies)
        {
            var fileName = MakeValidFileName($"{securityPolicy.PolicySchema}.{securityPolicy.PolicyName}.sql");
            if (string.IsNullOrEmpty(fileFilter) ||
                fileFilter.EqualsIgnoreCase(fileName))
            {

                var script = provider.GetScript(securityPolicy);
                if (script is not null)
                {
                    var sbSecurityPolicyScript = new StringBuilder();
                    sbSecurityPolicyScript.AppendLine(script.CreateScript);

                    files.Add(WriteIfDifferent(System.IO.Path.Combine(directory.FullName, fileName), sbSecurityPolicyScript.ToString()).File);
                    files.Add(WriteIfDifferent(System.IO.Path.Combine(directory.FullName, fileName + ".json"), GetJson(securityPolicy)).File);
                }

            }

        }

        return files;
    }
    private IEnumerable<FileInfo> CreateIndexScripts(IDatabaseProvider provider, Models.DatabaseModel database, System.IO.DirectoryInfo directory, string fileFilter = "")
    {
        var files = new Dictionary<string, FileInfo>();

        foreach (var pk in database.PrimaryKeys)
        {
            var fileName = MakeValidFileName($"{pk.SchemaName}.{pk.IndexName}.sql");

            if (files.ContainsKey(fileName))
            {
                fileName = MakeValidFileName($"{pk.SchemaName}.{pk.TableName}.{pk.IndexName}.sql");
            }

            if (!files.ContainsKey(fileName))
            {
                if (string.IsNullOrEmpty(fileFilter) ||
                fileFilter.EqualsIgnoreCase(fileName))
                {
                    var script = provider.GetScript(pk, IsView(pk, database));
                    if (script is not null)
                    {
                        files.Add(fileName, WriteIfDifferent(System.IO.Path.Combine(directory.FullName, fileName), script.CreateScript).File);
                        files.Add(fileName + ".json", WriteIfDifferent(System.IO.Path.Combine(directory.FullName, fileName + ".json"), GetJson(pk)).File);
                    }
                }
            }
        }

        foreach (var index in database.Indexes)
        {
            var fileName = MakeValidFileName($"{index.SchemaName}.{index.IndexName}.sql");

            if (files.ContainsKey(fileName))
            {
                fileName = MakeValidFileName($"{index.SchemaName}.{index.TableName}.{index.IndexName}.sql");
            }

            if (!files.ContainsKey(fileName))
            {
                if (string.IsNullOrEmpty(fileFilter) ||
                fileFilter.EqualsIgnoreCase(fileName))
                {
                    var script = provider.GetScript(index, IsView(index, database));
                    if (script is not null)
                    {
                        files.Add(fileName, WriteIfDifferent(System.IO.Path.Combine(directory.FullName, fileName), script.CreateScript).File);
                        files.Add(fileName + ".json", WriteIfDifferent(System.IO.Path.Combine(directory.FullName, fileName + ".json"), GetJson(index)).File);
                    }
                }
            }
        }

        return files.Values.ToArray();
    }
    private IEnumerable<FileInfo> CreateCheckConstraintScripts(IDatabaseProvider provider, Models.DatabaseModel database, System.IO.DirectoryInfo directory, string fileFilter = "")
    {
        var files = new List<FileInfo>();

        foreach (var constraint in database.CheckConstraints)
        {
            var fileName = MakeValidFileName($"{constraint.SchemaName}.{constraint.CheckConstraintName}.sql");
            if (string.IsNullOrEmpty(fileFilter) ||
                fileFilter.EqualsIgnoreCase(fileName))
            {
                var script = provider.GetScript(constraint);
                if (script is not null)
                {
                    var sb = new StringBuilder();
                    sb.Append(script.CreateScript);

                    files.Add(WriteIfDifferent(System.IO.Path.Combine(directory.FullName, fileName), sb.ToString()).File);
                    files.Add(WriteIfDifferent(System.IO.Path.Combine(directory.FullName, fileName + ".json"), GetJson(constraint)).File);
                }
            }
        }

        return files;
    }
    private IEnumerable<FileInfo> CreateProgrammabilityScripts(IDatabaseProvider provider, Models.DatabaseModel database, System.IO.DirectoryInfo directory, string fileFilter = "")
    {
        var files = new List<FileInfo>();

        foreach (var definition in database.Definitions.Where(i => i.View is null))
        {
            var fileName = MakeValidFileName($"{definition.SchemaName}.{definition.DefinitionName}.sql");
            if (string.IsNullOrEmpty(fileFilter) ||
                fileFilter.EqualsIgnoreCase(fileName))
            {

                var script = provider.GetScript(definition);

                if (script is not null)
                {
                    var sb = new System.Text.StringBuilder();
                    sb.Append(script.CreateScript);

                    files.Add(WriteIfDifferent(System.IO.Path.Combine(directory.FullName, fileName), sb.ToString()).File);
                    files.Add(WriteIfDifferent(System.IO.Path.Combine(directory.FullName, fileName + ".json"), GetJson(definition)).File);
                }
            }

        }

        return files;
    }

    private IEnumerable<FileInfo> CreateViewScripts(IDatabaseProvider provider, Models.DatabaseModel database, System.IO.DirectoryInfo directory, string fileFilter = "")
    {
        var files = new List<FileInfo>();

        foreach (var definition in database.Definitions.Where(i => i.View is not null))
        {
            var fileName = MakeValidFileName($"{definition.SchemaName}.{definition.DefinitionName}.sql");
            if (string.IsNullOrEmpty(fileFilter) ||
                fileFilter.EqualsIgnoreCase(fileName))
            {

                var script = provider.GetScript(definition);

                if (script is not null)
                {
                    var sb = new System.Text.StringBuilder();
                    sb.Append(script.CreateScript);

                    files.Add(WriteIfDifferent(System.IO.Path.Combine(directory.FullName, fileName), sb.ToString()).File);
                    files.Add(WriteIfDifferent(System.IO.Path.Combine(directory.FullName, fileName + ".json"), GetJson(definition)).File);
                }
            }

        }

        return files;
    }

    private IEnumerable<FileInfo> CreateTriggerScripts(IDatabaseProvider provider, Models.DatabaseModel database, System.IO.DirectoryInfo directory, string fileFilter = "")
    {
        var files = new List<FileInfo>();
        foreach (var trigger in database.Triggers)
        {
            var fileName = MakeValidFileName($"{trigger.SchemaName}.{trigger.TriggerName}.sql");
            if (string.IsNullOrEmpty(fileFilter) ||
                fileFilter.EqualsIgnoreCase(fileName))
            {

                var script = provider.GetScript(trigger);

                if (script is not null)
                {
                    var sb = new System.Text.StringBuilder();

                    sb.Append(script.CreateScript);

                    files.Add(WriteIfDifferent(System.IO.Path.Combine(directory.FullName, fileName), sb.ToString()).File);
                    files.Add(WriteIfDifferent(System.IO.Path.Combine(directory.FullName, fileName + ".json"), GetJson(trigger)).File);
                }

            }
        }

        return files;
    }
    private IEnumerable<FileInfo> CreateFkScripts(IDatabaseProvider provider, Models.DatabaseModel database, System.IO.DirectoryInfo directory, string fileFilter = "")
    {
        var files = new List<FileInfo>();
        foreach (var foreignKey in database.ForeignKeys)
        {
            var fileName = MakeValidFileName($"{foreignKey.SchemaName}.{foreignKey.ForeignKeyName}.sql");
            if (string.IsNullOrEmpty(fileFilter) ||
                fileFilter.EqualsIgnoreCase(fileName))
            {
                var script = provider.GetScript(foreignKey);
                if (script is not null)
                {
                    var sb = new System.Text.StringBuilder();
                    sb.Append(script.CreateScript);
                    files.Add(WriteIfDifferent(System.IO.Path.Combine(directory.FullName, fileName), sb.ToString()).File);
                    files.Add(WriteIfDifferent(System.IO.Path.Combine(directory.FullName, fileName + ".json"), GetJson(foreignKey)).File);
                }
            }
        }

        return files;
    }

    private string GetJson<T>(T model) where T : DatabaseObjectModel
    {
        var json = JsonSerializer.Serialize(model, ModelsJsonSerializerOptions.Default);
        json = json.Replace("\\u0027", "'").Replace("\\u003C", "<").Replace("\\u003E", ">").Replace("\\u002B", "+").Trim(' ').Trim('\n').Trim('\r').Trim(' ').Trim('\n').Trim('\r');
        return json;
    }

    public void CreateScripts(ConnectionStringModel connectionString, System.IO.DirectoryInfo directory, Action<ScriptProgress>? progress, string objectFilter = "", IDatabaseProvider? createScriptsProvider = null)
    {
        var provider = _databaseFactory.GetDatabaseProvider(connectionString);
        Models.DatabaseModel database = _databaseModelService.CreateModel(connectionString, new Models.DatabaseModelOptions { ObjectFilter = objectFilter, ExcludeDoubleUnderscoreObjects = true });

        if (createScriptsProvider is not null)
        {
            provider = createScriptsProvider;
        }

        foreach (var di in _directoryList)
        {
            if (!Directory.Exists(Path.Combine(directory.FullName, di)))
            {
                Directory.CreateDirectory(Path.Combine(directory.FullName, di));
            }
            if (!Directory.Exists(Path.Combine(directory.FullName, "01_Drop", di)))
            {
                Directory.CreateDirectory(Path.Combine(directory.FullName, "01_Drop", di));
            }
        }

        int intIndex = 1;
        int intTotalCount = _directoryList.Count();

        int intProgress = 0;

        static void syncFiles(DirectoryInfo subDirectory, Dictionary<string, FileInfo> files)
        {
            foreach (var file in subDirectory.GetFiles("*.sql"))
            {
                if (!files.ContainsKey(file.Name))
                {
                    file.Delete();
                }
            }
            foreach (var file in subDirectory.GetFiles("*.sql.json"))
            {
                if (!files.ContainsKey(file.Name))
                {
                    file.Delete();
                }
            }
        }

        foreach (var subDirectoryName in _directoryList)
        {

            string subDirectoryPath = System.IO.Path.Combine(directory.FullName, subDirectoryName);

            var subDirectory = new System.IO.DirectoryInfo(subDirectoryPath);

            progress?.Invoke(new ScriptProgress() { ProgressPercentage = intProgress, ProgressStatus = subDirectory.Name });

            if (subDirectory.Name.StartsWith("02_Tables", StringComparison.InvariantCultureIgnoreCase))
            {
                var files = CreateTableScripts(provider, database, subDirectory).ToDictionary(i => i.Name, StringComparer.OrdinalIgnoreCase);
                syncFiles(subDirectory, files);
            }
            else if (subDirectory.Name.StartsWith("03_Indexes", StringComparison.InvariantCultureIgnoreCase))
            {
                var files = CreateIndexScripts(provider, database, subDirectory).ToDictionary(i => i.Name, StringComparer.OrdinalIgnoreCase);
                syncFiles(subDirectory, files);
            }
            else if (subDirectory.Name.StartsWith("04_CheckConstraints", StringComparison.InvariantCultureIgnoreCase))
            {
                var files = CreateCheckConstraintScripts(provider, database, subDirectory).ToDictionary(i => i.Name, StringComparer.OrdinalIgnoreCase);
                syncFiles(subDirectory, files);
            }
            else if (subDirectory.Name.StartsWith("05_Programmability", StringComparison.InvariantCultureIgnoreCase))
            {
                var files = CreateProgrammabilityScripts(provider, database, subDirectory).ToDictionary(i => i.Name, StringComparer.OrdinalIgnoreCase);
                syncFiles(subDirectory, files);
            }
            else if (subDirectory.Name.StartsWith("05_Views", StringComparison.InvariantCultureIgnoreCase))
            {
                var files = CreateViewScripts(provider, database, subDirectory).ToDictionary(i => i.Name, StringComparer.OrdinalIgnoreCase);
                syncFiles(subDirectory, files);
            }
            else if (subDirectory.Name.StartsWith("06_Triggers", StringComparison.InvariantCultureIgnoreCase))
            {
                var files = CreateTriggerScripts(provider, database, subDirectory).ToDictionary(i => i.Name, StringComparer.OrdinalIgnoreCase);
                syncFiles(subDirectory, files);
            }
            else if (subDirectory.Name.StartsWith("08_ForeignKeys", StringComparison.InvariantCultureIgnoreCase))
            {
                var files = CreateFkScripts(provider, database, subDirectory).ToDictionary(i => i.Name, StringComparer.OrdinalIgnoreCase);
                syncFiles(subDirectory, files);
            }
            else if (subDirectory.Name.StartsWith("09_SecurityPolicies", StringComparison.InvariantCultureIgnoreCase))
            {
                var files = CreateSecurityPolicyScripts(provider, database, subDirectory).ToDictionary(i => i.Name, StringComparer.OrdinalIgnoreCase);
                syncFiles(subDirectory, files);
            }

            intProgress = Convert.ToInt32((intIndex / (double)intTotalCount) * 100);

            intIndex += 1;

        }

        progress?.Invoke(new ScriptProgress() { ProgressPercentage = 100, ProgressStatus = "Complete" });

    }

    private string MakeValidFileName(string name)
    {
        if (name.StartsWith("."))
        {
            name = name.Substring(1);
        }

        string invalidChars = System.Text.RegularExpressions.Regex.Escape(new string(System.IO.Path.GetInvalidFileNameChars()));
        string invalidRegStr = string.Format(@"([{0}]*\.+$)|([{0}]+)", invalidChars);

        return System.Text.RegularExpressions.Regex.Replace(name, invalidRegStr, "_");
    }

    public void ExecuteScripts(ConnectionStringModel connectionString, DirectoryInfo directory, Action<ScriptProgress>? progress, bool continueOnError = true, bool skipMigrations = false)
    {
        IList<string> migrations = [];

        void addMigration(string id)
        {
            using var conn = _databaseExecutionService.CreateDbConnection(connectionString);
            using var cmd = _databaseExecutionService.CreateDbCommand(conn);
            var skipMigrationsBit = skipMigrations ? "1" : "0";
            cmd.CommandText = $"INSERT INTO Migrations (Id, Date, Skipped) VALUES {$"('{id}', '{DateTime.UtcNow:yyyy-MM-ddTHH:mm:ss}', {skipMigrationsBit})"}";
            cmd.ExecuteNonQuery();
            migrations.Add(id);
        }

        IList<string> getMigrations()
        {
            using var conn = _databaseExecutionService.CreateDbConnection(connectionString);
            using var cmd = _databaseExecutionService.CreateDbCommand(conn);
            cmd.CommandText = $"SELECT Id FROM Migrations";
            using var reader = cmd.ExecuteReader();
            var list = new List<string>() { "loaded" };
            while (reader.Read())
            {
                list.Add(reader.GetString(0));
            }
            return list;
        }

        bool checkMigration(string id)
        {
            if (migrations.Count == 0)
            {
                migrations = getMigrations();
            }
            return migrations.Contains(id);
        }

        IEnumerable<(string Name, string Contents)> getFiles(DirectoryInfo dir)
        {
            if (dir.Parent?.Name.EndsWith("_Migrations") == true)
            {

                var list = new List<(string Name, string Contents)>();
                var files = dir.GetFiles("*.sql").OrderBy(i => i.FullName);
                if (files.Any())
                {
                    foreach (var file in files)
                    {
                        var id = $"{dir.Parent.Name}/{dir.Name}/{file.Name}";
                        if (!checkMigration(id))
                        {
                            list.Add((id, File.ReadAllText(file.FullName)));
                        }
                    }
                }

                return list;
            }
            else
            {
                var list = new List<(string Name, string Contents)>();
                var files = dir.GetFiles("*.sql").OrderBy(i => i.FullName).ToList();
                if (files.Count > 0)
                {
                    var jsonFiles = dir.GetFiles("*.sql.json").OrderBy(i => i.FullName).ToList();

                    if (jsonFiles.Count == 0)
                    {
                        foreach (var file in files)
                        {
                            var id = $"{dir.Name}/{file.Name}";
                            if (dir.Name.EndsWith("_Always") || !checkMigration(id))
                            {
                                list.Add((id, File.ReadAllText(file.FullName)));
                            }
                        }
                    }

                }
                return list;
            }
        }

        void executeScriptsInernal()
        {
            var modelList = directory.GetDirectories().Where(i =>
                _directoryList.Any(i2 => i.Name.Contains(i2))
            ).SelectMany(i => i.GetFiles("*.sql.json", SearchOption.AllDirectories)).ToList();

            var shouldEnsureMigrationsTable = true;

            void ensureMigrationsTable(DatabaseModel currentDatabaseModel)
            {
                if (shouldEnsureMigrationsTable)
                {
                    var table = new TableModel()
                    {
                        SchemaName = "dbo",
                        TableName = "Migrations"
                    };
                    table.Columns.Add(new ColumnModel() { IsPrimaryKey = true, ColumnName = "Id", ColumnType = "NVARCHAR(500)", ColumnId = 1, SchemaName = table.SchemaName });
                    table.Columns.Add(new ColumnModel() { ColumnName = "Date", ColumnType = "DATETIME", ColumnId = 2, SchemaName = table.SchemaName });
                    table.Columns.Add(new ColumnModel() { ColumnName = "Skipped", ColumnType = "BIT", IsNullable = true, TableName = table.TableName, ColumnId = 3, SchemaName = table.SchemaName });

                    var pk = new IndexModel()
                    {
                        SchemaName = table.SchemaName,
                        TableName = table.TableName,
                        IndexName = "PK_Migrations",
                        IndexType = "CLUSTERED",
                        IsPrimaryKey = true
                    };

                    pk.Columns.Add(new IndexColumnModel() { ColumnName = "Id" });

                    var provider = _databaseFactory.GetDatabaseProvider(connectionString);

                    var sb = new StringBuilder();

                    var tableScript = provider.GetScript(table);
                    if (tableScript is not null)
                    {
                        sb.AppendLine(tableScript.CreateScript);
                    }

                    var pkScript = provider.GetScript(pk, false);
                    if (pkScript is not null)
                    {
                        sb.AppendLine(pkScript.CreateScript);
                    }

                    var existingTable = currentDatabaseModel.Tables.FirstOrDefault(t =>
                            t.TableName == table.TableName);

                    // If the table doesn't currently exist, it will be created with all new columns
                    if (existingTable is not null)
                    {
                        foreach (var column in table.Columns)
                        {
                            var existingColumn = existingTable?.Columns.FirstOrDefault(i => i.ColumnName.EqualsIgnoreCase(column.ColumnName));

                            if (existingColumn is not null)
                            {
                                continue;
                            }

                            var columnScript = provider.GetColumnScript(column);
                            if (columnScript is not null)
                            {
                                sb.AppendLine(columnScript.CreateScript);
                            }
                        }
                    }

                    var script = sb.ToString();

                    _databaseExecutionService.ExecuteFile(connectionString, script);

                    shouldEnsureMigrationsTable = false;
                }
            }

            DatabaseModel readCurrentModel() => _databaseModelService.CreateModel(connectionString, new Models.DatabaseModelOptions { ExcludeDoubleUnderscoreObjects = true });

            var currentDatabaseModel = readCurrentModel();
            ensureMigrationsTable(currentDatabaseModel);

            migrations = getMigrations();

            var updatedDatabaseModel = DatabaseModel.CreateFromFiles(modelList.Select(i => (i.FullName, File.ReadAllText(i.FullName))));
            var provider = _databaseFactory.GetDatabaseProvider(connectionString);

            var tablesDirectory = directory.GetDirectories("02_Tables");

            if (tablesDirectory.Length > 0)
            {
                var renameFound = false;
                foreach (var current in currentDatabaseModel.Tables)
                {
                    var updatedTable = updatedDatabaseModel.Tables.FirstOrDefault(i =>
                        i.SchemaName.EqualsIgnoreCase(current.SchemaName) && i.TableName.EqualsIgnoreCase(current.TableName));
                    if (updatedTable is null && current.TableName != "Migrations")
                    {
                        var rename = provider.GetRenameScript(current, $"__{current.TableName}", dropNewTableIfExists: true);

                        progress?.Invoke(new ScriptProgress() { ProgressPercentage = 0, ProgressStatus = $"__{current.TableName}" });

                        _databaseExecutionService.ExecuteFile(connectionString, rename);

                        renameFound = true;
                    }
                }

                if (renameFound)
                {
                    currentDatabaseModel = readCurrentModel();
                }
            }
            var directories = new List<(DirectoryInfo Directory, bool IsMigration, List<(string Name, string Contents)> Files)>();
            var managedDirectories = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var dir in directory.GetDirectories().OrderBy(i => i.Name))
            {
                var category = _directoryList.FirstOrDefault(dir.Name.Contains);
                if (category != null && dir.GetFiles("*.sql.json").Length > 0)
                {
                    managedDirectories.Add(category);
                }

                if (dir.Name.EndsWith("_Migrations"))
                {
                    foreach (var migration in dir.GetDirectories().OrderBy(i => i.Name))
                    {
                        var list = getFiles(migration).ToList();
                        if (list.Count > 0)
                        {
                            directories.Add(new(migration, true, list));
                        }
                    }
                }
                else
                {
                    directories.Add(new(dir, false, getFiles(dir).ToList()));
                }
            }

            int? getSlot(string category)
            {
                if (!managedDirectories.Contains(category))
                {
                    return null;
                }

                var slot = directories.FindIndex(d => !d.IsMigration && d.Directory.Name.Contains(category));
                return slot < 0 ? null : slot;
            }

            var databaseName = provider.GetDatabaseName(connectionString.ConnectionString);
            var reportedTables = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var reportedSkips = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            DatabaseDependencyPlan createPlan(IEnumerable<string> migrationSql, IEnumerable<string> pendingKeys)
            {
                var plan = new DatabaseDependencyPlan(currentDatabaseModel, updatedDatabaseModel, provider, migrationSql, getSlot, databaseName, pendingKeys);
                foreach (var table in plan.ChangedTables.Where(reportedTables.Add))
                {
                    progress?.Invoke(new ScriptProgress() { ProgressPercentage = 0, ProgressStatus = "Table Change: " + table });
                }

                foreach (var skipped in plan.SkippedChanges.Where(reportedSkips.Add))
                {
                    progress?.Invoke(new ScriptProgress() { ProgressPercentage = 0, ProgressStatus = "Migration Required: " + skipped });
                }

                return plan;
            }

            var plan = createPlan([], []);

            // A re-plan only applies from planStart on; folders that already ran are not revisited, except that a drop
            // moves up when its object is still created in a later folder.
            var planStart = 0;
            int? slotOf(DatabaseDependencyPlan.Script script, bool drop)
            {
                if (script.Slot >= planStart)
                {
                    return script.Slot;
                }

                return drop && plan.Creates.Any(c => c.Slot >= planStart && c.Key.Equals(script.Key, StringComparison.OrdinalIgnoreCase)) ? planStart : null;
            }

            IEnumerable<DatabaseDependencyPlan.Script> scheduled(Func<int, bool> slots) =>
                plan.Drops.Where(s => slotOf(s, true) is int slot && slots(slot))
                .Concat(plan.Creates.Where(s => slotOf(s, false) is int slot && slots(slot)));

            double total = scheduled(_ => true).Count() + directories.Sum(d => d.Files.Count);
            var index = 0;

            var errors = new List<string>();

            for (var slot = 0; slot < directories.Count; slot++)
            {
                var dir = directories[slot];

                // Folders run in name order as before; each folder's planned drops then creates run before its own scripts.
                var steps = scheduled(s => s == slot)
                    .Select(s => (s.Name, s.Contents, IsMigration: false))
                    .Concat(dir.Files.Select(f => (f.Name, f.Contents, dir.IsMigration)))
                    .ToList();

                foreach (var file in steps)
                {
                    int percent = Math.Min(100, Convert.ToInt32((index / total) * 100));

                    progress?.Invoke(new ScriptProgress() { ProgressPercentage = percent, ProgressStatus = $"{file.Name}" });

                    try
                    {
                        if (!(file.IsMigration && skipMigrations) && !string.IsNullOrEmpty(file.Contents.Trim()))
                        {
                            _databaseExecutionService.ExecuteFile(connectionString, file.Contents);
                        }
                        if (file.IsMigration)
                        {
                            addMigration(file.Name);
                        }
                    }
                    catch (Exception ex)
                    {
                        errors.Add($"{file.Name} ErrorMessage: {ex.Message}");
                        progress?.Invoke(new ScriptProgress() { ProgressPercentage = percent, ProgressStatus = $"{file.Name}", ErrorMessage = ex.Message });
                        if (!continueOnError || file.IsMigration)
                        {
                            throw new Exception(string.Join(Environment.NewLine, errors));
                        }
                    }

                    index += 1;
                }

                //refresh the knowledge of the current schema after migrations and plan the rest against it
                if (dir.IsMigration)
                {
                    currentDatabaseModel = readCurrentModel();
                    var pendingKeys = scheduled(s => s > slot).Select(s => s.Key).Where(plan.RefreshKeys.Contains).ToList();
                    plan = createPlan(skipMigrations ? [] : dir.Files.Select(f => f.Contents), pendingKeys);
                    planStart = slot + 1;
                    total = index + scheduled(_ => true).Count() + directories.Skip(slot + 1).Sum(d => d.Files.Count);
                }
            }
            if (errors.Count > 0)
            {
                throw new Exception(string.Join(Environment.NewLine, errors));
            }
        }

        int retryCount = continueOnError ? 5 : 0;
        int retryIndex = 0;

        while (retryIndex <= retryCount)
        {
            try
            {
                executeScriptsInernal();
                retryIndex = retryCount + 1;
            }
            catch
            {
                if (retryIndex == retryCount)
                {
                    throw;
                }
                retryIndex += 1;
                progress?.Invoke(new ScriptProgress() { ProgressPercentage = 0, ProgressStatus = $"Retry {retryIndex} of {retryCount}" });
            }
        }

        progress?.Invoke(new ScriptProgress() { ProgressPercentage = 100, ProgressStatus = "Complete" });
    }

    public string MergeScripts(IEnumerable<string> scripts)
    {
        System.Text.StringBuilder sbFile = new System.Text.StringBuilder();

        sbFile.AppendLine("SET NOCOUNT ON");
        sbFile.AppendLine();

        foreach (var file in scripts)
        {
            string strFileContents = System.IO.File.ReadAllText(file);
            if (!(string.IsNullOrEmpty(strFileContents)))
            {
                sbFile.AppendLine("PRINT '*********** " + System.IO.Path.GetFileName(file) + " ***********'");
                sbFile.AppendLine("GO");
                sbFile.AppendLine();
                sbFile.AppendLine(strFileContents);
            }
        }

        sbFile.AppendLine();
        sbFile.AppendLine("SET NOCOUNT OFF");

        return sbFile.ToString();
    }

    public void MergeScripts(IEnumerable<string> scripts, string toFile)
    {
        var strFile = MergeScripts(scripts);

        System.IO.File.WriteAllText(toFile, strFile);
    }
}
