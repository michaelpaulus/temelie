using System.Configuration;
using System.Data;
using System.Data.Common;
using System.Text;
using Cornerstone.Database.Providers;

namespace Cornerstone.Database.Services;

public class TableConverterService
{

    private readonly IEnumerable<IDatabaseProvider> _databaseProviders;
    private readonly IEnumerable<IConnectionCreatedNotification> _connectionCreatedNotifications;

    public TableConverterService(IEnumerable<IDatabaseProvider> databaseProviders, IEnumerable<IConnectionCreatedNotification> connectionCreatedNotifications)
    {
        _databaseProviders = databaseProviders;
        _connectionCreatedNotifications = connectionCreatedNotifications;
    }

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
                   where
                   i.SchemaName.Equals(sourceTable.SchemaName, StringComparison.InvariantCultureIgnoreCase) &&
                   i.TableName.Equals(sourceTable.TableName, StringComparison.InvariantCultureIgnoreCase)
                   select i).FirstOrDefault();

        if (targetTable == null && sourceTable.SchemaName == "dbo")
        {
            targetTable = (
                 from i in settings.TargetTables
                 where
                 i.TableName.Equals(sourceTable.TableName, StringComparison.InvariantCultureIgnoreCase)
                 select i).FirstOrDefault();
        }

        ConvertTable(settings, sourceTable, targetTable, progress);
    }

    public void ConvertTable(TableConverterSettings settings, Models.TableModel sourceTable, Models.TableModel targetTable, IProgress<TableProgress> progress, bool throwOnFailure = false)
    {
        if (targetTable != null)
        {
            try
            {

                if (settings.UseBulkCopy)
                {
                    this.ConvertBulk(progress, sourceTable, settings.SourceConnectionString, targetTable, settings.TargetConnectionString, settings.TrimStrings, settings.BatchSize, settings.UseTransaction);
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

                if (progress == null || throwOnFailure)
                {
                    if (ex as TableConverterException == null)
                    {
                        throw;
                    }
                    else
                    {
                        throw new TableConverterException(sourceTable, strException, ex);
                    }
                }
                else
                {
                    progress.Report(new TableProgress() { ProgressPercentage = 100, Table = sourceTable, ErrorMessage = strException, Exception = ex });
                }

            }
        }
    }

    public void ConvertBulk(IProgress<TableProgress> progress,
      Models.TableModel sourceTable,
      IDataReader sourceReader,
      int sourceRowCount,
      Models.TableModel targetTable,
      ConnectionStringSettings targetConnectionString,
      bool trimStrings,
      int batchSize,
      bool useTransaction = true,
      bool validateTargetTable = true)
    {
        var targetDatabase = new DatabaseService(DatabaseService.GetDatabaseType(targetConnectionString), _databaseProviders, _connectionCreatedNotifications);
        using (System.Data.Common.DbConnection targetConnection = targetDatabase.CreateDbConnection(targetConnectionString))
        {
            targetDatabase.Provider.ConvertBulk(this, progress, sourceTable, sourceReader, sourceRowCount, targetTable, targetConnection, trimStrings, batchSize, useTransaction, validateTargetTable);
        }
    }

    private DbCommand CreateSourceCommand(Models.TableModel sourceTable, Models.TableModel targetTable, System.Data.Common.DbConnection sourceConnection)
    {
        var sourceDatabaseType = Cornerstone.Database.Services.DatabaseService.GetDatabaseType(sourceConnection);

        var sourceDatabase = new DatabaseService(sourceDatabaseType, _databaseProviders, _connectionCreatedNotifications);

        StringBuilder sbColumns = new System.Text.StringBuilder();

        var sourceMatchedColumns = this.GetMatchedColumns(sourceTable.Columns, targetTable.Columns);

        foreach (var sourceColumn in sourceMatchedColumns)
        {
            if (sbColumns.Length > 0)
            {
                sbColumns.Append(", ");
            }
            sbColumns.AppendFormat("[{0}]", sourceColumn.ColumnName);
        }

        using (var command = sourceDatabase.CreateDbCommand(sourceConnection))
        {
            this.SetReadTimeout(sourceDatabaseType, command);
            command.CommandText = this.FormatCommandText($"SELECT {sbColumns.ToString()} FROM [{sourceTable.SchemaName}].[{sourceTable.TableName}]", sourceDatabaseType);
            return command;
        }

    }

    public void ConvertBulk(IProgress<TableProgress> progress,
        Models.TableModel sourceTable,
        System.Configuration.ConnectionStringSettings sourceConnectionString,
        Models.TableModel targetTable,
        System.Configuration.ConnectionStringSettings targetConnectionString,
        bool trimStrings,
        int batchSize,
        bool useTransaction = true)
    {
        progress?.Report(new TableProgress() { ProgressPercentage = 0, Table = sourceTable });

        var sourceDatabaseType = DatabaseService.GetDatabaseType(sourceConnectionString);
        var targetDatabaseType = DatabaseService.GetDatabaseType(targetConnectionString);

        var sourceDatabase = new DatabaseService(sourceDatabaseType, _databaseProviders, _connectionCreatedNotifications);
        var targetDatabase = new DatabaseService(targetDatabaseType, _databaseProviders, _connectionCreatedNotifications);

        using (var targetConnection = targetDatabase.CreateDbConnection(targetConnectionString))
        {
            var targetRowCount = GetRowCount(targetTable, targetConnection);
            if (targetRowCount != 0)
            {
                progress?.Report(new TableProgress() { ProgressPercentage = 100, Table = sourceTable });
                return;
            }
        }

        int? intSourceRowCount = null;

        if (progress != null)
        {
            using (System.Data.Common.DbConnection sourceConnection = sourceDatabase.CreateDbConnection(sourceConnectionString))
            {
                intSourceRowCount = GetRowCount(sourceTable, sourceConnection);
            }
        }

        if (!intSourceRowCount.HasValue || intSourceRowCount.Value > 0)
        {
            using (System.Data.Common.DbConnection sourceConnection = sourceDatabase.CreateDbConnection(sourceConnectionString))
            {
                using (var command = CreateSourceCommand(sourceTable, targetTable, sourceConnection))
                {
                    using (var reader = command.ExecuteReader())
                    {
                        var sourceMatchedColumns = this.GetMatchedColumns(sourceTable.Columns, targetTable.Columns);
                        var targetMatchedColumns = this.GetMatchedColumns(targetTable.Columns, sourceTable.Columns);
                        var reader2 = new TableConverterReader(reader, sourceMatchedColumns, targetMatchedColumns, trimStrings, sourceDatabaseType, targetDatabaseType, _databaseProviders);
                        ConvertBulk(progress, sourceTable, reader2, intSourceRowCount.GetValueOrDefault(), targetTable, targetConnectionString, trimStrings, batchSize, useTransaction, false);
                    }
                }
            }
        }
    }

    public void Convert(IProgress<TableProgress> progress,
        Models.TableModel sourceTable,
        System.Configuration.ConnectionStringSettings sourceConnectionString,
        Models.TableModel targetTable,
        System.Configuration.ConnectionStringSettings targetConnectionString,
        bool trimStrings,
        Action<IDbConnection> connectionCreatedCallback = null)
    {
        progress?.Report(new TableProgress() { ProgressPercentage = 0, Table = sourceTable });

        var sourceDatabaseType = Cornerstone.Database.Services.DatabaseService.GetDatabaseType(sourceConnectionString);
        var targetDatabaseType = Cornerstone.Database.Services.DatabaseService.GetDatabaseType(targetConnectionString);

        var sourceDatabase = new DatabaseService(sourceDatabaseType, _databaseProviders, _connectionCreatedNotifications);
        var targetDatabase = new DatabaseService(targetDatabaseType, _databaseProviders, _connectionCreatedNotifications);

        int intProgress = 0;

        using (var targetConnection = targetDatabase.CreateDbConnection(targetConnectionString))
        {
            var targetRowCount = GetRowCount(targetTable, targetConnection);
            if (targetRowCount != 0)
            {
                progress?.Report(new TableProgress() { ProgressPercentage = 100, Table = sourceTable });
                return;
            }
        }

        bool blnContainsIdentity = false;

        var sourceMatchedColumns = this.GetMatchedColumns(sourceTable.Columns, targetTable.Columns);
        var targetMatchedColumns = this.GetMatchedColumns(targetTable.Columns, sourceTable.Columns);

        using (System.Data.Common.DbConnection sourceConnection = sourceDatabase.CreateDbConnection(sourceConnectionString))
        {

            connectionCreatedCallback?.Invoke(sourceConnection);

            using (System.Data.Common.DbCommand sourceCommand = sourceDatabase.CreateDbCommand(sourceConnection))
            {
                this.SetReadTimeout(sourceDatabaseType, sourceCommand);

                int? intSourceRowCount = null;

                if (progress != null)
                {
                    intSourceRowCount = this.GetRowCount(sourceConnection, sourceTable.SchemaName, sourceTable.TableName, sourceDatabaseType);
                }

                if (!intSourceRowCount.HasValue || intSourceRowCount.Value > 0)
                {

                    System.Text.StringBuilder sbSelectColumns = new System.Text.StringBuilder();

                    foreach (var sourceColumn in sourceMatchedColumns)
                    {
                        if (sbSelectColumns.Length > 0)
                        {
                            sbSelectColumns.Append(", ");
                        }
                        sbSelectColumns.AppendFormat("[{0}]", sourceColumn.ColumnName);
                    }

                    sourceCommand.CommandText = this.FormatCommandText($"SELECT {sbSelectColumns.ToString()} FROM [{sourceTable.SchemaName}].[{sourceTable.TableName}]", sourceDatabaseType);

                    using (System.Data.Common.DbDataReader sourceReader = sourceCommand.ExecuteReader())
                    {

                        var converterReader = new TableConverterReader(sourceReader, sourceMatchedColumns, targetMatchedColumns, trimStrings, sourceDatabaseType, targetDatabaseType, _databaseProviders);

                        var intFieldCount = converterReader.FieldCount;

                        using (System.Data.Common.DbConnection targetConnection = targetDatabase.CreateDbConnection(targetConnectionString))
                        {
                            using (var targetCommand = targetDatabase.CreateDbCommand(targetConnection))
                            {

                                System.Text.StringBuilder sbColumns = new System.Text.StringBuilder();
                                System.Text.StringBuilder sbParamaters = new System.Text.StringBuilder();

                                foreach (Cornerstone.Database.Models.ColumnModel targetColumn in targetMatchedColumns)
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

                                    System.Data.Common.DbParameter paramater = targetDatabase.Provider.CreateProvider().CreateParameter();
                                    paramater.ParameterName = string.Concat("@", this.GetParameterNameFromColumn(targetColumn.ColumnName));

                                    paramater.DbType = targetColumn.DbType;

                                    targetDatabase.Provider.UpdateParameter(paramater, targetColumn);

                                    sbParamaters.Append(paramater.ParameterName);

                                    targetCommand.Parameters.Add(paramater);

                                }

                                targetCommand.CommandText = this.FormatCommandText(string.Format("INSERT INTO [{0}].[{1}] ({2}) VALUES ({3})", targetTable.SchemaName, targetTable.TableName, sbColumns.ToString(), sbParamaters.ToString()), targetDatabaseType);

                                if (blnContainsIdentity)
                                {
                                    targetCommand.CommandText = this.FormatCommandText(string.Format("SET IDENTITY_INSERT [{0}].[{1}] ON;" + Environment.NewLine + targetCommand.CommandText + Environment.NewLine + "SET IDENTITY_INSERT [{0}].[{1}] OFF;", targetTable.SchemaName, targetTable.TableName), targetDatabaseType);
                                }

                                int rowIndex = 0;

                                while (converterReader.Read())
                                {

                                    for (int intIndex = 0; intIndex < targetCommand.Parameters.Count; intIndex++)
                                    {
                                        var parameter = targetCommand.Parameters[intIndex];
                                        parameter.Value = converterReader.GetValue(intIndex);
                                    }

                                    rowIndex += 1;

                                    int intNewProgress = intProgress;

                                    if (intSourceRowCount.HasValue)
                                    {
                                        intNewProgress = System.Convert.ToInt32(rowIndex / (double)intSourceRowCount.Value * 100);
                                    }

                                    try
                                    {
                                        targetCommand.ExecuteNonQuery();

                                        if (progress != null)
                                        {
                                            if (intProgress != intNewProgress &&
                                                intNewProgress != 100)
                                            {
                                                intProgress = intNewProgress;
                                                progress.Report(new TableProgress() { ProgressPercentage = intNewProgress, Table = sourceTable });
                                            }
                                        }

                                    }
                                    catch (Exception ex)
                                    {
                                        string strRowErrorMessage = this.GetRowErrorMessage(targetCommand, targetMatchedColumns, targetCommand.Parameters.Count - 1, ex);

                                        string strErrorMessage = string.Format("could not insert row on table: {0} at row: {1}", sourceTable.TableName, strRowErrorMessage);

                                        throw new TableConverterException(sourceTable, strErrorMessage, ex);
                                    }

                                    intProgress = intNewProgress;

                                }
                            }
                        }
                    }

                }
            }
        }

        if (progress != null &&
            intProgress != 100)
        {
            progress.Report(new TableProgress() { ProgressPercentage = 100, Table = sourceTable });
        }

    }

    private object GetColumnValue(Models.ColumnModel targetColumn, object value, bool trimStrings)
    {
        object returnValue = value;

        if (value != DBNull.Value)
        {
            var dbType = targetColumn.DbType;

            switch (dbType)
            {
                case System.Data.DbType.Date:
                case System.Data.DbType.DateTime:
                    try
                    {
                        DateTime dt = System.Convert.ToDateTime(value);

                        if (dt <= new DateTime(1753, 1, 1))
                        {
                            returnValue = new DateTime(1753, 1, 1);
                        }
                        else if (dt > new DateTime(9999, 12, 31))
                        {
                            returnValue = new DateTime(9999, 12, 31);
                        }
                        else
                        {
                            returnValue = dt;
                        }
                    }
                    catch
                    {
                        returnValue = new DateTime(1753, 1, 1);
                    }
                    break;
                case System.Data.DbType.DateTime2:
                    try
                    {
                        DateTime dt = System.Convert.ToDateTime(value);
                        returnValue = dt;
                    }
                    catch
                    {
                        returnValue = DateTime.MinValue;
                    }
                    break;
                case System.Data.DbType.Time:
                    {
                        if ((value) is TimeSpan)
                        {
                            returnValue = (new DateTime(1753, 1, 1)).Add((TimeSpan)value);
                        }
                        else if ((value) is DateTime)
                        {
                            DateTime dt = System.Convert.ToDateTime(value);

                            if (dt <= new DateTime(1753, 1, 1))
                            {
                                returnValue = DBNull.Value;
                            }
                            else if (dt > new DateTime(9999, 12, 31))
                            {
                                returnValue = DBNull.Value;
                            }
                            else
                            {
                                returnValue = dt;
                            }
                        }
                        else
                        {
                            returnValue = value;
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
                            returnValue = System.Convert.ToString(value).TrimEnd();
                        }
                        else
                        {
                            returnValue = System.Convert.ToString(value);
                        }
                        break;
                    }
            }
        }

        return returnValue;
    }

    private DataTable GetTableValues(System.Configuration.ConnectionStringSettings sourceConnectionString, Models.TableModel sourceTable, Models.TableModel targetTable, bool trimStrings,
        int take, int skip)
    {
        var sourceDatabaseType = Cornerstone.Database.Services.DatabaseService.GetDatabaseType(sourceConnectionString);

        var sourceDatabase = new DatabaseService(sourceDatabaseType, _databaseProviders, _connectionCreatedNotifications);

        System.Text.StringBuilder sbColumns = new System.Text.StringBuilder();
        System.Text.StringBuilder sbKeyColumns = new System.Text.StringBuilder();

        var sourceMatchedColumns = this.GetMatchedColumns(sourceTable.Columns, targetTable.Columns);
        var targetMatchedColumns = this.GetMatchedColumns(targetTable.Columns, sourceTable.Columns);

        foreach (var sourceColumn in sourceMatchedColumns)
        {
            if (sbColumns.Length > 0)
            {
                sbColumns.Append(", ");
            }
            sbColumns.AppendFormat("[{0}]", sourceColumn.ColumnName);
            if (sourceColumn.IsPrimaryKey)
            {
                if (sbKeyColumns.Length > 0)
                {
                    sbKeyColumns.Append(", ");
                }
                sbKeyColumns.AppendFormat("[{0}]", sourceColumn.ColumnName);
            }
        }

        var dataTable = new System.Data.DataTable($"[{targetTable.TableName}]");

        foreach (var targetColumn in targetMatchedColumns)
        {
            var column = new System.Data.DataColumn($"[{targetColumn.ColumnName}]");
            column.AutoIncrement = targetColumn.IsIdentity;
            column.AllowDBNull = targetColumn.IsNullable;

            column.DataType = DatabaseService.GetSystemType(targetColumn.DbType);

            if (column.DataType == typeof(string))
            {
                column.MaxLength = targetColumn.Precision;
            }

            dataTable.Columns.Add(column);
        }

        using (System.Data.Common.DbConnection sourceConnection = sourceDatabase.CreateDbConnection(sourceConnectionString))
        {

            using (System.Data.Common.DbCommand sourceCommand = sourceDatabase.CreateDbCommand(sourceConnection))
            {
                this.SetReadTimeout(sourceDatabaseType, sourceCommand);

                if (take != 0)
                {
                    if (sourceDatabaseType == Models.DatabaseType.MySql)
                    {
                        sourceCommand.CommandText = this.FormatCommandText($"SELECT {sbColumns.ToString()} FROM [{sourceTable.SchemaName}].[{sourceTable.TableName}] LIMIT {take} OFFSET {skip}", sourceDatabaseType);
                    }
                    else
                    {
                        sourceCommand.CommandText = this.FormatCommandText($"SELECT {sbColumns.ToString()} FROM [{sourceTable.SchemaName}].[{sourceTable.TableName}] ORDER BY {sbKeyColumns.ToString()} OFFSET {skip} ROWS FETCH NEXT {take} ROWS ONLY", sourceDatabaseType);
                    }
                }
                else
                {
                    sourceCommand.CommandText = this.FormatCommandText($"SELECT {sbColumns.ToString()} FROM [{sourceTable.SchemaName}].[{sourceTable.TableName}]", sourceDatabaseType);
                }

                using (System.Data.Common.DbDataReader sourceReader = sourceCommand.ExecuteReader())
                {

                    var intFieldCount = sourceReader.FieldCount;

                    while (sourceReader.Read())
                    {
                        var dataRow = dataTable.NewRow();

                        for (int i = 0; i < intFieldCount; i++)
                        {

                            var sourceColumn = sourceMatchedColumns[i];
                            var targetColumn = targetMatchedColumns[i];

                            object value;

                            try
                            {
                                value = sourceReader.GetValue(i);
                            }
                            catch (Exception ex)
                            {
                                object newValue;

                                if (sourceDatabase.Provider != null &&
                                    sourceDatabase.Provider.TryHandleColumnValueLoadException(ex, sourceColumn, out newValue))
                                {
                                    value = newValue;
                                }
                                else
                                {
                                    throw;
                                }

                            }

                            dataRow[i] = this.GetColumnValue(targetColumn, value, trimStrings);
                        }

                        dataTable.Rows.Add(dataRow);
                    }
                }
            }
        }

        return dataTable;
    }

    public IList<Models.ColumnModel> GetMatchedColumns(IList<Models.ColumnModel> sourceColumns, IList<Models.ColumnModel> targetColumns)
    {
        var list = new List<Models.ColumnModel>();

        foreach (Cornerstone.Database.Models.ColumnModel sourceColumn in sourceColumns)
        {
            string strColumnName = sourceColumn.ColumnName;
            Cornerstone.Database.Models.ColumnModel targetColumn = (
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

    private void SetReadTimeout(Models.DatabaseType databaseType, System.Data.Common.DbCommand sourceCommand)
    {
        var databaseProvider = Cornerstone.Database.Services.DatabaseService.GetDatabaseProvider(_databaseProviders, databaseType);
        databaseProvider?.SetReadTimeout(sourceCommand);
    }

    private string FormatCommandText(string commandText, Cornerstone.Database.Models.DatabaseType databaseType)
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

    public int GetRowCount(Models.TableModel table, System.Data.Common.DbConnection connection)
    {
        var databaseType = Cornerstone.Database.Services.DatabaseService.GetDatabaseType(connection);
        return this.GetRowCount(connection, table.SchemaName, table.TableName, databaseType);
    }

    private int GetRowCount(System.Data.Common.DbConnection connection, string schemaName, string tableName, Cornerstone.Database.Models.DatabaseType databaseType)
    {
        int rowCount = 0;

        var database = new DatabaseService(databaseType, _databaseProviders, _connectionCreatedNotifications);

        using (var command = database.CreateDbCommand(connection))
        {
            try
            {
                if (databaseType == Cornerstone.Database.Models.DatabaseType.MicrosoftSQLServer)
                {
                    command.CommandText = string.Format("(SELECT sys.sysindexes.rows FROM sys.tables INNER JOIN sys.sysindexes ON sys.tables.object_id = sys.sysindexes.id AND sys.sysindexes.indid < 2 WHERE sys.tables.name = '{0}')", tableName);
                    rowCount = System.Convert.ToInt32(command.ExecuteScalar());
                }
            }
            catch
            {

            }

            try
            {
                if (rowCount == 0)
                {
                    command.CommandText = this.FormatCommandText(string.Format("SELECT COUNT(1) FROM [{0}].[{1}]", schemaName, tableName), databaseType);
                    rowCount = System.Convert.ToInt32(command.ExecuteScalar());
                }
            }
            catch
            {

            }
        }

        return rowCount;
    }

    private string GetParameterNameFromColumn(string columnName)
    {
        return columnName.Replace("-", "").Replace(" ", "");
    }

    private string GetRowErrorMessage(System.Data.Common.DbCommand command, IList<Cornerstone.Database.Models.ColumnModel> columns, int columnIndex, System.Exception ex)
    {
        System.Text.StringBuilder sbRow = new System.Text.StringBuilder();

        for (int intErrorIndex = 0; intErrorIndex < columnIndex; intErrorIndex++)
        {
            Cornerstone.Database.Models.ColumnModel targetColumn = columns[intErrorIndex];
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
