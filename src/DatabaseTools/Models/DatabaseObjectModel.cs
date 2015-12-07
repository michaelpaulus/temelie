
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
			public abstract void AppendDropScript(System.Text.StringBuilder sb, string quoteCharacter);
			public abstract void AppendCreateScript(System.Text.StringBuilder sb, string quoteCharacter);

		}
	}


}