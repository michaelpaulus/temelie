using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseTools.Configuration
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ExportAttribute : Attribute
    {
        public Type ForInterface { get; set; }
        public string Name { get; set; }

        public ExportAttribute(Type forInterface)
        {
            ForInterface = forInterface;
        }

    }
}
