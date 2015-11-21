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

namespace DatabaseTools.Configuration.Preferences
{

    public class UserSettings
    {

        #region Properties


        public string CreateScriptsPath { get; set; }
        public string ScriptsPath { get; set; }
        public string TargetConnectionString { get; set; }
        public string SourceConnectionString { get; set; }

        private System.Collections.Specialized.StringCollection _mergeTableLists;
        public System.Collections.Specialized.StringCollection MergeTableLists
        {
            get
            {
                if (this._mergeTableLists == null)
                {
                    this._mergeTableLists = new System.Collections.Specialized.StringCollection();
                }
                return this._mergeTableLists;
            }
            set
            {
                this._mergeTableLists = value;
            }
        }

        private System.Collections.Specialized.StringCollection _databases;
        public System.Collections.Specialized.StringCollection Databases
        {
            get
            {
                if (this._databases == null)
                {
                    this._databases = new System.Collections.Specialized.StringCollection();
                }
                return this._databases;
            }
            set
            {
                this._databases = value;
            }
        }

        public string TargetServer { get; set; }
        public string SourceServer { get; set; }

        private string _targetDatabase;
        public string TargetDatabase
        {
            get
            {
                return this._targetDatabase;
            }
            set
            {
                this._targetDatabase = value;
                this.UpdateDatabases(value);
            }
        }

        private string _sourceDatabase;
        public string SourceDatabase
        {
            get
            {
                return this._sourceDatabase;
            }
            set
            {
                this._sourceDatabase = value;
                this.UpdateDatabases(value);
            }
        }

        public string TargetDSN { get; set; }
        public string SourceDSN { get; set; }

        public string TargetOLE { get; set; }
        public string SourceOLE { get; set; }

        public string TargetMySql { get; set; }
        public string SourceMySql { get; set; }

        public string TargetAccessOLE { get; set; }
        public string SourceAccessOLE { get; set; }

        public string MainWindowPlacement { get; set; }

        #endregion

        #region Methods

        private void UpdateDatabases(string database)
        {
            if (!(string.IsNullOrEmpty(database)))
            {
                foreach (string d in this.Databases)
                {
                    if (d.Equals(database, StringComparison.InvariantCultureIgnoreCase))
                    {
                        return;
                    }
                }
                this.Databases.Add(database);
            }
        }



        #endregion

    }

}