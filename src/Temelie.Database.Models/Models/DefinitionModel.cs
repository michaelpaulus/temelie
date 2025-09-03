namespace Temelie.Database.Models;
public class DefinitionModel : DatabaseObjectModel
{

    public string DefinitionName { get; set; }
    public string SchemaName { get; set; }
    [System.Text.Json.Serialization.JsonIgnore]
    public string Definition { get; set; }

    [System.Text.Json.Serialization.JsonPropertyName(nameof(Definition))]
    public string[] DefinitionLines
    {
        get
        {
            return Definition?.Split('\n') ?? [];
        }
        set
        {
            Definition = string.Join("\n", value ?? []);
        }
    }

    public string XType { get; set; }

    public string Type
    {
        get
        {
            switch (this.XType)
            {
                case "P":
                    return "PROCEDURE";
                case "V":
                    return "VIEW";
            }
            return "FUNCTION";
        }
    }

    public TableModel View { get; set; }

}
