
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
		public class Model : System.ComponentModel.INotifyPropertyChanged
		{
#region Methods

			protected string GetStringValue(DataRow row, string columnName)
			{
				string value = string.Empty;

				try
				{
					if (row.Table.Columns.Contains(columnName) && !(row.IsNull(columnName)))
					{
						value = Convert.ToString(row[columnName]);
					}
				}
				catch (Exception ex)
				{

				}

				return value;
			}

			protected Int32 GetInt32Value(DataRow row, string columnName)
			{
				Int32 value = 0;

				try
				{
					var strValue = this.GetStringValue(row, columnName);
					if (!(string.IsNullOrEmpty(strValue)))
					{
						int.TryParse(strValue, out value);
					}
				}
				catch (Exception ex)
				{

				}

				return value;
			}

			protected bool GetBoolValue(DataRow row, string columnName)
			{
				bool value = false;

				try
				{
					var strValue = this.GetStringValue(row, columnName);

					if (!(string.IsNullOrEmpty(strValue)))
					{
						if (strValue.EqualsIgnoreCase("Yes") || strValue.EqualsIgnoreCase("1"))
						{
							strValue = "True";
						}
						else if (strValue.EqualsIgnoreCase("No") || strValue.EqualsIgnoreCase("0"))
						{
							strValue = "False";
						}
						bool.TryParse(strValue, out value);
					}
				}
				catch (Exception ex)
				{

				}

				return value;
			}

#endregion

			protected virtual void OnPropertyChanged(string propertyName)
			{
				if (PropertyChanged != null)
					PropertyChanged(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
			}

			public event System.ComponentModel.PropertyChangedEventHandler PropertyChanged;

		}
	}


}