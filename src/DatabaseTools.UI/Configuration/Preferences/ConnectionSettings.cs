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

}