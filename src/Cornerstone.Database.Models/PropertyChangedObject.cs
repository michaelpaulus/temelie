using System.ComponentModel;
using PropertyChanged;

namespace Cornerstone.Database;

[AddINotifyPropertyChangedInterface()]
public class PropertyChangedObject : INotifyPropertyChanged
{
    protected virtual void OnPropertyChanged(string propertyName)
    {
        if (this.PropertyChanged != null)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;
}
