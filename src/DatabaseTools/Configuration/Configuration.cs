
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
		public class Configuration
		{

			private static string _baseDirectory;
			public static string BaseDirectory
			{
				get
				{
					if (string.IsNullOrEmpty(_baseDirectory))
					{
						_baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
						if (!(_baseDirectory.EndsWith("\\")))
						{
							_baseDirectory += "\\";
						}
						if (_baseDirectory.StartsWith("file:\\"))
						{
							_baseDirectory = _baseDirectory.Remove(0, 6);
						}
					}
					return _baseDirectory;
				}
			}

			private static string _configurationFile;
			private static string ConfigurationFile
			{
				get
				{
					if (string.IsNullOrEmpty(_configurationFile))
					{
						string strAssemblyBase = System.Reflection.Assembly.GetExecutingAssembly().FullName.Split(',')[0].Split('.')[0];
						_configurationFile = string.Format("{0}{1}.config", BaseDirectory, strAssemblyBase);
						if (!(System.IO.File.Exists(_configurationFile)))
						{
							_configurationFile = string.Format("{0}{1}.config", BaseDirectory, "web");
						}
						if (!(System.IO.File.Exists(_configurationFile)) && System.Reflection.Assembly.GetEntryAssembly() != null)
						{
							_configurationFile = string.Format("{0}{1}.config", BaseDirectory, System.IO.Path.GetFileName(System.Reflection.Assembly.GetEntryAssembly().CodeBase));
						}
						if (!(System.IO.File.Exists(_configurationFile)))
						{
							_configurationFile = string.Empty;
						}
					}
					return _configurationFile;
				}
			}

			private static System.Configuration.Configuration _current;
			public static System.Configuration.Configuration Current
			{
				get
				{
					if (_current == null)
					{
						string strConfigurationFile = Configuration.ConfigurationFile;
						if (!(string.IsNullOrEmpty(strConfigurationFile)))
						{
							if (_current == null)
							{
								System.Configuration.ExeConfigurationFileMap fileMap = new System.Configuration.ExeConfigurationFileMap();
								fileMap.ExeConfigFilename = strConfigurationFile;
								try
								{
									_current = System.Configuration.ConfigurationManager.OpenMappedExeConfiguration(fileMap, System.Configuration.ConfigurationUserLevel.None);
								}
								catch 
								{

								}
							}
						}
						if (_current == null)
						{
							_current = System.Configuration.ConfigurationManager.OpenExeConfiguration(System.Configuration.ConfigurationUserLevel.None);
						}
					}
					return _current;
				}
			}

		}
	}


}