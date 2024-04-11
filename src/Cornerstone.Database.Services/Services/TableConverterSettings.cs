using Cornerstone.Database.Models;

namespace Cornerstone.Database.Services;

public class TableConverterSettings
{

    public TableConverterSettings()
    {
    }

    public ConnectionStringModel SourceConnectionString { get; set; }
    public ConnectionStringModel TargetConnectionString { get; set; }

    public IList<Cornerstone.Database.Models.TableModel> SourceTables { get; set; }
    public IList<Cornerstone.Database.Models.TableModel> TargetTables { get; set; }

    public bool UseBulkCopy { get; set; }

    public int BatchSize { get; set; }

    public bool TrimStrings { get; set; }
    public bool UseTransaction { get; set; } = true;

}
