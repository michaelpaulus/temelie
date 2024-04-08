
using Cornerstone.Database.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Xml.Linq;

namespace Cornerstone.Database
{
	namespace Models
	{
		public enum DatabaseType
		{
			AccessOLE,
			MicrosoftSQLServer,
			MicrosoftSQLServerCompact,
            MySql,
			Odbc,
			Oracle,
			OLE
		}
	}


}