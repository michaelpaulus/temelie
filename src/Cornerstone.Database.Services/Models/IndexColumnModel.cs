using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cornerstone.Database.Models
{
    public class IndexColumnModel
    {
        public string ColumnName { get; set; }
        public bool IsDescending { get; set; }
        public int PartitionOrdinal { get; set; }
    }
}
