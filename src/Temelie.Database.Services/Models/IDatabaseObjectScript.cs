namespace Temelie.Database.Models;
public interface IDatabaseObjectScript
{
    string CreateScript { get; set; }
    string DropScript { get; set; }

}
