
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
    namespace ViewModels
    {
        public class AttachDatabaseViewModel : ViewModel
        {
            #region Properties

            private string _directory;
            public string Directory
            {
                get
                {
                    return _directory;
                }
                set
                {
                    _directory = value;
                    this.OnPropertyChanged("Directory");
                }
            }

            private string _results;
            public string Results
            {
                get
                {
                    return _results;
                }
                set
                {
                    _results = value;
                    this.OnPropertyChanged("Results");
                }
            }


            private System.Collections.ObjectModel.ObservableCollection<Models.UIDatabaseModel> _files;
            public System.Collections.ObjectModel.ObservableCollection<Models.UIDatabaseModel> Databases
            {
                get
                {
                    if (this._files == null)
                    {
                        this._files = new System.Collections.ObjectModel.ObservableCollection<Models.UIDatabaseModel>();
                    }
                    return this._files;
                }
            }

            #endregion

            #region Methods

            public void DetachDatabases(System.Configuration.ConnectionStringSettings connectionString)
            {
                foreach (var model in (
                    from i in this.Databases
                    where i.IsSelected
                    select i))
                {
                    try
                    {
                        System.Text.StringBuilder sbCommand = new System.Text.StringBuilder();
                        sbCommand.AppendLine(string.Format("EXEC sp_detach_db @dbname = N'{0}'", model.DatabaseName));

                        DatabaseTools.Processes.Database.ExecuteNonQuery(connectionString, sbCommand.ToString());

                        model.IsSelected = false;
                    }
                    catch
                    {
                        this.Results += string.Format("Detach of database: {0} failed.", model.DatabaseName) + Environment.NewLine;
                    }
                }
            }

            public void AttachDatabases(System.Configuration.ConnectionStringSettings connectionString)
            {
                foreach (var model in (
                    from i in this.Databases
                    where i.IsSelected
                    select i))
                {
                    try
                    {
                        System.Text.StringBuilder sbCommand = new System.Text.StringBuilder();
                        sbCommand.AppendLine(string.Format("EXEC sp_attach_db @dbname = N'{0}'", model.DatabaseName));

                        int intFileCount = 0;

                        foreach (var file in model.Files)
                        {
                            intFileCount += 1;
                            sbCommand.AppendLine(string.Format(", @filename{0} = N'{1}'", intFileCount, file));
                        }

                        DatabaseTools.Processes.Database.ExecuteNonQuery(connectionString, sbCommand.ToString());

                        model.IsSelected = false;

                    }
                    catch
                    {
                        this.Results += string.Format("Attach of database: {0} failed.", model.DatabaseName) + Environment.NewLine;
                    }
                }
            }

            protected override void OnPropertyChanged(string propertyName)
            {
                base.OnPropertyChanged(propertyName);
                switch (propertyName)
                {
                    case "Directory":
                        if (string.IsNullOrEmpty(this.Directory) || !(System.IO.Directory.Exists(this.Directory)))
                        {
                            this.Databases.Clear();
                        }
                        else
                        {
                            var mdfFiles = System.IO.Directory.GetFiles(this.Directory, "*.mdf").ToList();
                            var ldfFiles = System.IO.Directory.GetFiles(this.Directory, "*.ldf").ToList();

                            List<Models.UIDatabaseModel> list = new List<Models.UIDatabaseModel>();

                            foreach (var mdfFile in (
                                from i in mdfFiles
                                orderby i descending
                                select i).ToList())
                            {
                                mdfFiles.Remove(mdfFile);
                                Models.UIDatabaseModel model = new Models.UIDatabaseModel { DatabaseName = System.IO.Path.GetFileNameWithoutExtension(mdfFile).Replace("_Data", "") };

                                model.Files.Add(mdfFile);

                                //Find log files for this mdf file
                                foreach (var strFile in (
                                    from i in ldfFiles
                                    where System.IO.Path.GetFileName(i).StartsWith(model.DatabaseName, StringComparison.InvariantCultureIgnoreCase)
                                    select i).ToList())
                                {
                                    model.Files.Add(strFile);
                                    ldfFiles.Remove(strFile);
                                }

                                list.Add(model);
                            }

                            foreach (var item in (
                                from i in list
                                orderby i.DatabaseName
                                select i))
                            {
                                this.Databases.Add(item);
                            }
                        }
                        break;
                }
            }

            #endregion

        }
    }

}