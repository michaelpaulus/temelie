using System.Text;
using System.Text.Json;
using Temelie.Database.Extensions;
using Temelie.Database.Models;
using Temelie.Database.Providers;
using Temelie.DependencyInjection;
using System.Security.Cryptography;

namespace Temelie.Database.Services;
[ExportTransient(typeof(IScriptService))]
public class ScriptService : IScriptService
{
    private readonly IDatabaseFactory _databaseFactory;
    private readonly IDatabaseExecutionService _databaseExecutionService;
    private readonly IDatabaseModelService _databaseModelService;

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

        if (path.EndsWith(".sql"))
        {
            contents = contents.Replace("\r\n", "\n").Replace("\r", "\n").Replace("\n", Environment.NewLine);
            contents = contents.Replace("\t", "    ");
        }
        var currentContents = "";
        bool changed = false;
        bool isNew = false;
        if (File.Exists(path))
        {
            currentContents = File.ReadAllText(path);
        }
        if (currentContents != contents)
        {
            isNew = !File.Exists(path);
            changed = true;
            File.WriteAllText(path, contents, System.Text.Encoding.UTF8);
        }
        return (new FileInfo(path), changed, isNew);
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
                    sbSecurityPolicyScript.AppendLine(script.DropScript);
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
            if (!files.ContainsKey(fileName))
            {
                if (string.IsNullOrEmpty(fileFilter) ||
                fileFilter.EqualsIgnoreCase(fileName))
                {
                    var script = provider.GetScript(pk);
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
            if (!files.ContainsKey(fileName))
            {
                if (string.IsNullOrEmpty(fileFilter) ||
                fileFilter.EqualsIgnoreCase(fileName))
                {
                    var script = provider.GetScript(index);
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
                    sb.Append(script.DropScript);
                    sb.Append(script.CreateScript);

                    files.Add(WriteIfDifferent(System.IO.Path.Combine(directory.FullName, fileName), sb.ToString()).File);
                    files.Add(WriteIfDifferent(System.IO.Path.Combine(directory.FullName, fileName + ".json"), GetJson(constraint)).File);
                }
            }
        }

        return files;
    }
    private IEnumerable<FileInfo> CreateViewsAndProgrammabilityScripts(IDatabaseProvider provider, Models.DatabaseModel database, System.IO.DirectoryInfo directory, bool includeViews, bool includeProgrammability, string fileFilter = "")
    {
        var files = new List<FileInfo>();

        foreach (var definition in database.Definitions)
        {
            var fileName = MakeValidFileName($"{definition.SchemaName}.{definition.DefinitionName}.sql");
            if (string.IsNullOrEmpty(fileFilter) ||
                fileFilter.EqualsIgnoreCase(fileName))
            {

                if (definition.View != null && !includeViews)
                {
                    continue;
                }
                else if (definition.View == null && !includeProgrammability)
                {
                    continue;
                }

                var script = provider.GetScript(definition);

                if (script is not null)
                {
                    var sb = new System.Text.StringBuilder();
                    sb.Append(script.DropScript);
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

                    sb.Append(script.DropScript);
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
                    sb.Append(script.DropScript);
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
        json = json.Replace("\\u0027", "'").Replace("\\u003C", "<").Replace("\\u003E", ">").Replace("\\u002B", "+");
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

        var directoryList = new List<string>()
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

        foreach (var di in directoryList)
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
        int intTotalCount = directoryList.Count;

        int intProgress = 0;

        void syncFiles<T>(DirectoryInfo subDirectory, Dictionary<string, FileInfo> files, Func<T, IDatabaseObjectScript> getScript) where T : Model
        {
            foreach (var file in subDirectory.GetFiles("*.sql"))
            {
                if (files.ContainsKey(file.Name))
                {
                    var dropFileName = Path.Combine(directory.FullName, "01_Drop", subDirectory.Name, file.Name);
                    if (File.Exists(dropFileName))
                    {
                        File.Delete(dropFileName);
                    }
                }
                else
                {
                    file.Delete();
                }
            }
            foreach (var file in subDirectory.GetFiles("*.sql.json"))
            {
                var dropFileName = Path.Combine(directory.FullName, "01_Drop", subDirectory.Name, file.Name);
                if (files.ContainsKey(file.Name))
                {
                    if (File.Exists(dropFileName))
                    {
                        File.Delete(dropFileName);
                    }
                }
                else
                {
                    var model = JsonSerializer.Deserialize<T>(File.ReadAllText(file.FullName), ModelsJsonSerializerOptions.Default);
                    var script = getScript(model);
                    if (script is not null)
                    {
                        WriteIfDifferent(dropFileName.Replace(".json", ""), script.DropScript);
                        WriteIfDifferent(dropFileName, File.ReadAllText(file.FullName));
                    }
                    file.Delete();
                }
            }
        }

        foreach (var subDirectoryName in directoryList)
        {

            string subDirectoryPath = System.IO.Path.Combine(directory.FullName, subDirectoryName);

            var subDirectory = new System.IO.DirectoryInfo(subDirectoryPath);

            progress?.Report(new ScriptProgress() { ProgressPercentage = intProgress, ProgressStatus = subDirectory.Name });

            if (subDirectory.Name.StartsWith("02_Tables", StringComparison.InvariantCultureIgnoreCase))
            {
                var files = CreateTableScripts(provider, database, subDirectory).ToDictionary(i => i.Name, StringComparer.OrdinalIgnoreCase);
                syncFiles<TableModel>(subDirectory, files, provider.GetScript);
            }
            else if (subDirectory.Name.StartsWith("03_Indexes", StringComparison.InvariantCultureIgnoreCase))
            {
                var files = CreateIndexScripts(provider, database, subDirectory).ToDictionary(i => i.Name, StringComparer.OrdinalIgnoreCase);
                syncFiles<IndexModel>(subDirectory, files, provider.GetScript);
            }
            else if (subDirectory.Name.StartsWith("04_CheckConstraints", StringComparison.InvariantCultureIgnoreCase))
            {
                var files = CreateCheckConstraintScripts(provider, database, subDirectory).ToDictionary(i => i.Name, StringComparer.OrdinalIgnoreCase);
                syncFiles<CheckConstraintModel>(subDirectory, files, provider.GetScript);
            }
            else if (subDirectory.Name.StartsWith("05_Programmability", StringComparison.InvariantCultureIgnoreCase))
            {
                var files = CreateViewsAndProgrammabilityScripts(provider, database, subDirectory, false, true).ToDictionary(i => i.Name, StringComparer.OrdinalIgnoreCase);
                syncFiles<DefinitionModel>(subDirectory, files, provider.GetScript);
            }
            else if (subDirectory.Name.StartsWith("05_Views", StringComparison.InvariantCultureIgnoreCase))
            {
                var files = CreateViewsAndProgrammabilityScripts(provider, database, subDirectory, true, false).ToDictionary(i => i.Name, StringComparer.OrdinalIgnoreCase);
                syncFiles<TableModel>(subDirectory, files, provider.GetScript);
            }
            else if (subDirectory.Name.StartsWith("06_Triggers", StringComparison.InvariantCultureIgnoreCase))
            {
                var files = CreateTriggerScripts(provider, database, subDirectory).ToDictionary(i => i.Name, StringComparer.OrdinalIgnoreCase);
                syncFiles<TriggerModel>(subDirectory, files, provider.GetScript);
            }
            else if (subDirectory.Name.StartsWith("08_ForeignKeys", StringComparison.InvariantCultureIgnoreCase))
            {
                var files = CreateFkScripts(provider, database, subDirectory).ToDictionary(i => i.Name, StringComparer.OrdinalIgnoreCase);
                syncFiles<ForeignKeyModel>(subDirectory, files, provider.GetScript);
            }
            else if (subDirectory.Name.StartsWith("09_SecurityPolicies", StringComparison.InvariantCultureIgnoreCase))
            {
                var files = CreateSecurityPolicyScripts(provider, database, subDirectory).ToDictionary(i => i.Name, StringComparer.OrdinalIgnoreCase);
                syncFiles<SecurityPolicyModel>(subDirectory, files, provider.GetScript);
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

    public void ExecuteScripts(ConnectionStringModel connectionString, DirectoryInfo directory, IProgress<ScriptProgress> progress, bool continueOnError = true)
    {
        int intFileCount = 1;

        var list = new List<FileInfo>();

        var pendingMigrations = new List<string>();

        var shouldEnsureMigrationsTable = true;

        void ensureMigrationsTable()
        {
            if (shouldEnsureMigrationsTable)
            {
                var table = new TableModel()
                {
                    SchemaName = "dbo",
                    TableName = "Migrations"
                };
                table.Columns.Add(new ColumnModel() { IsPrimaryKey = true, ColumnName = "Id", ColumnType = "NVARCHAR(500)" });
                table.Columns.Add(new ColumnModel() { IsPrimaryKey = true, ColumnName = "Date", ColumnType = "DATETIME" });

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

                var pkScript = provider.GetScript(pk);
                if (pkScript is not null)
                {
                    sb.AppendLine(pkScript.CreateScript);
                }

                var script = sb.ToString();

                _databaseExecutionService.ExecuteFile(connectionString, script);

                shouldEnsureMigrationsTable = false;
            }
        }

        ensureMigrationsTable();

        foreach (var dir in directory.GetDirectories().OrderBy(i => i.Name))
        {
            if (dir.Name.EndsWith("_Migrations"))
            {
                foreach (var migration in dir.GetDirectories().OrderBy(i => i.Name))
                {
                    var files = migration.GetFiles("*.sql", SearchOption.AllDirectories).OrderBy(i => i.FullName);
                    if (files.Any())
                    {
                        var hash = CreateMd5ForDirectory(migration.FullName);
                        var id = $"{dir.Name}/{migration.Name}/{hash}";
                        using var conn = _databaseExecutionService.CreateDbConnection(connectionString);
                        using var cmd = _databaseExecutionService.CreateDbCommand(conn);
                        cmd.CommandText = $"SELECT COUNT(*) FROM Migrations WHERE Id = '{id}'";
                        var migrationCount = long.Parse(cmd.ExecuteScalar().ToString());
                        if (migrationCount == 0)
                        {
                            pendingMigrations.Add(id);
                            list.AddRange(files);
                        }
                    }
                }
            }
            else
            {
                var files = dir.GetFiles("*.sql", SearchOption.AllDirectories).OrderBy(i => i.FullName);
                if (files.Any())
                {
                    var hash = CreateMd5ForDirectory(dir.FullName);
                    var id = $"{dir.Name}/{hash}";

                    using var conn = _databaseExecutionService.CreateDbConnection(connectionString);
                    using var cmd = _databaseExecutionService.CreateDbCommand(conn);
                    cmd.CommandText = $"SELECT COUNT(*) FROM Migrations WHERE Id = '{id}'";
                    var migrationCount = long.Parse(cmd.ExecuteScalar().ToString());

                    // if this folder has changed, or a previous step has changed, run these files
                    if (pendingMigrations.Count > 0 || migrationCount == 0)
                    {
                        //only add the id if it doesn't already exist
                        if (migrationCount == 0)
                        {
                            pendingMigrations.Add(id);
                        }
                        list.AddRange(files);
                    }

                }
            }
        }

        double count = list.Count;

        var finalErrors = new List<string>();

        var retryList = new Dictionary<FileInfo, int>();

        while (list.Count > 0)
        {
            var file = list.First();
            list.Remove(file);

            string strFile = string.Empty;

            if (progress != null)
            {
                int percent = Convert.ToInt32((intFileCount / count) * 100);
                progress.Report(new ScriptProgress() { ProgressPercentage = percent, ProgressStatus = $"{file.Directory.Name}/{file.Name}" });
            }

            using (System.IO.Stream stream = System.IO.File.OpenRead(file.FullName))
            {
                using (System.IO.StreamReader reader = new System.IO.StreamReader(stream))
                {
                    strFile = reader.ReadToEnd();
                    reader.Close();
                }
                stream.Close();
            }

            if (!(string.IsNullOrEmpty(strFile.Trim())))
            {
                try
                {
                    _databaseExecutionService.ExecuteFile(connectionString, strFile);
                }
                catch (Exception ex)
                {
                    if (continueOnError)
                    {
                        if (ex.Message.Contains("Invalid object name") && (!retryList.TryGetValue(file, out int retryCount) || retryCount < 5))
                        {
                            if (retryCount == 0)
                            {
                                retryList.Add(file, 1);
                            }
                            else
                            {
                                retryList[file] += 1;
                            }
                            list.Add(file);
                            count += 1;
                        }
                        else
                        {
                            finalErrors.Add($"{file.Directory.Name}/{file.Name} ErrorMessage: {ex.Message}");
                            if (progress is not null)
                            {
                                int percent = Convert.ToInt32((intFileCount / count) * 100);
                                progress.Report(new ScriptProgress() { ProgressPercentage = percent, ProgressStatus = $"{file.Directory.Name}/{file.Name}", ErrorMessage = ex.Message });
                            }
                        }
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            intFileCount += 1;
        }

        if (pendingMigrations.Count > 0)
        {
            using var conn = _databaseExecutionService.CreateDbConnection(connectionString);
            using var cmd = _databaseExecutionService.CreateDbCommand(conn);
            cmd.CommandText = $"INSERT INTO Migrations (Id, Date) VALUES {string.Join(", ", pendingMigrations.Select(i => $"('{i}', '{DateTime.UtcNow:yyyy-MM-ddTHH:mm:ss}')"))}";
            cmd.ExecuteNonQuery();
        }

        progress?.Report(new ScriptProgress() { ProgressPercentage = 100, ProgressStatus = "Complete" });

        if (finalErrors.Count > 0)
        {
            throw new Exception(string.Join(Environment.NewLine, finalErrors));
        }
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

    private static string CreateMd5ForDirectory(string path)
    {
        // assuming you want to include nested folders
        var files = Directory.GetFiles(path, "*", SearchOption.AllDirectories)
                             .OrderBy(p => p).ToList();

        MD5 md5 = MD5.Create();

        for (int i = 0; i < files.Count; i++)
        {
            string file = files[i];

            // hash path
            string relativePath = file.Substring(path.Length + 1);
            byte[] pathBytes = Encoding.UTF8.GetBytes(relativePath.ToLower());
            md5.TransformBlock(pathBytes, 0, pathBytes.Length, pathBytes, 0);

            // hash contents
            byte[] contentBytes = File.ReadAllBytes(file);
            if (i == files.Count - 1)
            {
                md5.TransformFinalBlock(contentBytes, 0, contentBytes.Length);
            }
            else
            {
                md5.TransformBlock(contentBytes, 0, contentBytes.Length, contentBytes, 0);
            }
        }

        return BitConverter.ToString(md5.Hash).Replace("-", "").ToLower();
    }

}
