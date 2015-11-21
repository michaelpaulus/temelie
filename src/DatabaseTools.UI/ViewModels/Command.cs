using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseTools.ViewModels
{
    public class Command : System.Windows.Input.ICommand
    {

        public Command(Action action)
        {
            this.Action = action;
            this._isEnabled = true;
        }

        #region "Properties"

        Action Action { get; set; }

        bool _isEnabled;
        public bool IsEnabled
        {
            get
            {
                return this._isEnabled;
            }
            set
            {
                if (!bool.Equals(this._isEnabled, value))
                {
                    this._isEnabled = value;
                    this.OnCanExecuteChanged(new EventArgs());
                }
            }
        }

        #endregion;

        #region "Methods"

        internal void OnCanExecuteChanged(EventArgs e)
        {
            if (CanExecuteChanged != null)
            {
                CanExecuteChanged.Invoke(this, e);
            }
        }

        public Boolean CanExecute(Object parameter)
        {
            return this.IsEnabled;
        }

        public void Execute(Object parameter)
        {
            if (this.Action != null)
            {
                this.Action.Invoke();
            }

        }

        #endregion;

        #region "Events"

        public event EventHandler CanExecuteChanged;

        #endregion;

    }
    public class Command<T> : System.Windows.Input.ICommand where T : class
    {

        public Command(Action<T> action)
        {
            this.Action = action;
            this._isEnabled = true;
        }

        #region "Properties"

        Action<T> Action { get; set; }


        bool _isEnabled;
        public bool IsEnabled
        {
            get
            {
                return this._isEnabled;
            }
            set
            {
                if (!bool.Equals(this._isEnabled, value))
                {
                    this._isEnabled = value;
                    this.OnCanExecuteChanged(new EventArgs());
                }
            }
        }

        #endregion;

        #region "Methods"

        internal void OnCanExecuteChanged(EventArgs e)
        {
            if (CanExecuteChanged != null)
            {
                CanExecuteChanged.Invoke(this, e);
            }
        }

        public Boolean CanExecute(Object parameter)
        {
            return this.IsEnabled;
        }

        public void Execute(Object parameter)
        {
            if (this.Action != null)
            {
                this.Action.Invoke(parameter as T);
            }
        }

        #endregion;

        #region "Events"

        public event EventHandler CanExecuteChanged;

        #endregion;

    }

}
