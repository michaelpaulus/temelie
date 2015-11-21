
using System.Linq;
using System.Xml.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Shapes;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

namespace DatabaseTools
{
    public partial class Convert
    {

        private List<string> _sourceTables;
        private object _lockObject = new object();
        private int _completedWorkerCount;
        private DateTime _conversionStartTime;
        private int _workerCount = 5;
        private int _totalCount;

        public Convert()
        {

            // This call is required by the Windows Form Designer.
            InitializeComponent();

            // Add any initialization after the InitializeComponent() call.
            this.SourceDatabaseConnection.IsSource = true;
            SubscribeToEvents();
        }

        #region Methods

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

        private string GetParameterNameFromColumn(string columnName)
        {
            return columnName.Replace("-", "").Replace(" ", "");
        }

        private void ConvertTableBulk(System.ComponentModel.BackgroundWorker worker, string tableName, System.Configuration.ConnectionStringSettings sourceConnectionString, IList<DatabaseTools.Models.ColumnModel> sourceTableColumns, System.Configuration.ConnectionStringSettings targetConnectionString, IList<DatabaseTools.Models.ColumnModel> targetTableColumns)
        {
            System.Data.Common.DbProviderFactory sourceFactory = DatabaseTools.Processes.Database.CreateDbProviderFactory(sourceConnectionString);
            System.Data.Common.DbProviderFactory targetFactory = DatabaseTools.Processes.Database.CreateDbProviderFactory(targetConnectionString);

            var sourceDatabaseType = DatabaseTools.Processes.Database.GetDatabaseType(sourceConnectionString);
            var targetDatabaseType = DatabaseTools.Processes.Database.GetDatabaseType(targetConnectionString);

            using (System.Data.Common.DbConnection targetConnection = DatabaseTools.Processes.Database.CreateDbConnection(targetFactory, targetConnectionString))
            {

                var intTargetRowCount = this.GetRowCount(targetConnection, tableName, targetDatabaseType);

                if (intTargetRowCount == 0L)
                {
                    using (System.Data.Common.DbConnection sourceConnection = DatabaseTools.Processes.Database.CreateDbConnection(sourceFactory, sourceConnectionString))
                    {

                        var intSourceRowCount = this.GetRowCount(sourceConnection, tableName, sourceDatabaseType);

                        if (intSourceRowCount > 0L)
                        {
                            using (System.Data.SqlClient.SqlBulkCopy bcp = new System.Data.SqlClient.SqlBulkCopy((System.Data.SqlClient.SqlConnection)targetConnection))
                            {
                                bcp.DestinationTableName = tableName;
                                bcp.BatchSize = 1000;
                                bcp.BulkCopyTimeout = 600;
                                bcp.NotifyAfter = bcp.BatchSize;

                                long intRowIndex = 0L;
                                int intProgress = 0;

                                bcp.SqlRowsCopied += (object sender, System.Data.SqlClient.SqlRowsCopiedEventArgs e) =>
                                {
                                    intRowIndex += 1L;

                                    int intNewProgress = System.Convert.ToInt32(intRowIndex / (double)intSourceRowCount * 100);

                                    if (intProgress != intNewProgress)
                                    {
                                        intProgress = intNewProgress;
                                        worker.ReportProgress(intProgress, tableName);
                                    }
                                };

                                using (var command = DatabaseTools.Processes.Database.CreateDbCommand(sourceConnection))
                                {
                                    command.CommandText = this.FormatCommandText(string.Format("SELECT * FROM [{0}]", tableName), sourceDatabaseType);
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

            worker.ReportProgress(100, tableName);
        }

        private Int64 GetRowCount(System.Data.Common.DbConnection connection, string tableName, DatabaseTools.Models.DatabaseType databaseType)
        {
            long lngRowCount = 0;

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
                            command.CommandText = this.FormatCommandText(string.Format("SELECT COUNT(*) FROM [{0}]", tableName), databaseType);
                            break;
                    }
                    lngRowCount = System.Convert.ToInt64(command.ExecuteScalar());
                }
            }
            catch 
            {

            }

            return lngRowCount;
        }

        private void ConvertTable(System.ComponentModel.BackgroundWorker worker, string tableName, System.Configuration.ConnectionStringSettings sourceConnectionString, IList<DatabaseTools.Models.ColumnModel> sourceTableColumns, System.Configuration.ConnectionStringSettings targetConnectionString, IList<DatabaseTools.Models.ColumnModel> targetTableColumns)
        {
            System.Data.Common.DbProviderFactory sourceFactory = DatabaseTools.Processes.Database.CreateDbProviderFactory(sourceConnectionString);
            System.Data.Common.DbProviderFactory targetFactory = DatabaseTools.Processes.Database.CreateDbProviderFactory(targetConnectionString);

            var sourceDatabaseType = DatabaseTools.Processes.Database.GetDatabaseType(sourceConnectionString);
            var targetDatabaseType = DatabaseTools.Processes.Database.GetDatabaseType(targetConnectionString);

            int intProgress = 0;

            using (System.Data.Common.DbConnection targetConnection = DatabaseTools.Processes.Database.CreateDbConnection(targetFactory, targetConnectionString))
            {
                var intTargetRowCount = this.GetRowCount(targetConnection, tableName, targetDatabaseType);
                if (intTargetRowCount == 0L)
                {
                    using (System.Data.Common.DbCommand targetCommand = DatabaseTools.Processes.Database.CreateDbCommand(targetConnection))
                    {
                        System.Text.StringBuilder sbColumns = new System.Text.StringBuilder();
                        System.Text.StringBuilder sbParamaters = new System.Text.StringBuilder();

                        bool blnContainsIdentity = false;

                        var sourceColumns = sourceTableColumns.ToList();
                        List<DatabaseTools.Models.ColumnModel> targetColumns = new List<DatabaseTools.Models.ColumnModel>();

                        foreach (DatabaseTools.Models.ColumnModel sourceColumn in sourceTableColumns)
                        {
                            string strColumnName = sourceColumn.ColumnName;
                            DatabaseTools.Models.ColumnModel targetColumn = (
                                from c in targetTableColumns
                                where c.ColumnName.Equals(strColumnName, StringComparison.InvariantCultureIgnoreCase)
                                select c).FirstOrDefault();

                            if (targetColumn != null && !targetColumn.IsComputed)
                            {

                                if (targetColumn.IsIdentity)
                                {
                                    blnContainsIdentity = true;
                                }

                                targetColumns.Add(targetColumn);

                                if (sbColumns.Length > 0)
                                {
                                    sbColumns.Append(", ");
                                }
                                sbColumns.AppendFormat("[{0}]", sourceColumn.ColumnName);
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
                                        paramater.Size = sourceColumn.Precision;
                                        if (paramater.DbType == System.Data.DbType.String && (paramater) is System.Data.SqlClient.SqlParameter)
                                        {
                                            ((System.Data.SqlClient.SqlParameter)paramater).SqlDbType = System.Data.SqlDbType.VarChar;
                                        }
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
                            else
                            {
                                sourceColumns.Remove(sourceColumn);
                            }
                        }

                        targetCommand.CommandText = this.FormatCommandText(string.Format("INSERT INTO [{0}] ({1}) VALUES ({2})", tableName, sbColumns.ToString(), sbParamaters.ToString()), targetDatabaseType);

                        if (blnContainsIdentity)
                        {
                            targetCommand.CommandText = this.FormatCommandText(string.Format("SET IDENTITY_INSERT [{0}] ON;" + Environment.NewLine + targetCommand.CommandText + Environment.NewLine + "SET IDENTITY_INSERT {0} OFF;", tableName), targetDatabaseType);
                        }

                        using (System.Data.Common.DbConnection sourceConnection = DatabaseTools.Processes.Database.CreateDbConnection(sourceFactory, sourceConnectionString))
                        {
                            var intRowCount = this.GetRowCount(sourceConnection, tableName, sourceDatabaseType);

                            using (System.Data.Common.DbCommand sourceCommand = DatabaseTools.Processes.Database.CreateDbCommand(sourceConnection))
                            {
                                sourceCommand.CommandText = this.FormatCommandText(string.Format("SELECT COUNT(*) FROM [{1}]", sbColumns.ToString(), tableName), sourceDatabaseType);

                                Int64 intRowIndex = 0L;

                                if (intRowCount > 0L)
                                {
                                    sourceCommand.CommandText = this.FormatCommandText(string.Format("SELECT {0} FROM [{1}]", sbColumns.ToString(), tableName), sourceDatabaseType);

                                    System.Data.Common.DbDataReader sourceReader = sourceCommand.ExecuteReader();

                                    while (sourceReader.Read())
                                    {
                                        object[] values = new object[sourceReader.FieldCount];
                                        int intFieldCount = sourceReader.GetValues(values);

                                        for (int intIndex = 0; intIndex < targetCommand.Parameters.Count; intIndex++)
                                        {
                                            var parameter = targetCommand.Parameters[intIndex];
                                            object objValue = values[intIndex];

                                            var sourceColumn = sourceColumns[intIndex];

                                            try
                                            {
                                                if (System.Convert.IsDBNull(objValue))
                                                {
                                                    parameter.Value = objValue;
                                                }
                                                else
                                                {
                                                    switch (sourceColumn.DbType)
                                                    {
                                                        case System.Data.DbType.Date:
                                                        case System.Data.DbType.DateTime:
                                                        case System.Data.DbType.DateTime2:
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
                                                                break;
                                                            }
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
                                                                parameter.Value = System.Convert.ToString(objValue).TrimEnd();
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
                                            catch (Exception ex)
                                            {
                                                string strRowErrorMessage = this.GetRowErrorMessage(targetCommand, targetColumns, intIndex, ex);

                                                string strErrorMessage = string.Format("could not get the value for paramater: {0} on table: {1} at row: {2}", parameter.ParameterName, tableName, strRowErrorMessage);

                                                worker.ReportProgress(System.Convert.ToInt32(intRowIndex / (double)intRowCount * 100), string.Concat(tableName, "|", strErrorMessage));

                                                parameter.Value = DBNull.Value;
                                            }
                                        }

                                        try
                                        {
                                            targetCommand.ExecuteNonQuery();

                                            intRowIndex += 1L;

                                            int intNewProgress = System.Convert.ToInt32(intRowIndex / (double)intRowCount * 100);

                                            if (intProgress != intNewProgress)
                                            {
                                                intProgress = intNewProgress;
                                                worker.ReportProgress(intProgress, tableName);
                                            }

                                        }
                                        catch (Exception ex)
                                        {
                                            string strRowErrorMessage = this.GetRowErrorMessage(targetCommand, targetColumns, targetCommand.Parameters.Count - 1, ex);

                                            string strErrorMessage = string.Format("could not insert row on table: {0} at row: {1}", tableName, strRowErrorMessage);

                                            int intNewProgress = System.Convert.ToInt32(intRowIndex / (double)intRowCount * 100);

                                            intProgress = intNewProgress;

                                            worker.ReportProgress(intProgress, string.Concat(tableName, "|", strErrorMessage));

                                            break;
                                        }

                                    }
                                }

                            }
                        }
                    }
                }

            }

            worker.ReportProgress(100, tableName);
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
            sbRow.AppendLine(ex.ToString());

            return sbRow.ToString();
        }

        private void UpdateColumns()
        {
            this.ResultsGridView.Columns[2].Width = this.ResultsListBox.ActualWidth - this.ResultsGridView.Columns[0].ActualWidth - this.ResultsGridView.Columns[1].ActualWidth - 30;
        }

        #endregion

        #region Event Handlers

        private void ThreadsTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            int.TryParse(this.ThreadsTextBox.Text, out this._workerCount);
        }

        private void ToggleAllButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (this.ResultsListBox.ItemsSource != null)
            {
                foreach (var t in (List<DatabaseTools.Models.TableModel>)this.ResultsListBox.ItemsSource)
                {
                    t.Selected = !t.Selected;
                }
            }
        }

        private void ConvertButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            this.ErrorsTextBox.Text = string.Empty;
            this.ConvertButton.IsEnabled = false;
            this._conversionStartTime = DateTime.Now;
            this._completedWorkerCount = 0;

            var sourceConnectionString = this.SourceDatabaseConnection.ConnectionString;
            var targetConnectionString = this.TargetDatabaseConnection.ConnectionString;

            List<DatabaseTools.Models.TableModel> sourceTables = this.ResultsListBox.ItemsSource as List<DatabaseTools.Models.TableModel>;

            if (sourceTables != null)
            {
                this._sourceTables = (
                    from i in sourceTables
                    where i.Selected
                    select i.TableName).ToList();

                int intWorkerCount = Math.Min(this._sourceTables.Count, this._workerCount);

                var targetColumns = DatabaseTools.Processes.Database.GetTableColumns(targetConnectionString);
                var targetTables = DatabaseTools.Processes.Database.GetTables(targetConnectionString, targetColumns);

                this._totalCount = intWorkerCount;

                for (int intIndex = 0; intIndex < this._totalCount; intIndex++)
                {
                    System.ComponentModel.BackgroundWorker worker = new System.ComponentModel.BackgroundWorker { WorkerReportsProgress = true };
                    worker.DoWork += this.ConverterBackgroundWorker_DoWork;
                    worker.ProgressChanged += this.ConverterBackgroundWorker_ProgressChanged;
                    worker.RunWorkerCompleted += this.ConverterBackgroundWorker_RunWorkerCompleted;

                    TableConversion tableConvert = new TableConversion();
                    tableConvert.SourceConnectionString = sourceConnectionString;
                    tableConvert.TargetConnectionString = targetConnectionString;

                    tableConvert.SourceTables = (
                        from i in sourceTables
                        where i.Selected
                        select i).ToList();
                    tableConvert.TargetTables = targetTables;

                    tableConvert.UseBulkCopy = this.UseBulkCopyCheckBox.IsChecked.GetValueOrDefault();

                    worker.RunWorkerAsync(tableConvert);
                }
            }
        }

        private void ConverterBackgroundWorker_DoWork(object sender, System.ComponentModel.DoWorkEventArgs e)
        {
            System.ComponentModel.BackgroundWorker worker = (System.ComponentModel.BackgroundWorker)sender;

            var tableConvert = (TableConversion)e.Argument;

            foreach (var sourceTable in tableConvert.SourceTables)
            {

                bool blnProcessTableName = false;
                lock (_lockObject)
                {
                    if (this._sourceTables.Contains(sourceTable.TableName))
                    {
                        blnProcessTableName = true;
                        this._sourceTables.Remove(sourceTable.TableName);
                    }
                }

                if (blnProcessTableName)
                {
                    string strTableName = sourceTable.TableName;
                    var targetTable = (
                        from i in tableConvert.TargetTables
                        where i.TableName.Equals(strTableName, StringComparison.InvariantCultureIgnoreCase)
                        select i).FirstOrDefault();

                    if (targetTable != null)
                    {
                        try
                        {

                            if (tableConvert.UseBulkCopy)
                            {
                                this.ConvertTableBulk(worker, sourceTable.TableName, tableConvert.SourceConnectionString, sourceTable.Columns, tableConvert.TargetConnectionString, targetTable.Columns);
                            }
                            else
                            {
                                this.ConvertTable(worker, sourceTable.TableName, tableConvert.SourceConnectionString, sourceTable.Columns, tableConvert.TargetConnectionString, targetTable.Columns);
                            }

                        }
                        catch (Exception ex)
                        {
                            string strException = string.Empty;
                            if (!(ex.Message.Equals("ERROR [00000] [QODBC] Error: 3250 - This feature is not enabled or not available in this version of QuickBooks.")))
                            {
                                strException = ex.ToString();
                                if (ex.InnerException != null)
                                {
                                    strException += Environment.NewLine + ex.InnerException.ToString();
                                }
                            }
                            worker.ReportProgress(100, sourceTable.TableName + "|" + strException);
                        }
                    }
                    else
                    {
                        worker.ReportProgress(100, sourceTable.TableName);
                    }
                }
            }

        }

        private void ConverterBackgroundWorker_ProgressChanged(object sender, System.ComponentModel.ProgressChangedEventArgs e)
        {
            string strTableName = e.UserState.ToString();
            string strErrorMessage = string.Empty;
            if (strTableName.Contains("|"))
            {
                strErrorMessage = strTableName.Split('|')[1];
                strTableName = strTableName.Split('|')[0];
            }

            DatabaseTools.Models.TableModel table = (
                from i in ((IList<DatabaseTools.Models.TableModel>)this.ResultsListBox.ItemsSource)
                where i.TableName.Equals(strTableName)
                select i).Take(1).SingleOrDefault();
            if (table != null)
            {
                table.ProgressPercentage = e.ProgressPercentage;
                table.ErrorMessage = strErrorMessage;
            }

            if (!(string.IsNullOrEmpty(table.ErrorMessage)))
            {
                this.ErrorsTextBox.Text += Environment.NewLine + Environment.NewLine + "Table: " + strTableName + Environment.NewLine + "Error: " + strErrorMessage;
            }
        }

        private void ConverterBackgroundWorker_RunWorkerCompleted(object sender, System.ComponentModel.RunWorkerCompletedEventArgs e)
        {
            this._completedWorkerCount += 1;

            if (e.Error != null)
            {
                this.ErrorsTextBox.Text += Environment.NewLine + Environment.NewLine + "Worker Error: " + e.Error.ToString();
            }

            if (this._completedWorkerCount == this._totalCount)
            {
                MessageBox.Show(string.Format("Conversion Completed in {0} minutes.", DateTime.Now.Subtract(_conversionStartTime).TotalMinutes));
                this.ConvertButton.IsEnabled = true;
            }
        }

        private void ResultsListBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            this.UpdateColumns();
        }

        private void ResultsListBox_SizeChanged(object sender, System.Windows.SizeChangedEventArgs e)
        {
            this.UpdateColumns();
        }

        private void ResultsListBox_SourceUpdated(object sender, System.Windows.Data.DataTransferEventArgs e)
        {
            this.UpdateColumns();
        }

        private void SourceDatabaseConnection_SelectionChanged(object sender, System.EventArgs e)
        {
            this.ResultsListBox.ItemsSource = null;

            Action<object> action = new Action<object>((object obj) =>
            {
                try
                {
                    var connectionString = (System.Configuration.ConnectionStringSettings)obj;
                    var tables = DatabaseConnection.GetTables(connectionString);
                    this.Dispatcher.Invoke(new Action<IEnumerable>((IEnumerable results) =>
                    {
                        this.ResultsListBox.ItemsSource = results;
                    }), tables);
                }
                catch (Exception ex)
                {
                    Dispatcher.Invoke(new Action<Exception>((Exception results) =>
                    {
                        MessageBox.Show(results.ToString());
                    }), ex);
                }
            });

            System.Threading.Tasks.Task.Factory.StartNew(action, this.SourceDatabaseConnection.ConnectionString);
        }

        #endregion

        #region Nested Types

        private class TableConversion
        {

            public System.Configuration.ConnectionStringSettings SourceConnectionString { get; set; }
            public System.Configuration.ConnectionStringSettings TargetConnectionString { get; set; }

            public IList<DatabaseTools.Models.TableModel> SourceTables { get; set; }
            public IList<DatabaseTools.Models.TableModel> TargetTables { get; set; }

            public bool UseBulkCopy { get; set; }

        }

        #endregion


        
        private bool EventsSubscribed = false;
        private void SubscribeToEvents()
        {
            if (EventsSubscribed)
                return;
            else
                EventsSubscribed = true;

            ThreadsTextBox.TextChanged += ThreadsTextBox_TextChanged;
            ToggleAllButton.Click += ToggleAllButton_Click;
            ConvertButton.Click += ConvertButton_Click;
            ResultsListBox.SelectionChanged += ResultsListBox_SelectionChanged;
            ResultsListBox.SizeChanged += ResultsListBox_SizeChanged;
            ResultsListBox.SourceUpdated += ResultsListBox_SourceUpdated;
            SourceDatabaseConnection.SelectionChanged += SourceDatabaseConnection_SelectionChanged;
        }

    }

}