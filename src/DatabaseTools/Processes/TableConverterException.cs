using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseTools.Processes
{
    public class TableConverterException : Exception 
    {

        public TableConverterException(Models.TableModel table, string message) : base(message)
        {
            this.Table = table;
        }

        public TableConverterException(Models.TableModel table, string message, Exception innerException) : base(message, innerException)
        {
            this.Table = table;
        }

        public Models.TableModel Table { get; set; }

    }
}
