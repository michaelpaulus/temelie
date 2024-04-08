using System.Collections.Generic;

namespace Cornerstone.Database
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
                    this.OnPropertyChanged(nameof(IsSelected));
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
                    this.OnPropertyChanged(nameof(DatabaseName));
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
