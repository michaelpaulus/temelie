using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cornerstone.Database.Processes
{
    public class ScriptProgress
    {
        public int ProgressPercentage { get; set; }
        public string ProgressStatus { get; set; }
        public string ErrorMessage { get; set; }
    }
}
