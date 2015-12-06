
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
		public class ColumnMappingModel : Mapping
		{
		
			private string _sourceColumnName;
			public string SourceColumnName
			{
				get
				{
					return _sourceColumnName;
				}
				set
				{
					_sourceColumnName = value;
				}
			}

			private string _targetColumnName;
			public string TargetColumnName
			{
				get
				{
					return _targetColumnName;
				}
				set
				{
					_targetColumnName = value;
				}
			}

			private string _columnMapping;
			public string ColumnMapping
			{
				get
				{
					return _columnMapping.IsNull("");
				}
				set
				{
					if (value == null)
					{
						value = "";
					}
					if (value.Equals("None"))
					{
						value = string.Empty;
					}
					this._columnMapping = value;
				}
			}

			public void Parse(System.Xml.XmlNode xmlNode)
			{
				this.SourceColumnName = this.GetAttributeValue(xmlNode, "sourceColumnName");
				this.TargetColumnName = this.GetAttributeValue(xmlNode, "targetColumnName");
				this.ColumnMapping = this.GetAttributeValue(xmlNode, "columnMapping");
			}

			public bool IsTargetColumnIdentity {get; set;}

			public string TargetColumnNameWithMapping
			{
				get
				{
					string strColumnName = this.TargetColumnName;

					if (strColumnName.Contains(" ") || strColumnName.Contains("-"))
					{
						strColumnName = "[" + strColumnName + "]";
					}

					return strColumnName;
				}
			}

			public string SourceColumnNameWithMapping
			{
				get
				{
					string strColumnMapping = this.ColumnMapping;

					if (string.IsNullOrEmpty(this.ColumnMapping))
					{
						strColumnMapping = "{0}";
					}
				
					string strColumnName = this.SourceColumnName;

					if (strColumnName.Contains(" ") || strColumnName.Contains("-"))
					{
						strColumnName = "[" + strColumnName + "]";
					}

					return string.Format(strColumnMapping, strColumnName);
				}
			}

			public override string ToString()
			{
				if (string.IsNullOrEmpty(this.ColumnMapping))
				{
					return string.Format("{0} --> {1}", this.SourceColumnName, this.TargetColumnName);
				}
				else
				{
					return string.Format("{0} --> {1} ({2})", this.SourceColumnName, this.TargetColumnName, this.ColumnMapping);
				}
			}

		}
	}


}