using Temelie.Database.Models;

namespace Temelie.Database.Services;

public class TableConverterSettings
{

    public TableConverterSettings()
    {
    }

    public ConnectionStringModel SourceConnectionString { get; set; }
    public ConnectionStringModel TargetConnectionString { get; set; }

    public IList<Temelie.Database.Models.TableModel> SourceTables { get; set; }
    public IList<Temelie.Database.Models.TableModel> TargetTables { get; set; }

    public int BatchSize { get; set; }

    public bool TrimStrings { get; set; }
    public bool UseTransaction { get; set; } = true;

}
