namespace Temelie.Database.Models;

public class ConnectionStringModel : Model
{

    public string Name { get; set; }
    public string DatabaseProviderName { get; set; }
    public string ConnectionString { get; set; }

}
