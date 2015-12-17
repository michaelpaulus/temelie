using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseTools.Processes
{
    public class TableConverter
    {


        public void ConvertTables(TableConverterSettings settings,
            IProgress<TableProgress> progress,
            int maxDegreeOfParallelism)
        {

            var tables = settings.SourceTables.OrderBy(i => i.TableName);

            var options = new ParallelOptions();
            options.MaxDegreeOfParallelism = maxDegreeOfParallelism;

            Parallel.ForEach(tables, options, (sourceTable) =>
            {
                this.ConvertTable(settings, sourceTable, progress);
            });
        }

        public void ConvertTable(TableConverterSettings settings, Models.TableModel sourceTable, IProgress<TableProgress> progress)
        {

            var targetTable = (
                       from i in settings.TargetTables
                       where i.TableName.Equals(sourceTable.TableName, StringComparison.InvariantCultureIgnoreCase)
                       select i).FirstOrDefault();

            if (targetTable != null)
            {
                try
                {
                    if (settings.UseBulkCopy)
                    {
                        this.ConvertBulk(progress, sourceTable, settings.SourceConnectionString, targetTable, settings.TargetConnectionString);
                    }
                    else
                    {
                        this.Convert(progress, sourceTable, settings.SourceConnectionString, targetTable, settings.TargetConnectionString, settings.TrimStrings);
                    }

                }
                catch (Exception ex)
                {
                    string strException = string.Empty;
                    if (!(ex.Message.Equals("ERROR [00000] [QODBC] Error: 3250 - This feature is not enabled or not available in this version of QuickBooks.")))
                    {
                        strException = ex.ToString();
                    }
                    progress.Report(new TableProgress() { ProgressPercentage = 100, Table = sourceTable, ErrorMessage = strException });
                }
            }
        }

        public void ConvertBulk(IProgress<TableProgress> progress,
            Models.TableModel sourceTable,
            System.Configuration.ConnectionStringSettings sourceConnectionString,
            Models.TableModel targetTable,
            System.Configuration.ConnectionStringSettings targetConnectionString)
        {
            System.Data.Common.DbProviderFactory sourceFactory = DatabaseTools.Processes.Database.CreateDbProviderFactory(sourceConnectionString);
            System.Data.Common.DbProviderFactory targetFactory = DatabaseTools.Processes.Database.CreateDbProviderFactory(targetConnectionString);

            var sourceDatabaseType = DatabaseTools.Processes.Database.GetDatabaseType(sourceConnectionString);
            var targetDatabaseType = DatabaseTools.Processes.Database.GetDatabaseType(targetConnectionString);

            int intProgress = 0;

            using (System.Data.Common.DbConnection targetConnection = DatabaseTools.Processes.Database.CreateDbConnection(targetFactory, targetConnectionString))
            {

                var intTargetRowCount = this.GetRowCount(targetConnection, targetTable.TableName, targetDatabaseType);

                if (intTargetRowCount == 0)
                {
                    using (System.Data.Common.DbConnection sourceConnection = DatabaseTools.Processes.Database.CreateDbConnection(sourceFactory, sourceConnectionString))
                    {

                        var intSourceRowCount = this.GetRowCount(sourceConnection, sourceTable.TableName, sourceDatabaseType);

                        if (intSourceRowCount > 0)
                        {
                            using (System.Data.SqlClient.SqlBulkCopy bcp = new System.Data.SqlClient.SqlBulkCopy((System.Data.SqlClient.SqlConnection)targetConnection))
                            {
                                bcp.DestinationTableName = targetTable.TableName;
                                bcp.BatchSize = 1000;
                                bcp.BulkCopyTimeout = 600;
                                bcp.NotifyAfter = bcp.BatchSize;

                                long intRowIndex = 0L;


                                bcp.SqlRowsCopied += (object sender, System.Data.SqlClient.SqlRowsCopiedEventArgs e) =>
                                {
                                    intRowIndex += 1L;

                                    int intNewProgress = System.Convert.ToInt32(intRowIndex / (double)intSourceRowCount * 100);

                                    if (intProgress != intNewProgress)
                                    {
                                        intProgress = intNewProgress;
                                        progress.Report(new TableProgress() { ProgressPercentage = intProgress, Table = sourceTable });
                                    }
                                };

                                using (var command = DatabaseTools.Processes.Database.CreateDbCommand(sourceConnection))
                                {
                                    command.CommandText = this.FormatCommandText(string.Format("SELECT * FROM [{0}]", sourceTable.TableName), sourceDatabaseType);
                                    using (var reader = command.ExecuteReader())
                                    {
                                        bcp.WriteToServer(reader);
                                    }
                                }
                            }
                        }


                    }
                }
            }

            if (intProgress != 100)
            {
                progress.Report(new TableProgress() { ProgressPercentage = 100, Table = sourceTable });
            }

        }

        public void Convert(IProgress<TableProgress> progress,
            Models.TableModel sourceTable,
            System.Configuration.ConnectionStringSettings sourceConnectionString,
            Models.TableModel targetTable,
            System.Configuration.ConnectionStringSettings targetConnectionString,
            bool trimStrings)
        {
            System.Data.Common.DbProviderFactory sourceFactory = DatabaseTools.Processes.Database.CreateDbProviderFactory(sourceConnectionString);
            System.Data.Common.DbProviderFactory targetFactory = DatabaseTools.Processes.Database.CreateDbProviderFactory(targetConnectionString);

            var sourceDatabaseType = DatabaseTools.Processes.Database.GetDatabaseType(sourceConnectionString);
            var targetDatabaseType = DatabaseTools.Processes.Database.GetDatabaseType(targetConnectionString);

            int? intProgress = null;

            var intTargetRowCount = 0;

            using (System.Data.Common.DbConnection targetConnection = DatabaseTools.Processes.Database.CreateDbConnection(targetFactory, targetConnectionString))
            {
                intTargetRowCount = this.GetRowCount(targetConnection, targetTable.TableName, targetDatabaseType);
            }

            if (intTargetRowCount == 0)
            {
                System.Text.StringBuilder sbColumns = new System.Text.StringBuilder();
                System.Text.StringBuilder sbParamaters = new System.Text.StringBuilder();

                bool blnContainsIdentity = false;

                var sourceMatchedColumns = this.GetMatchedColumns(sourceTable.Columns, targetTable.Columns);
                var targetMatchedColumns = this.GetMatchedColumns(targetTable.Columns, sourceTable.Columns);

                var sourceValues = this.GetTableValues(sourceFactory, sourceConnectionString, sourceTable, sourceMatchedColumns);

                using (System.Data.Common.DbConnection targetConnection = DatabaseTools.Processes.Database.CreateDbConnection(targetFactory, targetConnectionString))
                {
                    using (var targetCommand = DatabaseTools.Processes.Database.CreateDbCommand(targetConnection))
                    {
                        foreach (DatabaseTools.Models.ColumnModel targetColumn in targetMatchedColumns)
                        {
                            if (targetColumn.IsIdentity)
                            {
                                blnContainsIdentity = true;
                            }

                            if (sbColumns.Length > 0)
                            {
                                sbColumns.Append(", ");
                            }
                            sbColumns.AppendFormat("[{0}]", targetColumn.ColumnName);
                            if (sbParamaters.Length > 0)
                            {
                                sbParamaters.Append(", ");
                            }

                            System.Data.Common.DbParameter paramater = targetFactory.CreateParameter();
                            paramater.ParameterName = string.Concat("@", this.GetParameterNameFromColumn(targetColumn.ColumnName));

                            paramater.DbType = targetColumn.DbType;

                            switch (paramater.DbType)
                            {
                                case System.Data.DbType.StringFixedLength:
                                case System.Data.DbType.String:
                                    paramater.Size = targetColumn.Precision;
                                    break;
                                case System.Data.DbType.Time:
                                    if ((paramater) is System.Data.SqlClient.SqlParameter)
                                    {
                                        ((System.Data.SqlClient.SqlParameter)paramater).SqlDbType = System.Data.SqlDbType.Time;
                                    }
                                    break;
                            }

                            sbParamaters.Append(paramater.ParameterName);

                            targetCommand.Parameters.Add(paramater);

                        }

                        targetCommand.CommandText = this.FormatCommandText(string.Format("INSERT INTO [{0}] ({1}) VALUES ({2})", targetTable.TableName, sbColumns.ToString(), sbParamaters.ToString()), targetDatabaseType);

                        if (blnContainsIdentity)
                        {
                            targetCommand.CommandText = this.FormatCommandText(string.Format("SET IDENTITY_INSERT [{0}] ON;" + Environment.NewLine + targetCommand.CommandText + Environment.NewLine + "SET IDENTITY_INSERT [{0}] OFF;", targetTable.TableName), targetDatabaseType);
                        }

                        int intRowCount = sourceValues.Count;

                        int intRowIndex = 0;

                        if (intRowCount > 0)
                        {

                            foreach (var row in sourceValues)
                            {

                                for (int intIndex = 0; intIndex < targetCommand.Parameters.Count; intIndex++)
                                {
                                    var parameter = targetCommand.Parameters[intIndex];

                                    object objValue = row[intIndex];
                                   
                                    if (System.Convert.IsDBNull(objValue))
                                    {
                                        parameter.Value = DBNull.Value;
                                    }
                                    else
                                    {
                                        var targetColumn = targetMatchedColumns[intIndex];

                                        switch (targetColumn.DbType)
                                        {
                                            case System.Data.DbType.Date:
                                            case System.Data.DbType.DateTime:

                                                try
                                                {
                                                    DateTime dt = System.Convert.ToDateTime(objValue);

                                                    if (dt <= new DateTime(1753, 1, 1))
                                                    {
                                                        parameter.Value = new DateTime(1753, 1, 1);
                                                    }
                                                    else if (dt > new DateTime(9999, 12, 31))
                                                    {
                                                        parameter.Value = new DateTime(9999, 12, 31);
                                                    }
                                                    else
                                                    {
                                                        parameter.Value = dt;
                                                    }
                                                }
                                                catch
                                                {
                                                    parameter.Value = new DateTime(1753, 1, 1);
                                                }
                                                break;
                                            case System.Data.DbType.DateTime2:
                                                try
                                                {
                                                    DateTime dt = System.Convert.ToDateTime(objValue);
                                                    parameter.Value = dt;
                                                }
                                                catch
                                                {
                                                    parameter.Value = DateTime.MinValue;
                                                }
                                                break;
                                            case System.Data.DbType.Time:
                                                {
                                                    if ((objValue) is TimeSpan)
                                                    {
                                                        parameter.Value = (new DateTime(1753, 1, 1)).Add((TimeSpan)objValue);
                                                    }
                                                    else if ((objValue) is DateTime)
                                                    {
                                                        DateTime dt = System.Convert.ToDateTime(objValue);

                                                        if (dt.Year <= 300 && dt.Year >= 200)
                                                        {
                                                            int newYear = dt.Year * 10;
                                                            dt = dt.AddYears(newYear - dt.Year);
                                                        }

                                                        if (dt <= new DateTime(1753, 1, 1))
                                                        {
                                                            parameter.Value = DBNull.Value;
                                                        }
                                                        else if (dt > new DateTime(9999, 12, 31))
                                                        {
                                                            parameter.Value = DBNull.Value;
                                                        }
                                                        else
                                                        {
                                                            parameter.Value = dt;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        parameter.Value = objValue;
                                                    }
                                                    break;
                                                }
                                            case System.Data.DbType.AnsiString:
                                            case System.Data.DbType.AnsiStringFixedLength:
                                            case System.Data.DbType.String:
                                            case System.Data.DbType.StringFixedLength:
                                                {
                                                    if (trimStrings)
                                                    {
                                                        parameter.Value = System.Convert.ToString(objValue).TrimEnd();
                                                    }
                                                    else
                                                    {
                                                        parameter.Value = System.Convert.ToString(objValue);
                                                    }
                                                    break;
                                                }
                                            default:
                                                {
                                                    parameter.Value = objValue;
                                                    break;
                                                }
                                        }
                                    }

                                }

                                intRowIndex += 1;

                                int intNewProgress = System.Convert.ToInt32(intRowIndex / (double)intRowCount * 100);

                                try
                                {
                                    targetCommand.ExecuteNonQuery();

                                    if (!intProgress.HasValue)
                                    {
                                        progress.Report(new TableProgress() { ProgressPercentage = 0, Table = sourceTable });
                                    }

                                    if (intProgress.GetValueOrDefault() != intNewProgress)
                                    {
                                        intProgress = intNewProgress;
                                        progress.Report(new TableProgress() { ProgressPercentage = intNewProgress, Table = sourceTable });
                                    }

                                }
                                catch (Exception ex)
                                {
                                    string strRowErrorMessage = this.GetRowErrorMessage(targetCommand, targetMatchedColumns, targetCommand.Parameters.Count - 1, ex);

                                    string strErrorMessage = string.Format("could not insert row on table: {0} at row: {1}", sourceTable.TableName, strRowErrorMessage);

                                    progress.Report(new TableProgress() { ProgressPercentage = intNewProgress, Table = sourceTable, ErrorMessage = strErrorMessage });

                                }

                                intProgress = intNewProgress;

                            }

                        }

                    }
                }
            }


            if (intProgress != 100)
            {
                progress.Report(new TableProgress() { ProgressPercentage = 100, Table = sourceTable });
            }
        }

        //private System.Data.DataTable ConvertToDataTable(IList<object[]> sourceValues, Models.TableModel targetTable, IList<Models.ColumnModel> targetColumns)
        //{
        //    var dataTable = new System.Data.DataTable(targetTable.TableName);
        //    foreach (var targetColumn in targetColumns)
        //    {
        //        var column = new System.Data.DataColumn(targetColumn.ColumnName);
        //        column.AutoIncrement = targetColumn.IsIdentity;
        //        column.AllowDBNull = targetColumn.IsNullable;
        //        column.MaxLength = targetColumn.Precision;

        //        dataTable.Columns.Add(column.ColumnName , column.type)
        //    }
        //}

        private IList<object[]> GetTableValues(System.Data.Common.DbProviderFactory sourceFactory, System.Configuration.ConnectionStringSettings sourceConnectionString, Models.TableModel sourceTable, IList<Models.ColumnModel> sourceColumns)
        {
            var list = new List<object[]>();

            var sourceDatabaseType = DatabaseTools.Processes.Database.GetDatabaseType(sourceConnectionString);

            System.Text.StringBuilder sbColumns = new System.Text.StringBuilder();

            foreach (var sourceColumn in sourceColumns)
            {
                if (sbColumns.Length > 0)
                {
                    sbColumns.Append(", ");
                }
                sbColumns.AppendFormat("[{0}]", sourceColumn.ColumnName);
            }

            using (System.Data.Common.DbConnection sourceConnection = DatabaseTools.Processes.Database.CreateDbConnection(sourceFactory, sourceConnectionString))
            {

                using (System.Data.Common.DbCommand sourceCommand = DatabaseTools.Processes.Database.CreateDbCommand(sourceConnection))
                {
                    var intRowCount = this.GetRowCount(sourceConnection, sourceTable.TableName, sourceDatabaseType);

                    this.SetReadTimeout(sourceCommand);

                    int intRowIndex = 0;

                    if (intRowCount > 0)
                    {
                        sourceCommand.CommandText = this.FormatCommandText(string.Format("SELECT {0} FROM [{1}]", sbColumns.ToString(), sourceTable.TableName), sourceDatabaseType);

                        using (System.Data.Common.DbDataReader sourceReader = sourceCommand.ExecuteReader())
                        {

                            var intFieldCount = sourceReader.FieldCount;

                            while (sourceReader.Read())
                            {
                                var values = new object[intFieldCount];

                                for (int intIndex = 0; intIndex < intFieldCount; intIndex++)
                                {

                                    var sourceColumn = sourceTable.Columns[intIndex];

                                    try
                                    {
                                        values[intIndex] = sourceReader.GetValue(intIndex);
                                    }
#pragma warning disable CS0168 // Variable is declared but never used
                                    catch (MySql.Data.Types.MySqlConversionException ex)
#pragma warning restore CS0168 // Variable is declared but never used
                                    {
                                        if (sourceColumn.DbType == System.Data.DbType.DateTime2 ||
                                            sourceColumn.DbType == System.Data.DbType.Date ||
                                            sourceColumn.DbType == System.Data.DbType.DateTime ||
                                            sourceColumn.DbType == System.Data.DbType.DateTimeOffset)
                                        {
                                            values[intIndex] = DateTime.MinValue;
                                        }
                                        else
                                        {
                                            throw;
                                        }
                                    }

                                }

                                list.Add(values);

                                intRowIndex += 1;

                            }
                        }
                    }
                }
            }

            return list;
        }

        private IList<Models.ColumnModel> GetMatchedColumns(IList<Models.ColumnModel> sourceColumns, IList<Models.ColumnModel> targetColumns)
        {
            var list = new List<Models.ColumnModel>();

            foreach (DatabaseTools.Models.ColumnModel sourceColumn in sourceColumns)
            {
                string strColumnName = sourceColumn.ColumnName;
                DatabaseTools.Models.ColumnModel targetColumn = (
                    from c in targetColumns
                    where c.ColumnName.Equals(strColumnName, StringComparison.InvariantCultureIgnoreCase)
                    select c).FirstOrDefault();

                if (targetColumn != null && !targetColumn.IsComputed)
                {

                    list.Add(sourceColumn);
                }
            }

            return list;
        }

        private void SetReadTimeout(System.Data.Common.DbCommand sourceCommand)
        {
            var command = sourceCommand as MySql.Data.MySqlClient.MySqlCommand;
            if (command != null)
            {
                command.CommandText = "set net_write_timeout=99999;set net_read_timeout=99999;";
                command.ExecuteNonQuery();
            }
        }

        private string FormatCommandText(string commandText, DatabaseTools.Models.DatabaseType databaseType)
        {
            switch (databaseType)
            {
                case Models.DatabaseType.Odbc:
                    commandText = commandText.Replace("[", "\"").Replace("]", "\"");
                    break;
                case Models.DatabaseType.OLE:
                    commandText = commandText.Replace("[", "\"").Replace("]", "\"");
                    break;
                case Models.DatabaseType.AccessOLE:
                    //Do nothing, access likes brackets
                    break;
                case Models.DatabaseType.MySql:
                    commandText = commandText.Replace("[", "`").Replace("]", "`");
                    break;
            }
            return commandText;
        }

        private int GetRowCount(System.Data.Common.DbConnection connection, string tableName, DatabaseTools.Models.DatabaseType databaseType)
        {
            int rowCount = 0;

            try
            {
                using (var command = DatabaseTools.Processes.Database.CreateDbCommand(connection))
                {

                    switch (databaseType)
                    {
                        case DatabaseTools.Models.DatabaseType.MicrosoftSQLServer:
                            command.CommandText = string.Format("(SELECT sys.sysindexes.rows FROM sys.tables INNER JOIN sys.sysindexes ON sys.tables.object_id = sys.sysindexes.id AND sys.sysindexes.indid < 2 WHERE sys.tables.name = '{0}')", tableName);
                            break;
                        default:
                            command.CommandText = this.FormatCommandText(string.Format("SELECT COUNT(1) FROM [{0}]", tableName), databaseType);
                            break;
                    }
                    rowCount = System.Convert.ToInt32(command.ExecuteScalar());
                }
            }
            catch
            {

            }

            return rowCount;
        }

        private string GetParameterNameFromColumn(string columnName)
        {
            return columnName.Replace("-", "").Replace(" ", "");
        }

        private string GetRowErrorMessage(System.Data.Common.DbCommand command, IList<DatabaseTools.Models.ColumnModel> columns, int columnIndex, System.Exception ex)
        {
            System.Text.StringBuilder sbRow = new System.Text.StringBuilder();

            for (int intErrorIndex = 0; intErrorIndex < columnIndex; intErrorIndex++)
            {
                DatabaseTools.Models.ColumnModel targetColumn = columns[intErrorIndex];
                if (targetColumn != null)
                {
                    if (!targetColumn.IsNullable)
                    {
                        string strColumnName = targetColumn.ColumnName;
                        object objTargetValue = command.Parameters[intErrorIndex].Value;
                        if (objTargetValue == null || objTargetValue == DBNull.Value)
                        {
                            objTargetValue = "NULL";
                        }
                        if (sbRow.Length > 0)
                        {
                            sbRow.Append(" AND ");
                        }
                        sbRow.AppendFormat(" {0} = '{1}'", strColumnName, System.Convert.ToString(objTargetValue));
                    }
                }
            }
            sbRow.AppendLine();

            sbRow.AppendLine("ERROR:");
            sbRow.AppendLine(ex.Message);

            return sbRow.ToString();
        }


    }
}
