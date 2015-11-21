
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
			static ColumnMappingModel()
			{
				ColumnMappingList = new string[] {"None", "%CHAR_TO_BOOLEAN%", "%STATUS_TO_INACTIVE%", "%ACTIVE_INACTIVE_TO_INACTIVE%", "%PAYMENT_METHOD%", "[GLFiscalYear]", "[GLLedger]"};
			}

			public static string[] ColumnMappingList;

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
					else
					{
						if (strColumnMapping.Contains("["))
						{
							//Preference Lookup
							string strPreferenceName = strColumnMapping.Substring(strColumnMapping.IndexOf("[") + 1, strColumnMapping.IndexOf("]") - strColumnMapping.IndexOf("[") - 1);
							strColumnMapping = strColumnMapping.Replace("[" + strPreferenceName + "]", string.Format("(SELECT preference_value FROM GetPreferenceValue('{0}', NULL))", strPreferenceName));
						}
						else if (strColumnMapping.Equals("%STATUS_TO_INACTIVE%"))
						{
							strColumnMapping = "CASE ISNULL({0}, 'A') WHEN 'A' THEN 0 ELSE 1 END";
						}
						else if (strColumnMapping.Equals("%CHAR_TO_BOOLEAN%"))
						{
							strColumnMapping = "CASE ISNULL({0}, '') WHEN 'Y' THEN 1 ELSE 0 END";
						}
						else if (strColumnMapping.Equals("%PAYMENT_METHOD%"))
						{
							strColumnMapping = "CASE {0} WHEN 'CREDIT' THEN 'CREDIT CARD' WHEN 'CASH' THEN 'CASH' WHEN 'CHECK' THEN 'CHECK' ELSE 'ON ACCOUNT' END";
						}
						else if (strColumnMapping.Equals("%ACTIVE_INACTIVE_TO_INACTIVE%"))
						{
							strColumnMapping = "CASE ISNULL({0}, 'A') WHEN 'A' THEN 0 ELSE 1 END";
						}
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