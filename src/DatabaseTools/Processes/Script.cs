
using DatabaseTools.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Xml.Linq;

namespace DatabaseTools
{
    namespace Processes
    {
        public class Script
        {

            private static void CreateDropScripts(Models.DatabaseModel database, System.IO.DirectoryInfo directory, string fileFilter = "")
            {
                foreach (var item in database.GetTriggerDropScriptsIndividual())
                {
                    var fileName = MakeValidFileName($"{item.Key.SchemaName}.{item.Key.TriggerName}.sql");
                    if (string.IsNullOrEmpty(fileFilter) ||
                       fileFilter.EqualsIgnoreCase(fileName))
                    {
                        System.IO.File.WriteAllText(System.IO.Path.Combine(directory.FullName, fileName), item.Value);
                    }
                }

                foreach (var item in database.GetFkDropScriptsIndividual())
                {
                    var fileName = MakeValidFileName($"{item.Key.ForeignKeyName}.sql");
                    if (string.IsNullOrEmpty(fileFilter) ||
                      fileFilter.EqualsIgnoreCase(fileName))
                    {
                        System.IO.File.WriteAllText(System.IO.Path.Combine(directory.FullName, fileName), item.Value);
                    }
                }
            }

            private static void CreateTableScripts(Models.DatabaseModel database, System.IO.DirectoryInfo directory, string fileFilter = "")
            {
                foreach (var item in database.GetTableScriptsIndividual())
                {
                    var fileName = MakeValidFileName($"{item.Key.SchemaName}.{item.Key.TableName}.sql");
                    if (string.IsNullOrEmpty(fileFilter) ||
                        fileFilter.EqualsIgnoreCase(fileName))
                    {
                        System.IO.File.WriteAllText(System.IO.Path.Combine(directory.FullName, fileName), item.Value);
                    }
                }
            }
            private static void CreateSecurityPolicyScripts(Models.DatabaseModel database, System.IO.DirectoryInfo directory, string fileFilter = "")
            {
                foreach (var item in database.GetSecurityPolicyScriptsIndividual())
                {
                    var fileName = MakeValidFileName($"{item.Key.PolicySchema}.{item.Key.PolicyName}.sql");
                    if (string.IsNullOrEmpty(fileFilter) ||
                        fileFilter.EqualsIgnoreCase(fileName))
                    {
                        System.IO.File.WriteAllText(System.IO.Path.Combine(directory.FullName, fileName), item.Value);
                    }
                }
            }
            private static void CreateIndexScripts(Models.DatabaseModel database, System.IO.DirectoryInfo directory, string fileFilter = "")
            {
                foreach (var item in database.GetIxPkScriptsIndividual())
                {
                    var fileName = MakeValidFileName($"{item.Key.SchemaName}.{item.Key.IndexName}.sql");
                    if (string.IsNullOrEmpty(fileFilter) ||
                        fileFilter.EqualsIgnoreCase(fileName))
                    {
                        System.IO.File.WriteAllText(System.IO.Path.Combine(directory.FullName, fileName), item.Value);
                    }
                }
            }

            private static void CreateViewsAndProgrammabilityScripts(Models.DatabaseModel database, System.IO.DirectoryInfo directory, string fileFilter = "")
            {
                var sbExecutionOrder = new System.Text.StringBuilder();

                foreach (var item in database.GetDefinitionScriptsIndividual())
                {
                    var fileName = MakeValidFileName($"{item.Key.SchemaName}.{item.Key.DefinitionName}.sql");
                    sbExecutionOrder.AppendLine(fileName);
                    if (string.IsNullOrEmpty(fileFilter) ||
                        fileFilter.EqualsIgnoreCase(fileName))
                    {
                        System.IO.File.WriteAllText(System.IO.Path.Combine(directory.FullName, fileName), item.Value);
                    }
                }

                if (string.IsNullOrEmpty(fileFilter))
                {
                    System.IO.File.WriteAllText(System.IO.Path.Combine(directory.FullName, "_executionOrder.txt"), sbExecutionOrder.ToString());
                }
            }

            private static void CreateTriggerScripts(Models.DatabaseModel database, System.IO.DirectoryInfo directory, string fileFilter = "")
            {
                foreach (var item in database.GetTriggerScriptsIndividual())
                {
                    var fileName = MakeValidFileName($"{item.Key.SchemaName}.{item.Key.TriggerName}.sql");
                    if (string.IsNullOrEmpty(fileFilter) ||
                        fileFilter.EqualsIgnoreCase(fileName))
                    {
                        System.IO.File.WriteAllText(System.IO.Path.Combine(directory.FullName, fileName), item.Value);
                    }
                }
            }

            private static void CreateInsertDefaultScripts(Models.DatabaseModel database, System.IO.DirectoryInfo directory, string fileFilter = "")
            {

                foreach (var item in database.GetInsertDefaultScriptsIndividual())
                {
                    var fileName = MakeValidFileName($"{item.Key.SchemaName}.{item.Key.TableName}.sql");
                    if (string.IsNullOrEmpty(fileFilter) ||
                        fileFilter.EqualsIgnoreCase(fileName))
                    {
                        System.IO.File.WriteAllText(System.IO.Path.Combine(directory.FullName, fileName), item.Value);
                    }
                }
            }

            private static void CreateFkScripts(Models.DatabaseModel database, System.IO.DirectoryInfo directory, string fileFilter = "")
            {
                foreach (var item in database.GetFkScriptsIndividual())
                {
                    var fileName = MakeValidFileName($"{item.Key.SchemaName}.{item.Key.ForeignKeyName}.sql");
                    if (string.IsNullOrEmpty(fileFilter) ||
                        fileFilter.EqualsIgnoreCase(fileName))
                    {
                        System.IO.File.WriteAllText(System.IO.Path.Combine(directory.FullName, fileName), item.Value);
                    }
                }
            }

            public static void CreateScriptsIndividual(System.Configuration.ConnectionStringSettings connectionString, System.IO.DirectoryInfo directory, Models.DatabaseType targetDatabaseType, IProgress<ScriptProgress> progress, string objectFilter = "")
            {
                Models.DatabaseModel database = new Models.DatabaseModel(connectionString, targetDatabaseType) { ObjectFilter = objectFilter, ExcludeDoubleUnderscoreObjects = true };

                var directoryList = new List<string>()
                {
                    "01_Drops",
                    "02_Tables",
                    "03_Indexes",
                    "05_ViewsAndProgrammability",
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

                        if (progress != null)
                        {
                            progress.Report(new ScriptProgress() { ProgressPercentage = intProgress, ProgressStatus = subDirectory.Name });
                        }


                        if (subDirectory.Name.StartsWith("01_Drops", StringComparison.InvariantCultureIgnoreCase))
                        {
                            foreach (var file in subDirectory.GetFiles("*.sql"))
                            {
                                file.Delete();
                            }

                            CreateDropScripts(database, subDirectory);

                        }
                        else if (subDirectory.Name.StartsWith("02_Tables", StringComparison.InvariantCultureIgnoreCase))
                        {
                            foreach (var file in subDirectory.GetFiles("*.sql"))
                            {
                                file.Delete();
                            }

                            CreateTableScripts(database, subDirectory);
                        }
                        else if (subDirectory.Name.StartsWith("03_Indexes", StringComparison.InvariantCultureIgnoreCase))
                        {
                            foreach (var file in subDirectory.GetFiles("*.sql"))
                            {
                                file.Delete();
                            }

                            CreateIndexScripts(database, subDirectory);
                        }
                        else if (subDirectory.Name.StartsWith("05_ViewsAndProgrammability", StringComparison.InvariantCultureIgnoreCase))
                        {
                            foreach (var file in subDirectory.GetFiles("*.sql"))
                            {
                                file.Delete();
                            }

                            CreateViewsAndProgrammabilityScripts(database, subDirectory);
                        }
                        else if (subDirectory.Name.StartsWith("06_Triggers", StringComparison.InvariantCultureIgnoreCase))
                        {
                            foreach (var file in subDirectory.GetFiles("*.sql"))
                            {
                                file.Delete();
                            }

                            CreateTriggerScripts(database, subDirectory);
                        }
                        else if (subDirectory.Name.StartsWith("07_InsertDefaults", StringComparison.InvariantCultureIgnoreCase))
                        {
                            foreach (var file in subDirectory.GetFiles("*.sql"))
                            {
                                file.Delete();
                            }

                            CreateInsertDefaultScripts(database, subDirectory);
                        }
                        else if (subDirectory.Name.StartsWith("08_ForeignKeys", StringComparison.InvariantCultureIgnoreCase))
                        {
                            foreach (var file in subDirectory.GetFiles("*.sql"))
                            {
                                file.Delete();
                            }

                            CreateFkScripts(database, subDirectory);
                        }
                        else if (subDirectory.Name.StartsWith("09_SecurityPolicies", StringComparison.InvariantCultureIgnoreCase))
                        {
                            foreach (var file in subDirectory.GetFiles("*.sql"))
                            {
                                file.Delete();
                            }

                            CreateSecurityPolicyScripts(database, subDirectory);
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

            public static void CreateScriptIndividual(System.Configuration.ConnectionStringSettings connectionString, System.IO.DirectoryInfo directory, Models.DatabaseType targetDatabaseType, string fileName)
            {
                Models.DatabaseModel database = new Models.DatabaseModel(connectionString, targetDatabaseType) { ExcludeDoubleUnderscoreObjects = true };
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
                    CreateViewsAndProgrammabilityScripts(database, directory, fileName);
                }
                else if (directory.Name.StartsWith("06_Triggers", StringComparison.InvariantCultureIgnoreCase))
                {
                    CreateTriggerScripts(database, directory, fileName);
                }
                else if (directory.Name.StartsWith("07_InsertDefaults", StringComparison.InvariantCultureIgnoreCase))
                {
                    CreateInsertDefaultScripts(database, directory, fileName);
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

            private static string MakeValidFileName(string name)
            {
                string invalidChars = System.Text.RegularExpressions.Regex.Escape(new string(System.IO.Path.GetInvalidFileNameChars()));
                string invalidRegStr = string.Format(@"([{0}]*\.+$)|([{0}]+)", invalidChars);

                return System.Text.RegularExpressions.Regex.Replace(name, invalidRegStr, "_");
            }

            public static void CreateScripts(System.Configuration.ConnectionStringSettings connectionString, IList<System.IO.FileInfo> fileList, Models.DatabaseType targetDatabaseType, IProgress<ScriptProgress> progress, string objectFilter = "")
            {
                int intFileCount = 1;

                int intProgress = 0;

                fileList = (from i in fileList orderby i.FullName select i).ToList();

                Models.DatabaseModel database = new Models.DatabaseModel(connectionString, targetDatabaseType) { ObjectFilter = objectFilter, ExcludeDoubleUnderscoreObjects = true };

                foreach (System.IO.FileInfo file in fileList)
                {
                    bool blnSkipFile = false;

                    if (file.Name.EndsWith("_merge.sql", StringComparison.InvariantCultureIgnoreCase))
                    {
                        blnSkipFile = true;
                    }

                    if (!blnSkipFile)
                    {
                        if (progress != null)
                        {
                            progress.Report(new ScriptProgress() { ProgressPercentage = intProgress, ProgressStatus = file.Name });
                        }
                        CreateScript(database, file);
                    }

                    intProgress = Convert.ToInt32((intFileCount / (double)fileList.Count) * 100);

                    intFileCount += 1;
                }

                if (progress != null)
                {
                    progress.Report(new ScriptProgress() { ProgressPercentage = 100, ProgressStatus = "Completed" });
                }

                var mergeFile = (
                    from i in fileList
                    where i.FullName.EndsWith("_merge.sql", StringComparison.InvariantCultureIgnoreCase)
                    select i).FirstOrDefault();
                if (mergeFile != null)
                {
                    var mergeList = (
                        from i in fileList
                        where i != mergeFile
                        select i.FullName).ToList();
                    MergeScripts(mergeList, mergeFile.FullName);
                }

            }

            public static void CreateScript(Models.DatabaseModel database, System.IO.FileInfo file)
            {

                System.Text.StringBuilder sbFile = new System.Text.StringBuilder();

                System.Text.StringBuilder sbPostGenerated = new System.Text.StringBuilder();

                using (System.IO.Stream stream = System.IO.File.OpenRead(file.FullName))
                {
                    using (System.IO.StreamReader reader = new System.IO.StreamReader(stream))
                    {

                        bool blnBeginGeneratedFound = false;
                        bool blnEndGeneratedFound = false;

                        while (!(reader.EndOfStream))
                        {
                            string strLine = reader.ReadLine();
                            if (strLine.Trim().StartsWith("-- BEGIN GENERATED"))
                            {
                                blnBeginGeneratedFound = true;
                            }
                            else if (strLine.Trim().StartsWith("-- END GENERATED"))
                            {
                                blnEndGeneratedFound = true;
                            }
                            else if (blnBeginGeneratedFound)
                            {
                                if (blnEndGeneratedFound)
                                {
                                    sbPostGenerated.AppendLine(strLine);
                                }
                            }
                            else
                            {
                                sbFile.AppendLine(strLine);
                            }
                        }
                        reader.Close();
                    }
                    stream.Close();
                }

                sbFile.AppendLine("-- BEGIN GENERATED SQL");
                sbFile.AppendLine();

                if (file.Name.EndsWith("_drop.sql", StringComparison.InvariantCultureIgnoreCase))
                {
                    sbFile.AppendLine(database.GetDefinitionDropScripts());
                    sbFile.AppendLine(database.GetTriggerDropScripts());
                    sbFile.AppendLine(database.GetFkDropScripts());
                }
                else if (file.Name.EndsWith("_create_tables.sql", StringComparison.InvariantCultureIgnoreCase))
                {
                    sbFile.AppendLine(database.GetTableScripts());
                }
                else if (file.Name.EndsWith("_create_ix_pk.sql", StringComparison.InvariantCultureIgnoreCase))
                {
                    sbFile.AppendLine(database.GetIxPkScripts());
                }
                else if (file.Name.EndsWith("_create_sp_fn_vw.sql", StringComparison.InvariantCultureIgnoreCase))
                {
                    sbFile.AppendLine(database.GetDefinitionScripts());
                }
                else if (file.Name.EndsWith("_create_triggers.sql", StringComparison.InvariantCultureIgnoreCase))
                {
                    sbFile.AppendLine(database.GetTriggerScripts());
                }
                else if (file.Name.EndsWith("_create_fk.sql", StringComparison.InvariantCultureIgnoreCase))
                {
                    sbFile.AppendLine(database.GetFkScripts());
                }
                else if (file.Name.EndsWith("_insert_default_values.sql", StringComparison.InvariantCultureIgnoreCase))
                {
                    sbFile.AppendLine(database.GetInsertDefaultScripts());
                }
                else if (file.Name.EndsWith("_create_security_policies.sql", StringComparison.InvariantCultureIgnoreCase))
                {
                    sbFile.AppendLine(database.GetSecurityPolicyScripts());
                }

                    sbFile.AppendLine("-- END GENERATED SQL");

                if (sbPostGenerated.Length > 0)
                {
                    sbFile.AppendLine(sbPostGenerated.ToString());
                }

                System.IO.File.WriteAllText(file.FullName, sbFile.ToString().TrimEnd());


            }

            public static void ExecuteScripts(System.Configuration.ConnectionStringSettings connectionString, IList<System.IO.FileInfo> fileList, bool continueOnError, IProgress<ScriptProgress> progress)
            {
                int intFileCount = 1;

                foreach (System.IO.FileInfo file in fileList)
                {
                    string strFile = string.Empty;

                    if (progress != null)
                    {
                        int percent = Convert.ToInt32((intFileCount / (double)fileList.Count) * 100);
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
                            Database.ExecuteFile(connectionString, strFile);
                        }
                        catch (Exception ex)
                        {
                            if (continueOnError)
                            {
                                if (progress != null)
                                {
                                    int percent = Convert.ToInt32((intFileCount / (double)fileList.Count) * 100);
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

            public static string MergeScripts(IList<string> scripts)
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

            public static void MergeScripts(IList<string> scripts, string toFile)
            {
                var strFile = MergeScripts(scripts);

                System.IO.File.WriteAllText(toFile, strFile);
            }

        }
    }


}