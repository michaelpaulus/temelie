
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
		public class Mapping
		{

			protected string GetAttributeValue(System.Xml.XmlNode node, string name)
			{
				if (node.Attributes != null && node.Attributes[name] != null)
				{
					return node.Attributes[name].Value;
				}
				return string.Empty;
			}

		}
	}


}