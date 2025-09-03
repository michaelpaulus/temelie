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
        if (!Directory.Exists(Path.GetDirectoryName(path)))
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path));
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

                    if (File.Exists(jsonPath))
                    {
                        var currentTable = JsonSerializer.Deserialize<TableModel>(File.ReadAllText(jsonPath), ModelsJsonSerializerOptions.Default);
                        var newTable = JsonSerializer.Deserialize<TableModel>(json, ModelsJsonSerializerOptions.Default);

                        foreach (var column in currentTable.Columns)
                        {
                            column.TableName = currentTable.TableName;
                        }

                        foreach (var column in newTable.Columns)
                        {
                            column.TableName = newTable.TableName;
                        }

                        var newColumns = newTable.Columns.Where(i => !currentTable.Columns.Any(i2 => i.ColumnName == i2.ColumnName)).ToList();
                        var removedColumns = currentTable.Columns.Where(i => !newTable.Columns.Any(i2 => i.ColumnName == i2.ColumnName)).ToList();

                        if (newColumns.Count > 0 || removedColumns.Count > 0)
                        {
                            var changeFileName = Path.Combine(directory.Parent.FullName, "04_Migrations", DateTime.UtcNow.ToString("yyyy-MM-dd"), $"01_{table.TableName}.sql");

                            var sb = new StringBuilder();

                            foreach (var newColumn in newColumns)
                            {
                                var columnScript = provider.GetColumnScript(newColumn);
                                if (columnScript is not null && !string.IsNullOrEmpty(columnScript.CreateScript))
                                {
                                    sb.AppendLine(columnScript.CreateScript);
                                }
                            }

                            foreach (var removedColumn in removedColumns)
                            {
                                var columnScript = provider.GetColumnScript(removedColumn);
                                if (columnScript is not null && !string.IsNullOrEmpty(columnScript.DropScript))
                                {
                                    sb.AppendLine(columnScript.DropScript);
                                }
                            }

                            if (sb.Length > 0)
                            {
                                WriteIfDifferent(changeFileName, sb.ToString());
                            }
                        }

                    }

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

    public void CreateScripts(ConnectionStringModel connectionString, System.IO.DirectoryInfo directory, IProgress<ScriptProgress> progress, string objectFilter = "", IDatabaseProvider createScriptsProvider = null)
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

            progress?.Report(new ScriptProgress() { ProgressPercentage = intProgress, ProgressStatus = subDirectory.Name });

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

        progress?.Report(new ScriptProgress() { ProgressPercentage = 100, ProgressStatus = "Complete" });

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

    public void ExecuteScripts(ConnectionStringModel connectionString, DirectoryInfo directory, IProgress<ScriptProgress> progress, bool continueOnError = true, bool skipMigrations = false)
    {
        void addMigration(string id)
        {
            using var conn = _databaseExecutionService.CreateDbConnection(connectionString);
            using var cmd = _databaseExecutionService.CreateDbCommand(conn);
            var skipMigrationsBit = skipMigrations ? "1" : "0";
            cmd.CommandText = $"INSERT INTO Migrations (Id, Date, Skipped) VALUES {$"('{id}', '{DateTime.UtcNow:yyyy-MM-ddTHH:mm:ss}', {skipMigrationsBit})"}";
            cmd.ExecuteNonQuery();
        }

        IEnumerable<string> getMigrations(string id)
        {
            using var conn = _databaseExecutionService.CreateDbConnection(connectionString);
            using var cmd = _databaseExecutionService.CreateDbCommand(conn);
            cmd.CommandText = $"SELECT Id FROM Migrations WHERE Id LIKE '{id}'";
            using var reader = cmd.ExecuteReader();
            var list = new List<string>();
            while (reader.Read())
            {
                list.Add(reader.GetString(0));
            }
            return list;
        }

        bool checkMigration(string id)
        {
            return getMigrations(id).Any();
        }

        var hasTableChange = false;

        IEnumerable<(string Name, string Contents)> getFiles(DirectoryInfo dir, DatabaseModel currentDatabaseModel, DatabaseModel updatedDatabaseModel, IDatabaseProvider provider)
        {
            if (dir.Parent.Name.EndsWith("_Migrations"))
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

                    if (jsonFiles.Count > 0)
                    {

                        void compareObjects<T>(IList<T> currentList,
                            IList<T> updatedList,
                            Func<T, string> getName,
                            Func<T, IDatabaseObjectScript> createScript,
                            bool alwaysUpdate = false,
                            Action<T> handleDrop = null,
                            Action<T, T> handleUpdate = null) where T : DatabaseObjectModel
                        {
                            foreach (var current in currentList)
                            {
                                var name = getName(current);

                                var updated = updatedList.FirstOrDefault(i => getName(i) == name);
                                if (updated is null)
                                {
                                    if (handleDrop is null)
                                    {
                                        var script = createScript(current);
                                        if (script is not null)
                                        {
                                            list.Add(($"DROP {dir.Name}/{name}", script.DropScript));
                                        }
                                    }
                                    else
                                    {
                                        handleDrop(current);
                                    }
                                }
                                else
                                {
                                    updatedList.Remove(updated);
                                    var currentScript = createScript(current);
                                    var updatedScript = createScript(updated);
                                    if (currentScript is not null &&
                                        updatedScript is not null &&
                                        (alwaysUpdate ||
                                        currentScript.CreateScript != updatedScript.CreateScript))
                                    {
                                        if (handleUpdate is null)
                                        {
                                            list.Add(($"DROP {dir.Name}/{name}", updatedScript.DropScript));
                                            list.Add(($"CREATE {dir.Name}/{name}", updatedScript.CreateScript));
                                        }
                                        else
                                        {
                                            handleUpdate(current, updated);
                                        }
                                    }
                                }
                            }

                            foreach (var updated in updatedList)
                            {
                                var name = getName(updated);
                                var script = createScript(updated);
                                list.Add(($"CREATE {dir.Name}/{name}", script.CreateScript));
                            }
                        }

                        if (dir.Name.Contains("02_Tables"))
                        {
                            compareObjects(
                                currentDatabaseModel.Tables.ToList(),
                                updatedDatabaseModel.Tables.ToList(),
                                i => $"{i.SchemaName}.{i.TableName}",
                                i => provider.GetScript(i),
                                handleDrop: (current) =>
                                {
                                    hasTableChange = true;
                                },
                                handleUpdate: (current, updated) =>
                                {
                                    hasTableChange = true;
                                }
                             );
                        }
                        else if (dir.Name.Contains("03_Indexes"))
                        {
                            compareObjects(
                                currentDatabaseModel.PrimaryKeys.ToList(),
                                updatedDatabaseModel.PrimaryKeys.ToList(),
                                i => $"{i.SchemaName}.{i.TableName}.{i.IndexName}",
                                i => provider.GetScript(i, IsView(i, currentDatabaseModel))
                             );
                            compareObjects(
                                currentDatabaseModel.Indexes.ToList(),
                                updatedDatabaseModel.Indexes.ToList(),
                                i => $"{i.SchemaName}.{i.TableName}.{i.IndexName}",
                                i => provider.GetScript(i, IsView(i, currentDatabaseModel))
                             );
                        }
                        else if (dir.Name.Contains("04_CheckConstraints"))
                        {
                            compareObjects(
                                currentDatabaseModel.CheckConstraints.ToList(),
                                updatedDatabaseModel.CheckConstraints.ToList(),
                                i => $"{i.SchemaName}.{i.TableName}.{i.CheckConstraintName}",
                                i => provider.GetScript(i)
                             );
                        }
                        else if (dir.Name.Contains("05_Programmability"))
                        {
                            compareObjects(
                                currentDatabaseModel.Definitions.Where(i => i.View is null).ToList(),
                                updatedDatabaseModel.Definitions.Where(i => i.View is null).ToList(),
                                i => $"{i.SchemaName}.{i.DefinitionName}",
                                i => provider.GetScript(i),
                                alwaysUpdate: hasTableChange
                             );
                        }
                        else if (dir.Name.Contains("05_Views"))
                        {
                            compareObjects(
                                currentDatabaseModel.Definitions.Where(i => i.View is not null).ToList(),
                                updatedDatabaseModel.Definitions.Where(i => i.View is not null).ToList(),
                                i => $"{i.SchemaName}.{i.DefinitionName}",
                                i => provider.GetScript(i),
                                alwaysUpdate: hasTableChange
                             );
                        }
                        else if (dir.Name.Contains("06_Triggers"))
                        {
                            compareObjects(
                                currentDatabaseModel.Triggers.ToList(),
                                updatedDatabaseModel.Triggers.ToList(),
                                i => $"{i.SchemaName}.{i.TriggerName}",
                                i => provider.GetScript(i),
                                alwaysUpdate: hasTableChange
                             );
                        }
                        else if (dir.Name.Contains("08_ForeignKeys"))
                        {
                            compareObjects(
                               currentDatabaseModel.ForeignKeys.ToList(),
                               updatedDatabaseModel.ForeignKeys.ToList(),
                               i => $"{i.SchemaName}.{i.TableName}.{i.ForeignKeyName}",
                               i => provider.GetScript(i)
                            );
                        }
                        else if (dir.Name.Contains("09_SecurityPolicies"))
                        {
                            compareObjects(
                               currentDatabaseModel.SecurityPolicies.ToList(),
                               updatedDatabaseModel.SecurityPolicies.ToList(),
                               i => $"{i.PolicySchema}.{i.PolicyName}",
                               i => provider.GetScript(i)
                            );
                        }
                    }
                    else
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

            var currentDatabaseModel = _databaseModelService.CreateModel(connectionString, new Models.DatabaseModelOptions { ExcludeDoubleUnderscoreObjects = true });
            ensureMigrationsTable(currentDatabaseModel);
            var updatedDatabaseModel = DatabaseModel.CreateFromFiles(modelList.Select(i => (i.FullName, File.ReadAllText(i.FullName))));
            var provider = _databaseFactory.GetDatabaseProvider(connectionString);

            var tablesDirectory = directory.GetDirectories("02_Tables");

            if (tablesDirectory is not null)
            {
                var renameFound = false;
                foreach (var current in currentDatabaseModel.Tables)
                {
                    var updatedTable = updatedDatabaseModel.Tables.FirstOrDefault(i => i.SchemaName == current.SchemaName && i.TableName == current.TableName);
                    if (updatedTable is null && current.TableName != "Migrations")
                    {
                        var rename = provider.GetRenameScript(current, $"__{current.TableName}");

                        progress?.Report(new ScriptProgress() { ProgressPercentage = 0, ProgressStatus = $"__{current.TableName}" });

                        _databaseExecutionService.ExecuteFile(connectionString, rename);

                        renameFound = true;
                    }
                }
                if (renameFound)
                {
                    currentDatabaseModel = _databaseModelService.CreateModel(connectionString, new Models.DatabaseModelOptions { ExcludeDoubleUnderscoreObjects = true });
                }
            }

            var directories = new List<(DirectoryInfo Directory, bool IsMigration)>();

            foreach (var dir in directory.GetDirectories().OrderBy(i => i.Name))
            {
                if (dir.Name.EndsWith("_Migrations"))
                {
                    foreach (var migration in dir.GetDirectories().OrderBy(i => i.Name))
                    {
                        var list = getFiles(migration, currentDatabaseModel, updatedDatabaseModel, provider);
                        if (list.Any())
                        {
                            directories.Add(new(migration, true));
                        }
                    }
                }
                else
                {
                    var list = getFiles(dir, currentDatabaseModel, updatedDatabaseModel, provider);
                    if (list.Any())
                    {
                        directories.Add(new(dir, false));
                    }
                }
            }

            double total = directories.Sum(i => getFiles(i.Directory, currentDatabaseModel, updatedDatabaseModel, provider).Count());
            var index = 0;

            var errors = new List<string>();

            foreach (var dir in directories)
            {
                var files = getFiles(dir.Directory, currentDatabaseModel, updatedDatabaseModel, provider);

                foreach (var file in files)
                {
                    int percent = Convert.ToInt32((index / total) * 100);

                    progress?.Report(new ScriptProgress() { ProgressPercentage = percent, ProgressStatus = $"{file.Name}" });

                    try
                    {
                        if (!(dir.IsMigration && skipMigrations) && !string.IsNullOrEmpty(file.Contents.Trim()))
                        {
                            _databaseExecutionService.ExecuteFile(connectionString, file.Contents);
                        }
                        if (dir.IsMigration)
                        {
                            addMigration(file.Name);
                        }
                    }
                    catch (Exception ex)
                    {
                        errors.Add($"{file.Name} ErrorMessage: {ex.Message}");
                        progress?.Report(new ScriptProgress() { ProgressPercentage = percent, ProgressStatus = $"{file.Name}", ErrorMessage = ex.Message });
                        if (!continueOnError || dir.IsMigration)
                        {
                            throw new Exception(string.Join(Environment.NewLine, errors));
                        }
                    }

                    index += 1;
                }

                //refresh the knowledge of the current schema after migrations
                if (dir.IsMigration)
                {
                    currentDatabaseModel = _databaseModelService.CreateModel(connectionString, new Models.DatabaseModelOptions { ExcludeDoubleUnderscoreObjects = true });
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
                progress?.Report(new ScriptProgress() { ProgressPercentage = 0, ProgressStatus = $"Retry {retryIndex} of {retryCount}" });
            }
        }

        progress?.Report(new ScriptProgress() { ProgressPercentage = 100, ProgressStatus = "Complete" });
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
