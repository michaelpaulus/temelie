using Temelie.Database.Models;

namespace Temelie.Database.Providers.MySql;
internal class DatabaseObjectScript : IDatabaseObjectScript
{
    public DatabaseObjectScript(Func<string> create, Func<string> drop)
    {
        CreateScript = create();
        DropScript = drop();
    }

    public string CreateScript { get; set; }
    public string DropScript { get; set; }
}
