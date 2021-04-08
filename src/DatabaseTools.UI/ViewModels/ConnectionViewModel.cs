using DatabaseTools.Configuration.Preferences;
using DatabaseTools.Views;
using PropertyChanged;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseTools.ViewModels
{
    public class ConnectionViewModel : ViewModel
    {

        public ConnectionViewModel()
        {
            this.EditCommand = new Command(() =>
            {
                this.Edit();
            });

            this.LoadConnections();
        }

        public Command EditCommand { get; set; }

        public Models.DatabaseConnection SelectedConnection { get; set; }

        private ObservableCollection<Models.DatabaseConnection> _connnections;
        public ObservableCollection<Models.DatabaseConnection> Connections
        {
            get
            {
                if (_connnections == null)
                {
                    _connnections = new ObservableCollection<Models.DatabaseConnection>();
                }
                return _connnections;
            }
        }

        public bool IsSource { get; set; }

        public void LoadConnections()
        {
            this.Connections.Clear();
            foreach (var item in ConnectionSettingsContext.Current.Connections.OrderBy(i => i.Name))
            {
                this.Connections.Add(item);
            }

            if (this.IsSource)
            {
                var current = (from i in this.Connections where i.Name == UserSettingsContext.Current.SourceConnectionString select i).FirstOrDefault();
                if (current != null)
                {
                    this.SelectedConnection = current;
                }
            }
            else
            {
                var current = (from i in this.Connections where i.Name == UserSettingsContext.Current.TargetConnectionString select i).FirstOrDefault();
                if (current != null)
                {
                    this.SelectedConnection = current;
                }
            }

            this.OnSelectionChanged(EventArgs.Empty);

        }

        private void Edit()
        {
            var dialog = new DatabaseConnectionDialog();
            dialog.ShowDialog();
            this.LoadConnections();
        }

        protected override void OnPropertyChanged(string propertyName)
        {
            base.OnPropertyChanged(propertyName);
            switch (propertyName)
            {
                case nameof(SelectedConnection):
                    if (SelectedConnection != null)
                    {
                        if (this.IsSource)
                        {
                            UserSettingsContext.Current.SourceConnectionString = SelectedConnection.Name;
                        }
                        else
                        {
                            UserSettingsContext.Current.TargetConnectionString = SelectedConnection.Name;
                        }
                        UserSettingsContext.Save();
                    }
                    this.OnSelectionChanged(EventArgs.Empty);
                    break;
                case nameof(IsSource):
                    this.LoadConnections();
                    break;
            }
        }

        #region Event Raising Methods

        [SuppressPropertyChangedWarnings]
        protected virtual void OnSelectionChanged(EventArgs e)
        {
            if (SelectionChanged != null)
                SelectionChanged(this, e);
        }

        #endregion

        #region Events

        public event EventHandler SelectionChanged;

        #endregion

    }
}
