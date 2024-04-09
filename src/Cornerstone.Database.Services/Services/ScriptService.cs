using System.Text;
using Cornerstone.Database.Extensions;
using Cornerstone.Database.Providers;

namespace Cornerstone.Database.Services;

public class ScriptService
{
    private readonly IDatabaseFactory _databaseFactory;

    public ScriptService(IDatabaseFactory databaseFactory)
    {
        _databaseFactory = databaseFactory;
    }

    private FileInfo WriteIfDifferent(string path, string contents)
    {
        contents = contents.Replace("\r\n", "\n").Replace("\r", "\n").Replace("\n", "\r\n");
        contents = contents.Replace("\t", "    ");
        var currentContents = "";
        if (File.Exists(path))
        {
            currentContents = File.ReadAllText(path);
        }
        if (currentContents != contents)
        {
            File.WriteAllText(path, contents, System.Text.Encoding.UTF8);
        }
        return new FileInfo(path);
    }

    private IEnumerable<FileInfo> CreateDropScripts(Models.DatabaseModel database, System.IO.DirectoryInfo directory, string fileFilter = "")
    {
        var files = new List<FileInfo>();

        foreach (var trigger in database.Triggers)
        {
            var fileName = MakeValidFileName($"{trigger.SchemaName}.{trigger.TriggerName}.sql");
            if (string.IsNullOrEmpty(fileFilter) ||
               fileFilter.EqualsIgnoreCase(fileName))
            {
                var sb = new StringBuilder();

                trigger.AppendDropScript(sb, database.QuoteCharacterStart, database.QuoteCharacterEnd);

                files.Add(WriteIfDifferent(System.IO.Path.Combine(directory.FullName, fileName), sb.ToString()));
            }

        }

        foreach (var foreignKey in database.ForeignKeys)
        {
            var fileName = MakeValidFileName($"{foreignKey.ForeignKeyName}.sql");
            if (string.IsNullOrEmpty(fileFilter) ||
              fileFilter.EqualsIgnoreCase(fileName))
            {
                var sb = new StringBuilder();
                foreignKey.AppendDropScript(sb, database.QuoteCharacterStart, database.QuoteCharacterEnd);
                files.Add(WriteIfDifferent(System.IO.Path.Combine(directory.FullName, fileName), sb.ToString()));
            }
        }

        return files;
    }

    private IEnumerable<FileInfo> CreateTableScripts(Models.DatabaseModel database, System.IO.DirectoryInfo directory, string fileFilter = "")
    {
        var files = new List<FileInfo>();

        foreach (var table in database.Tables)
        {
            var fileName = MakeValidFileName($"{table.SchemaName}.{table.TableName}.sql");
            if (string.IsNullOrEmpty(fileFilter) ||
                fileFilter.EqualsIgnoreCase(fileName))
            {
                var sbTableScript = new StringBuilder();
                table.AppendCreateScript(sbTableScript, database.QuoteCharacterStart, database.QuoteCharacterEnd, true);
                files.Add(WriteIfDifferent(System.IO.Path.Combine(directory.FullName, fileName), sbTableScript.ToString()));

                fileName += ".json";

                var sbJsonScript = new StringBuilder();
                table.AppendJsonScript(sbJsonScript);
                files.Add(WriteIfDifferent(System.IO.Path.Combine(directory.FullName, fileName), sbJsonScript.ToString()));
            }
        }

        return files;
    }
    private IEnumerable<FileInfo> CreateSecurityPolicyScripts(Models.DatabaseModel database, System.IO.DirectoryInfo directory, string fileFilter = "")
    {
        var files = new List<FileInfo>();

        foreach (var securityPolicy in database.SecurityPolicies)
        {
            var fileName = MakeValidFileName($"{securityPolicy.PolicySchema}.{securityPolicy.PolicyName}.sql");
            if (string.IsNullOrEmpty(fileFilter) ||
                fileFilter.EqualsIgnoreCase(fileName))
            {
                var sbSecurityPolicyScript = new StringBuilder();
                securityPolicy.AppendDropScript(sbSecurityPolicyScript, database.QuoteCharacterStart, database.QuoteCharacterEnd);
                securityPolicy.AppendCreateScript(sbSecurityPolicyScript, database.QuoteCharacterStart, database.QuoteCharacterEnd, true);

                files.Add(WriteIfDifferent(System.IO.Path.Combine(directory.FullName, fileName), sbSecurityPolicyScript.ToString()));
            }

        }

        return files;
    }
    private IEnumerable<FileInfo> CreateIndexScripts(Models.DatabaseModel database, System.IO.DirectoryInfo directory, string fileFilter = "")
    {
        var files = new List<FileInfo>();

        foreach (var pk in database.PrimaryKeys)
        {
            var fileName = MakeValidFileName($"{pk.SchemaName}.{pk.IndexName}.sql");
            if (string.IsNullOrEmpty(fileFilter) ||
                fileFilter.EqualsIgnoreCase(fileName))
            {
                var sb = new StringBuilder();
                pk.AppendCreateScript(sb, database.QuoteCharacterStart, database.QuoteCharacterEnd);

                files.Add(WriteIfDifferent(System.IO.Path.Combine(directory.FullName, fileName), sb.ToString()));
            }
        }

        foreach (var index in database.Indexes)
        {
            var fileName = MakeValidFileName($"{index.SchemaName}.{index.IndexName}.sql");
            if (string.IsNullOrEmpty(fileFilter) ||
                fileFilter.EqualsIgnoreCase(fileName))
            {
                var sb = new StringBuilder();
                index.AppendCreateScript(sb, database.QuoteCharacterStart, database.QuoteCharacterEnd);
                files.Add(WriteIfDifferent(System.IO.Path.Combine(directory.FullName, fileName), sb.ToString()));
            }
        }

        return files;
    }
    private IEnumerable<FileInfo> CreateCheckConstraintScripts(Models.DatabaseModel database, System.IO.DirectoryInfo directory, string fileFilter = "")
    {
        var files = new List<FileInfo>();

        foreach (var constraint in database.CheckConstraints)
        {
            var fileName = MakeValidFileName($"{constraint.SchemaName}.{constraint.CheckConstraintName}.sql");
            if (string.IsNullOrEmpty(fileFilter) ||
                fileFilter.EqualsIgnoreCase(fileName))
            {
                var sb = new StringBuilder();
                constraint.AppendDropScript(sb, database.QuoteCharacterStart, database.QuoteCharacterEnd);
                constraint.AppendCreateScript(sb, database.QuoteCharacterStart, database.QuoteCharacterEnd);

                files.Add(WriteIfDifferent(System.IO.Path.Combine(directory.FullName, fileName), sb.ToString()));
            }
        }

        return files;
    }
    private IEnumerable<FileInfo> CreateViewsAndProgrammabilityScripts(Models.DatabaseModel database, System.IO.DirectoryInfo directory, bool includeViews, bool includeProgrammability, string fileFilter = "")
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

                var sb = new System.Text.StringBuilder();
                definition.AppendDropScript(sb, database.QuoteCharacterStart, database.QuoteCharacterEnd);
                definition.AppendCreateScript(sb, database.QuoteCharacterStart, database.QuoteCharacterEnd);

                files.Add(WriteIfDifferent(System.IO.Path.Combine(directory.FullName, fileName), sb.ToString()));

                if (definition.View != null)
                {
                    fileName += ".json";

                    var sbJsonScript = new StringBuilder();
                    definition.View.AppendJsonScript(sbJsonScript);
                    files.Add(WriteIfDifferent(System.IO.Path.Combine(directory.FullName, fileName), sbJsonScript.ToString()));
                }
            }

        }

        return files;
    }

    private IEnumerable<FileInfo> CreateTriggerScripts(Models.DatabaseModel database, System.IO.DirectoryInfo directory, string fileFilter = "")
    {
        var files = new List<FileInfo>();

        foreach (var trigger in database.Triggers)
        {
            var fileName = MakeValidFileName($"{trigger.SchemaName}.{trigger.TriggerName}.sql");
            if (string.IsNullOrEmpty(fileFilter) ||
                fileFilter.EqualsIgnoreCase(fileName))
            {
                var sb = new System.Text.StringBuilder();

                trigger.AppendDropScript(sb, database.QuoteCharacterStart, database.QuoteCharacterEnd);
                trigger.AppendScript(sb, database.QuoteCharacterStart, database.QuoteCharacterEnd);

                files.Add(WriteIfDifferent(System.IO.Path.Combine(directory.FullName, fileName), sb.ToString()));
            }
        }

        return files;
    }

    private IEnumerable<FileInfo> CreateFkScripts(Models.DatabaseModel database, System.IO.DirectoryInfo directory, string fileFilter = "")
    {
        var files = new List<FileInfo>();

        foreach (var foreignKey in database.ForeignKeys)
        {
            var fileName = MakeValidFileName($"{foreignKey.SchemaName}.{foreignKey.ForeignKeyName}.sql");
            if (string.IsNullOrEmpty(fileFilter) ||
                fileFilter.EqualsIgnoreCase(fileName))
            {
                var sb = new System.Text.StringBuilder();

                foreignKey.AppendDropScript(sb, database.QuoteCharacterStart, database.QuoteCharacterEnd);
                foreignKey.AppendCreateScript(sb, database.QuoteCharacterStart, database.QuoteCharacterEnd);
                files.Add(WriteIfDifferent(System.IO.Path.Combine(directory.FullName, fileName), sb.ToString()));
            }
        }

        return files;
    }

    public void CreateScriptsIndividual(System.Configuration.ConnectionStringSettings connectionString, System.IO.DirectoryInfo directory, IProgress<ScriptProgress> progress, string objectFilter = "")
    {
        Models.DatabaseModel database = new Models.DatabaseModel(_databaseFactory, connectionString) { ObjectFilter = objectFilter, ExcludeDoubleUnderscoreObjects = true };

        var directoryList = new List<string>()
                {
                    "01_Drops",
                    "02_Tables",
                    "03_Indexes",
                    "04_CheckConstraints",
                    "05_ViewsAndProgrammability",
                    "05_Views",
                    "05_Programmability",
                    "06_Triggers",
                    "07_InsertDefaults",
                    "08_ForeignKeys",
                    "09_SecurityPolicies"
                };

        int intIndex = 1;
        int intTotalCount = directoryList.Count();

        int intProgress = 0;

        foreach (var subDirectoryName in directoryList)
        {

            string subDirectoryPath = System.IO.Path.Combine(directory.FullName, subDirectoryName);

            if (System.IO.Directory.Exists(subDirectoryPath))
            {
                var subDirectory = new System.IO.DirectoryInfo(subDirectoryPath);

                progress?.Report(new ScriptProgress() { ProgressPercentage = intProgress, ProgressStatus = subDirectory.Name });

                if (subDirectory.Name.StartsWith("01_Drops", StringComparison.InvariantCultureIgnoreCase))
                {
                    var files = CreateDropScripts(database, subDirectory).ToDictionary(i => i.Name, StringComparer.OrdinalIgnoreCase);
                    foreach (var file in subDirectory.GetFiles("*.sql"))
                    {
                        if (!files.ContainsKey(file.Name))
                        {
                            file.Delete();
                        }
                    }

                }
                else if (subDirectory.Name.StartsWith("02_Tables", StringComparison.InvariantCultureIgnoreCase))
                {
                    var files = CreateTableScripts(database, subDirectory).ToDictionary(i => i.Name, StringComparer.OrdinalIgnoreCase);
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
                else if (subDirectory.Name.StartsWith("03_Indexes", StringComparison.InvariantCultureIgnoreCase))
                {

                    var files = CreateIndexScripts(database, subDirectory).ToDictionary(i => i.Name, StringComparer.OrdinalIgnoreCase);
                    foreach (var file in subDirectory.GetFiles("*.sql"))
                    {
                        if (!files.ContainsKey(file.Name))
                        {
                            file.Delete();
                        }
                    }
                }
                else if (subDirectory.Name.StartsWith("04_CheckConstraints", StringComparison.InvariantCultureIgnoreCase))
                {

                    var files = CreateCheckConstraintScripts(database, subDirectory).ToDictionary(i => i.Name, StringComparer.OrdinalIgnoreCase);
                    foreach (var file in subDirectory.GetFiles("*.sql"))
                    {
                        if (!files.ContainsKey(file.Name))
                        {
                            file.Delete();
                        }
                    }
                }
                else if (subDirectory.Name.StartsWith("05_ViewsAndProgrammability", StringComparison.InvariantCultureIgnoreCase))
                {
                    var files = CreateViewsAndProgrammabilityScripts(database, subDirectory, true, true).ToDictionary(i => i.Name, StringComparer.OrdinalIgnoreCase);
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
                else if (subDirectory.Name.StartsWith("05_Programmability", StringComparison.InvariantCultureIgnoreCase))
                {
                    var files = CreateViewsAndProgrammabilityScripts(database, subDirectory, false, true).ToDictionary(i => i.Name, StringComparer.OrdinalIgnoreCase);
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
                else if (subDirectory.Name.StartsWith("05_Views", StringComparison.InvariantCultureIgnoreCase))
                {
                    var files = CreateViewsAndProgrammabilityScripts(database, subDirectory, true, false).ToDictionary(i => i.Name, StringComparer.OrdinalIgnoreCase);
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
                else if (subDirectory.Name.StartsWith("06_Triggers", StringComparison.InvariantCultureIgnoreCase))
                {
                    var files = CreateTriggerScripts(database, subDirectory).ToDictionary(i => i.Name, StringComparer.OrdinalIgnoreCase);
                    foreach (var file in subDirectory.GetFiles("*.sql"))
                    {
                        if (!files.ContainsKey(file.Name))
                        {
                            file.Delete();
                        }
                    }
                }
                else if (subDirectory.Name.StartsWith("08_ForeignKeys", StringComparison.InvariantCultureIgnoreCase))
                {
                    var files = CreateFkScripts(database, subDirectory).ToDictionary(i => i.Name, StringComparer.OrdinalIgnoreCase);
                    foreach (var file in subDirectory.GetFiles("*.sql"))
                    {
                        if (!files.ContainsKey(file.Name))
                        {
                            file.Delete();
                        }
                    }
                }
                else if (subDirectory.Name.StartsWith("09_SecurityPolicies", StringComparison.InvariantCultureIgnoreCase))
                {
                    var files = CreateSecurityPolicyScripts(database, subDirectory).ToDictionary(i => i.Name, StringComparer.OrdinalIgnoreCase);
                    foreach (var file in subDirectory.GetFiles("*.sql"))
                    {
                        if (!files.ContainsKey(file.Name))
                        {
                            file.Delete();
                        }
                    }
                }
            }

            intProgress = Convert.ToInt32((intIndex / (double)intTotalCount) * 100);

            intIndex += 1;

        }

        foreach (var file in directory.GetFiles("*_merge.sql"))
        {
            var mergeList = new List<string>();

            foreach (var subDirectory in directory.GetDirectories().OrderBy(i => i.FullName))
            {
                string executionOrderFileName = System.IO.Path.Combine(subDirectory.FullName, "_executionOrder.txt");

                if (System.IO.File.Exists(executionOrderFileName))
                {
                    foreach (var line in System.IO.File.ReadAllLines(executionOrderFileName))
                    {
                        if (!string.IsNullOrEmpty(line))
                        {
                            var fileName = System.IO.Path.Combine(subDirectory.FullName, line);
                            if (System.IO.File.Exists(fileName))
                            {
                                mergeList.Add(fileName);
                            }
                        }
                    }
                }
                else
                {
                    mergeList.AddRange(subDirectory.GetFiles("*.sql", System.IO.SearchOption.AllDirectories).OrderBy(i => i.FullName).Select(i => i.FullName));
                }
            }

            MergeScripts(mergeList, file.FullName);
        }

    }

    public void CreateScriptIndividual(System.Configuration.ConnectionStringSettings connectionString, System.IO.DirectoryInfo directory, string fileName)
    {
        Models.DatabaseModel database = new Models.DatabaseModel(_databaseFactory, connectionString) { ExcludeDoubleUnderscoreObjects = true };
        if (directory.Name.StartsWith("01_Drops", StringComparison.InvariantCultureIgnoreCase))
        {
            CreateDropScripts(database, directory, fileName);
        }
        else if (directory.Name.StartsWith("02_Tables", StringComparison.InvariantCultureIgnoreCase))
        {
            CreateTableScripts(database, directory, fileName);
        }
        else if (directory.Name.StartsWith("03_Indexes", StringComparison.InvariantCultureIgnoreCase))
        {
            CreateIndexScripts(database, directory, fileName);
        }
        else if (directory.Name.StartsWith("05_ViewsAndProgrammability", StringComparison.InvariantCultureIgnoreCase))
        {
            CreateViewsAndProgrammabilityScripts(database, directory, true, true, fileName);
        }
        else if (directory.Name.StartsWith("05_Programmability", StringComparison.InvariantCultureIgnoreCase))
        {
            CreateViewsAndProgrammabilityScripts(database, directory, false, true, fileName);
        }
        else if (directory.Name.StartsWith("05_Views", StringComparison.InvariantCultureIgnoreCase))
        {
            CreateViewsAndProgrammabilityScripts(database, directory, true, false, fileName);
        }
        else if (directory.Name.StartsWith("06_Triggers", StringComparison.InvariantCultureIgnoreCase))
        {
            CreateTriggerScripts(database, directory, fileName);
        }
        else if (directory.Name.StartsWith("08_ForeignKeys", StringComparison.InvariantCultureIgnoreCase))
        {
            CreateFkScripts(database, directory, fileName);
        }
        else if (directory.Name.StartsWith("09_SecurityPolicies", StringComparison.InvariantCultureIgnoreCase))
        {
            CreateSecurityPolicyScripts(database, directory, fileName);
        }
    }

    private string MakeValidFileName(string name)
    {
        string invalidChars = System.Text.RegularExpressions.Regex.Escape(new string(System.IO.Path.GetInvalidFileNameChars()));
        string invalidRegStr = string.Format(@"([{0}]*\.+$)|([{0}]+)", invalidChars);

        return System.Text.RegularExpressions.Regex.Replace(name, invalidRegStr, "_");
    }

    public void ExecuteScripts(System.Configuration.ConnectionStringSettings connectionString, IEnumerable<System.IO.FileInfo> fileList, bool continueOnError, IProgress<ScriptProgress> progress)
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
                progress.Report(new ScriptProgress() { ProgressPercentage = percent, ProgressStatus = file.Name });
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
                    var databaseProvider = _databaseFactory.GetDatabaseProvider(connectionString);
                    var database = new DatabaseService(_databaseFactory, databaseProvider);
                    database.ExecuteFile(connectionString, strFile);
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
                            progress.Report(new ScriptProgress() { ProgressPercentage = percent, ProgressStatus = file.Name, ErrorMessage = ex.Message });
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

    }

    public string MergeScripts(IList<string> scripts)
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

    public void MergeScripts(IList<string> scripts, string toFile)
    {
        var strFile = MergeScripts(scripts);

        System.IO.File.WriteAllText(toFile, strFile);
    }

}
