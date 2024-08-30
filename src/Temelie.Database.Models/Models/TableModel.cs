using System.Text.Json.Serialization;

namespace Temelie.Database.Models;

public class TableModel : DatabaseObjectModel
{

    public TableModel()
    {
        this.Selected = true;
    }

    [JsonIgnore]
    public bool Selected { get; set; }

    public string TableName { get; set; }
    public string SchemaName { get; set; }

    [JsonIgnore]
    public string ClassName
    {
        get
        {
            if (ExtendedProperties.TryGetValue("name", out var name))
            {
                return name;
            }
            if (ExtendedProperties.TryGetValue("dynamicName", out var dynamicName))
            {
                return dynamicName;
            }
            return ColumnModel.NormalizePropertyName(TableName);
        }
    }

    [JsonIgnore]
    public int ProgressPercentage { get; set; }

    [JsonIgnore]
    public string ErrorMessage { get; set; }

    public int TemporalType { get; set; }
    public string HistoryTableName { get; set; }
    public bool IsMemoryOptimized { get; set; }
    public string DurabilityDesc { get; set; }
    public bool IsExternal { get; set; }
    public string DataSourceName { get; set; }
    [JsonIgnore]
    public bool IsHistoryTable { get { return TemporalType == 1; } }
    public bool IsView { get; set; }

    private IList<ColumnModel> _columns;
    public IList<ColumnModel> Columns
    {
        get
        {
            if (this._columns == null)
            {
                this._columns = new List<ColumnModel>();
            }
            return this._columns;
        }
        set
        {
            _columns = value;
        }
    }

    public Dictionary<string, string> ExtendedProperties { get; set; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

    public string PartitionSchemeName { get; set; }
    public string PartitionSchemeColumns { get; set; }

    public string Options { get; set; }

    [JsonIgnore]
    public IndexModel PrimaryKey { get; set; }

}

