using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cornerstone.Database.Processes
{
    public interface IConnectionCreatedNotification
    {
        void Notify(IDbConnection connection);
        
    }
}
