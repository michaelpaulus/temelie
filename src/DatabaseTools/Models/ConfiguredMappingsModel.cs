
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
		public class ConfiguredMappingsModel
		{

			public ConfiguredMappingsModel(System.Xml.XmlDocument doc)
			{
				this.Initialize(doc);
			}

			private void Initialize(System.Xml.XmlDocument doc)
			{
				_rootMappings = new List<Models.ColumnMappingModel>();

				foreach (System.Xml.XmlNode node in doc.SelectNodes("/mappings/rootMappings/columnMapping"))
				{
					Models.ColumnMappingModel mapping = new Models.ColumnMappingModel();
					mapping.Parse(node);
					_rootMappings.Add(mapping);
				}

				_tableMappings = new List<Models.TableMapping>();

				foreach (System.Xml.XmlNode node in doc.SelectNodes("/mappings/tableGroup/table"))
				{
					Models.TableMapping mapping = new Models.TableMapping();
					mapping.Parse(node);
					_tableMappings.Add(mapping);
				}
			}

			private List<Models.TableMapping> _tableMappings;
			public List<Models.TableMapping> TableMappings
			{
				get
				{
					return _tableMappings;
				}
			}

			private List<Models.ColumnMappingModel> _rootMappings;
			public List<Models.ColumnMappingModel> RootMappings
			{
				get
				{
					return _rootMappings;
				}
			}
		}

	}


}