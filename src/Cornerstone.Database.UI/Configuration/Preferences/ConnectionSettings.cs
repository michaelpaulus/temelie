using System.Collections.Generic;

namespace Cornerstone.Database.Configuration.Preferences;

public class ConnectionSettings : PropertyChangedObject
{

    #region Properties

    private IList<Models.DatabaseConnection> _connections;
    public IList<Models.DatabaseConnection> Connections
    {
        get
        {
            if (_connections == null)
            {
                _connections = new List<Models.DatabaseConnection>();
            }
            return _connections;
        }
    }

    #endregion

    protected override void OnPropertyChanged(string propertyName)
    {
        base.OnPropertyChanged(propertyName);
    }

}
