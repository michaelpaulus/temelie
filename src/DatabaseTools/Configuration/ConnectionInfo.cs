
using DatabaseTools.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Xml.Linq;

namespace DatabaseTools
{
	namespace Configuration
	{
		public class ConnectionInfo
		{

			public static System.Configuration.ConnectionStringSettings GetConnectionStringSetting(string connectionStringName)
			{
				return Configuration.Current.ConnectionStrings.ConnectionStrings[connectionStringName];
			}

		}
	}


}