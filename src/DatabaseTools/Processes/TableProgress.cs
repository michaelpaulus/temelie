using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseTools.Processes
{
    public class TableProgress
    {
        public int ProgressPercentage { get; set; }
        public Models.TableModel Table { get; set; }
        public string ErrorMessage { get; set; }
        public Exception Exception { get; set; }
    }
}
