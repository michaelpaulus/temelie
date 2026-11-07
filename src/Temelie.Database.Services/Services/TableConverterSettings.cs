using Temelie.Database.Models;

namespace Temelie.Database.Services;

public class TableConverterSettings
{

    public TableConverterSettings()
    {
    }

    public required ConnectionStringModel SourceConnectionString { get; set; }
    public required ConnectionStringModel TargetConnectionString { get; set; }

    public required IList<Temelie.Database.Models.TableModel> SourceTables { get; set; }
    public required IList<Temelie.Database.Models.TableModel> TargetTables { get; set; }

    public int BatchSize { get; set; }

    public bool TrimStrings { get; set; }
    public bool UseTransaction { get; set; } = true;

}
