using System.Text;
using System.Text.Json.Serialization;
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
        contents = contents.Replace("\r\n", "\n").Replace("\r", "\n").Replace("\n", "\r\n");
        contents = contents.Replace("\t", "    ");
        var currentContents = "";
        bool changed = false;
        bool isNew = false;
        if (File.Exists(path))
        {
            currentContents = File.ReadAllText(path);
        }
        if (currentContents != contents)
        {
            changed = !File.Exists(path);
            isNew = !changed;
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

                files.Add(WriteIfDifferent(System.IO.Path.Combine(directory.FullName, fileName), script.CreateScript));
                files.Add(WriteIfDifferent(System.IO.Path.Combine(directory.FullName, fileName + ".json"), GetJson(table)));

            }
        }

        foreach (var file in files.Where(i => i.Changed && i.File.Name.EndsWith(".sql")).Select(i => i.File))
        {
            var changeFileName = Path.Combine(directory.Parent.FullName, "04_Migrations", DateTime.UtcNow.ToString("yyyy-MM-dd"), $"01_{file.Name}");
            if (!Directory.Exists(Path.GetDirectoryName(changeFileName)))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(changeFileName));
            }
            if (!File.Exists(changeFileName))
            {
                WriteIfDifferent(changeFileName, @"TODO: write migration

-- example: add column null
IF NOT EXISTS (SELECT 1 FROM sys.tables INNER JOIN sys.columns ON tables.object_id = columns.object_id INNER JOIN sys.schemas ON tables.schema_id = schemas.schema_id WHERE schemas.name = '{schemaName}' AND tables.name = '{tableName}' AND columns.name = '{columnName}')
BEGIN
	ALTER TABLE {schemaName}.{tableName} ADD {columnName} NULL
END
GO

-- example: add column not null with default
IF NOT EXISTS (SELECT 1 FROM sys.tables INNER JOIN sys.columns ON tables.object_id = columns.object_id INNER JOIN sys.schemas ON tables.schema_id = schemas.schema_id WHERE schemas.name = '{schemaName}' AND tables.name = '{tableName}' AND columns.name = '{columnName}')
BEGIN
	ALTER TABLE {schemaName}.{tableName} ADD {columnName} NOT NULL CONSTRAINT {contraintName} DEFAULT {defaultValue} WITH VALUES
END
GO

IF EXISTS (SELECT 1 FROM sys.check_constraints INNER JOIN sys.tables ON check_constraints.parent_object_id = tables.object_id INNER JOIN sys.schemas ON tables.schema_id = schemas.schema_id WHERE check_constraints.name = '{contraintName}' AND schemas.name = '{schemaName}')
BEGIN
	ALTER TABLE {schemaName}.{tableName} DROP CONSTRAINT {contraintName}
END
GO

-- example: delete column
IF EXISTS (SELECT 1 FROM sys.tables INNER JOIN sys.columns ON tables.object_id = columns.object_id INNER JOIN sys.schemas ON tables.schema_id = schemas.schema_id WHERE schemas.name = '{schemaName}' AND tables.name = '{tableName}' AND columns.name = '{columnName}')
BEGIN
	ALTER TABLE {schemaName}.{tableName} DROP COLUMN {columnName}
END
GO
");
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

                var sbSecurityPolicyScript = new StringBuilder();
                sbSecurityPolicyScript.AppendLine(script.DropScript);
                sbSecurityPolicyScript.AppendLine(script.CreateScript);

                files.Add(WriteIfDifferent(System.IO.Path.Combine(directory.FullName, fileName), sbSecurityPolicyScript.ToString()).File);
                files.Add(WriteIfDifferent(System.IO.Path.Combine(directory.FullName, fileName + ".json"), GetJson(securityPolicy)).File);

            }

        }

        return files;
    }
    private IEnumerable<FileInfo> CreateIndexScripts(IDatabaseProvider provider, Models.DatabaseModel database, System.IO.DirectoryInfo directory, string fileFilter = "")
    {
        var files = new List<FileInfo>();

        foreach (var pk in database.PrimaryKeys)
        {
            var fileName = MakeValidFileName($"{pk.SchemaName}.{pk.IndexName}.sql");
            if (string.IsNullOrEmpty(fileFilter) ||
                fileFilter.EqualsIgnoreCase(fileName))
            {
                var script = provider.GetScript(pk);
                files.Add(WriteIfDifferent(System.IO.Path.Combine(directory.FullName, fileName), script.CreateScript).File);
                files.Add(WriteIfDifferent(System.IO.Path.Combine(directory.FullName, fileName + ".json"), GetJson(pk)).File);

            }
        }

        foreach (var index in database.Indexes)
        {
            var fileName = MakeValidFileName($"{index.SchemaName}.{index.IndexName}.sql");
            if (string.IsNullOrEmpty(fileFilter) ||
                fileFilter.EqualsIgnoreCase(fileName))
            {
                var script = provider.GetScript(index);
                files.Add(WriteIfDifferent(System.IO.Path.Combine(directory.FullName, fileName), script.CreateScript).File);
                files.Add(WriteIfDifferent(System.IO.Path.Combine(directory.FullName, fileName + ".json"), GetJson(index)).File);
            }
        }

        return files;
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
                var sb = new StringBuilder();
                sb.Append(script.DropScript);
                sb.Append(script.CreateScript);

                files.Add(WriteIfDifferent(System.IO.Path.Combine(directory.FullName, fileName), sb.ToString()).File);
                files.Add(WriteIfDifferent(System.IO.Path.Combine(directory.FullName, fileName + ".json"), GetJson(constraint)).File);
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

                var sb = new System.Text.StringBuilder();
                sb.Append(script.DropScript);
                sb.Append(script.CreateScript);

                files.Add(WriteIfDifferent(System.IO.Path.Combine(directory.FullName, fileName), sb.ToString()).File);
                files.Add(WriteIfDifferent(System.IO.Path.Combine(directory.FullName, fileName + ".json"), GetJson(definition)).File);
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
                var sb = new System.Text.StringBuilder();

                sb.Append(script.DropScript);
                sb.Append(script.CreateScript);

                files.Add(WriteIfDifferent(System.IO.Path.Combine(directory.FullName, fileName), sb.ToString()).File);
                files.Add(WriteIfDifferent(System.IO.Path.Combine(directory.FullName, fileName + ".json"), GetJson(trigger)).File);
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
                var sb = new System.Text.StringBuilder();
                sb.Append(script.DropScript);
                sb.Append(script.CreateScript);
                files.Add(WriteIfDifferent(System.IO.Path.Combine(directory.FullName, fileName), sb.ToString()).File);
                files.Add(WriteIfDifferent(System.IO.Path.Combine(directory.FullName, fileName + ".json"), GetJson(foreignKey)).File);
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

    public void CreateScripts(ConnectionStringModel connectionString, System.IO.DirectoryInfo directory, IProgress<ScriptProgress> progress, string objectFilter = "")
    {
        var provider = _databaseFactory.GetDatabaseProvider(connectionString);
        Models.DatabaseModel database = _databaseModelService.CreateModel(connectionString, new Models.DatabaseModelOptions { ObjectFilter = objectFilter, ExcludeDoubleUnderscoreObjects = true });

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
        int intTotalCount = directoryList.Count();

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
                    WriteIfDifferent(dropFileName.Replace(".json", ""), script.DropScript);
                    WriteIfDifferent(dropFileName, File.ReadAllText(file.FullName));
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
        string invalidChars = System.Text.RegularExpressions.Regex.Escape(new string(System.IO.Path.GetInvalidFileNameChars()));
        string invalidRegStr = string.Format(@"([{0}]*\.+$)|([{0}]+)", invalidChars);

        return System.Text.RegularExpressions.Regex.Replace(name, invalidRegStr, "_");
    }

    public void ExecuteScripts(ConnectionStringModel connectionString, IEnumerable<System.IO.FileInfo> fileList, bool continueOnError, IProgress<ScriptProgress> progress)
    {
        int intFileCount = 1;

        var list = fileList.ToList();

        double count = list.Count;

        var retryList = new Dictionary<FileInfo, int>();

        while (list.Any())
        {
            var file = list.First();
            list.Remove(file);

            string strFile = string.Empty;

            if (progress != null)
            {
                int percent = Convert.ToInt32((intFileCount / count) * 100);
                progress.Report(new ScriptProgress() { ProgressPercentage = percent, ProgressStatus = $"{file.Directory.Name}/{file.Name}"});
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
                        else if (progress != null)
                        {
                            int percent = Convert.ToInt32((intFileCount / count) * 100);
                            progress.Report(new ScriptProgress() { ProgressPercentage = percent, ProgressStatus = $"{file.Directory.Name}/{file.Name}", ErrorMessage = ex.Message });
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
