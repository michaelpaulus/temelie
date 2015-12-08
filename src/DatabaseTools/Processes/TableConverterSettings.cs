using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseTools.Processes
{
    public class TableConverterSettings
    {
        public System.Configuration.ConnectionStringSettings SourceConnectionString { get; set; }
        public System.Configuration.ConnectionStringSettings TargetConnectionString { get; set; }

        public IList<DatabaseTools.Models.TableModel> SourceTables { get; set; }
        public IList<DatabaseTools.Models.TableModel> TargetTables { get; set; }

        public bool UseBulkCopy { get; set; }
        public bool TrimStrings { get; set; }
    }
}
