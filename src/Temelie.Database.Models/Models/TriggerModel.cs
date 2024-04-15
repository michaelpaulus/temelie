namespace Temelie.Database.Models;

public class TriggerModel : DatabaseObjectModel
{

    public string TriggerName { get; set; }
    public string SchemaName { get; set; }
    public string Definition { get; set; }
    public string TableName { get; set; }

}
