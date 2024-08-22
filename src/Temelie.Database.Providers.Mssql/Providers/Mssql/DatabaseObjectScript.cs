using Temelie.Database.Models;

namespace Temelie.Database.Providers.Mssql;
internal class DatabaseObjectScript : IDatabaseObjectScript
{
    public DatabaseObjectScript(Func<string> create, Func<string> drop)
    {
        CreateScript = create();
        DropScript = drop();
    }

    public DatabaseObjectScript()
    {
    }

    public string CreateScript { get; set; }
    public string DropScript { get; set; }
}
