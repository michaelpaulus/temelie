
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
                            var tableConverter = new Processes.TableConverter();

                            if (tableConvert.UseBulkCopy)
                            {
                                tableConverter.ConvertBulk(worker, sourceTable.TableName, tableConvert.SourceConnectionString, sourceTable.Columns, tableConvert.TargetConnectionString, targetTable.Columns);
                            }
                            else
                            {
                                tableConverter.Convert(worker, sourceTable.TableName, tableConvert.SourceConnectionString, sourceTable.Columns, tableConvert.TargetConnectionString, targetTable.Columns);
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