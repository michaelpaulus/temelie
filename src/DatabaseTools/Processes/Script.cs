
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

			public static void CreateScripts(System.Configuration.ConnectionStringSettings connectionString, List<System.IO.FileInfo> fileList, System.ComponentModel.BackgroundWorker worker, Version version, string objectFilter = "")
			{
				int intFileCount = 1;

				int intProgress = 0;

				Models.DatabaseModel database = new Models.DatabaseModel(connectionString) {ObjectFilter = objectFilter};

				string strQuoteCharacter = "";

				switch (Processes.Database.GetDatabaseType(connectionString))
				{
					case Models.DatabaseType.AccessOLE:
					case Models.DatabaseType.OLE:
					case Models.DatabaseType.Odbc:
						strQuoteCharacter = "\"";
						break;
                    case Models.DatabaseType.MySql:
                        strQuoteCharacter = "\"";
                        break;
                    case Models.DatabaseType.MicrosoftSQLServer:
                        strQuoteCharacter = "\"";
                        break;
                }

                foreach (System.IO.FileInfo file in fileList)
				{
					bool blnSkipFile = false;

					if (file.Name.EndsWith("_post_mods.sql", StringComparison.InvariantCultureIgnoreCase) && version == null)
					{
						blnSkipFile = true;
					}
					else if (file.Name.EndsWith("_merge.sql", StringComparison.InvariantCultureIgnoreCase))
					{
						blnSkipFile = true;
					}

					if (!blnSkipFile)
					{

						System.Text.StringBuilder sbFile = new System.Text.StringBuilder();

						System.Text.StringBuilder sbPostGenerated = new System.Text.StringBuilder();

						if (worker != null)
						{
							worker.ReportProgress(intProgress, file.Name);
						}

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
								sbFile.AppendLine(database.GetSpFnVwDropScripts(strQuoteCharacter));
								sbFile.AppendLine(database.GetTriggerDropScripts(strQuoteCharacter));
								sbFile.AppendLine(database.GetFkDropScripts(strQuoteCharacter));
						}
						else if (file.Name.EndsWith("_create_tables.sql", StringComparison.InvariantCultureIgnoreCase))
						{
								sbFile.AppendLine(database.GetTableScripts(strQuoteCharacter));
						}
						else if (file.Name.EndsWith("_create_ix_pk.sql", StringComparison.InvariantCultureIgnoreCase))
						{
								sbFile.AppendLine(database.GetIxPkScripts(strQuoteCharacter));
						}
						else if (file.Name.EndsWith("_create_sp_fn_vw.sql", StringComparison.InvariantCultureIgnoreCase))
						{
								sbFile.AppendLine(database.GetSpFnVwScripts(strQuoteCharacter));
						}
						else if (file.Name.EndsWith("_create_triggers.sql", StringComparison.InvariantCultureIgnoreCase))
						{
								sbFile.AppendLine(database.GetTriggerScripts(strQuoteCharacter));
						}
						else if (file.Name.EndsWith("_create_fk.sql", StringComparison.InvariantCultureIgnoreCase))
						{
								sbFile.AppendLine(database.GetFkScripts(strQuoteCharacter));
						}
						else if (file.Name.EndsWith("_insert_default_values.sql", StringComparison.InvariantCultureIgnoreCase))
						{
								if (Processes.Database.GetDatabaseType(connectionString) == Models.DatabaseType.MicrosoftSQLServer)
								{
									sbFile.AppendLine(database.GetInsertDefaultScripts(strQuoteCharacter));
								}
						}
						else if (file.Name.EndsWith("_post_mods.sql", StringComparison.InvariantCultureIgnoreCase))
						{
								sbFile.AppendLine(string.Format("IF NOT EXISTS (SELECT 1 FROM versions WHERE version = '{0}')", version));
								sbFile.AppendLine("	INSERT INTO");
								sbFile.AppendLine("		versions");
								sbFile.AppendLine("		(");
								sbFile.AppendLine("			version, version_major, version_minor, version_build, version_revision,");
								sbFile.AppendLine("			changed_date, changed_user_id");
								sbFile.AppendLine("		)");
								sbFile.AppendLine("		VALUES");
								sbFile.AppendLine("		(");
								sbFile.AppendLine(string.Format("			'{0}.{1}.{2}.{3}', {0}, {1}, {2}, {3},", version.Major, version.Minor, version.Build, version.Revision));
								sbFile.AppendLine("			GETDATE(), 'SYSTEM'");
								sbFile.AppendLine("		)");
								sbFile.AppendLine("GO");
						}

						sbFile.AppendLine("-- END GENERATED SQL");

						if (sbPostGenerated.Length > 0)
						{
							sbFile.AppendLine(sbPostGenerated.ToString());
						}

						System.IO.File.WriteAllText(file.FullName, sbFile.ToString().TrimEnd());
					}

					if (worker != null)
					{
						intProgress = Convert.ToInt32((intFileCount / (double)fileList.Count) * 100);
						worker.ReportProgress(intProgress, file.Name);
					}

					intFileCount += 1;
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

			public static void ExecuteScripts(System.Configuration.ConnectionStringSettings connectionString, List<System.IO.FileInfo> fileList, System.ComponentModel.BackgroundWorker worker)
			{
				int intFileCount = 1;

				foreach (System.IO.FileInfo file in fileList)
				{
					string strFile = string.Empty;

					if (worker != null)
					{
						worker.ReportProgress(0, file.Name);
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

					if (worker != null)
					{
						worker.ReportProgress(Convert.ToInt32((intFileCount / (double)fileList.Count) * 100), file.Name);
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