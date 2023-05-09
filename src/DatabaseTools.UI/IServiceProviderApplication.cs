using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace DatabaseTools.UI
{
    public interface IServiceProviderApplication
    {

        public IServiceProvider ServiceProvider { get; set; }

    }
}
