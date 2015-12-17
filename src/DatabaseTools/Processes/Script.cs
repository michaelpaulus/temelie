
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

            public static void CreateScripts(System.Configuration.ConnectionStringSettings connectionString, IList<System.IO.FileInfo> fileList, Models.DatabaseType targetDatabaseType, IProgress<ScriptProgress> progress, string objectFilter = "")
            {
                int intFileCount = 1;

                int intProgress = 0;

                fileList = (from i in fileList orderby i.FullName select i).ToList();

                Models.DatabaseModel database = new Models.DatabaseModel(connectionString, targetDatabaseType) { ObjectFilter = objectFilter };

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
                    sbFile.AppendLine(database.GetSpFnVwDropScripts());
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
                    sbFile.AppendLine(database.GetSpFnVwScripts());
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

                sbFile.AppendLine("-- END GENERATED SQL");

                if (sbPostGenerated.Length > 0)
                {
                    sbFile.AppendLine(sbPostGenerated.ToString());
                }

                System.IO.File.WriteAllText(file.FullName, sbFile.ToString().TrimEnd());


            }

            public static void ExecuteScripts(System.Configuration.ConnectionStringSettings connectionString, IList<System.IO.FileInfo> fileList, IProgress<ScriptProgress> progress)
            {
                int intFileCount = 1;

                foreach (System.IO.FileInfo file in fileList)
                {
                    string strFile = string.Empty;

                    if (progress != null)
                    {
                        progress.Report( new ScriptProgress() { ProgressPercentage = 0, ProgressStatus = file.Name });
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
                        Database.ExecuteFile(connectionString, strFile);
                    }

                    if (progress != null)
                    {
                        int percent = Convert.ToInt32((intFileCount / (double)fileList.Count) * 100);
                        progress.Report(new ScriptProgress() { ProgressPercentage = percent, ProgressStatus = file.Name });
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