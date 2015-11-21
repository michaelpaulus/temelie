
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
    namespace Models
    {
        public class UIDatabaseModel : Model
        {
            #region Properties

            private bool _isSelected;
            public bool IsSelected
            {
                get
                {
                    return _isSelected;
                }
                set
                {
                    _isSelected = value;
                    this.OnPropertyChanged("IsSelected");
                }
            }

            private string _databaseName;
            public string DatabaseName
            {
                get
                {
                    return _databaseName;
                }
                set
                {
                    _databaseName = value;
                    this.OnPropertyChanged("DatabaseName");
                }
            }

            private IList<string> _files;
            public IList<string> Files
            {
                get
                {
                    if (this._files == null)
                    {
                        this._files = new List<string>();
                    }
                    return this._files;
                }
            }

            #endregion

        }
    }


}