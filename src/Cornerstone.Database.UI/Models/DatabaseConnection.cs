using System.Linq;

namespace Cornerstone.Database.Models;

public class DatabaseConnection : Model
{

    public string Name { get; set; }
    public string ConnectionType { get; set; }
    public string ConnectionString { get; set; }

    protected override void OnPropertyChanged(string propertyName)
    {
        base.OnPropertyChanged(propertyName);
        switch (propertyName)
        {
            case nameof(ConnectionType):
                if (ConnectionType != null && string.IsNullOrEmpty(ConnectionString))
                {
                    this.ConnectionString = DatabaseConnectionType.GetDatabaseConnectionTypes().Where(i => i.ConnectionType == ConnectionType).Select(i => i.DefaultConnectionString).FirstOrDefault();
                }
                break;
        }
    }

}
