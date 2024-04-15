namespace Temelie.Database.Models;

public class DatabaseProviderModel : Model
{

    public DatabaseProviderModel()
    {

    }

    public DatabaseProviderModel(string name, string defaultConnectionString)
    {
        Name = name;
        DefaultConnectionString = defaultConnectionString;
    }

    public string Name { get; set; }
    public string DefaultConnectionString { get; set; }

}
