
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
	namespace Models
	{
		public abstract class DatabaseObjectModel : Model
		{
			public abstract void AppendDropScript(DatabaseModel database, System.Text.StringBuilder sb, string quoteCharacterStart, string quoteCharacterEnd);
			public abstract void AppendCreateScript(DatabaseModel database, System.Text.StringBuilder sb, string quoteCharacterStart, string quoteCharacterEnd);

		}
	}


}