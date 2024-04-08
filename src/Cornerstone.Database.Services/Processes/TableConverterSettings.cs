using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cornerstone.Database.Processes
{
    public class TableConverterSettings
    {

        public TableConverterSettings()
        {
        }

        public System.Configuration.ConnectionStringSettings SourceConnectionString { get; set; }
        public System.Configuration.ConnectionStringSettings TargetConnectionString { get; set; }

        public IList<Cornerstone.Database.Models.TableModel> SourceTables { get; set; }
        public IList<Cornerstone.Database.Models.TableModel> TargetTables { get; set; }

        public bool UseBulkCopy { get; set; }

        public int BatchSize { get; set; }

        public bool TrimStrings { get; set; }
        public bool UseTransaction { get; set; } = true;

    }
}
