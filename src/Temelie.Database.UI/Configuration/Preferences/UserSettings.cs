using System.Collections.Generic;

namespace Temelie.Database.Configuration.Preferences;

public class UserSettings : PropertyChangedObject
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

    private IList<Models.ConnectionStringModel> _connections;
    public IList<Models.ConnectionStringModel> Connections
    {
        get
        {
            if (_connections == null)
            {
                _connections = new List<Models.ConnectionStringModel>();
            }
            return _connections;
        }
        set
        {
            _connections = value;
        }
    }

    public string MainWindowPlacement { get; set; }

    #endregion

    protected override void OnPropertyChanged(string propertyName)
    {
        base.OnPropertyChanged(propertyName);
    }

}
