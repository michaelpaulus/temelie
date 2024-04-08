using System;
using System.Configuration;
using System.Linq;

namespace Cornerstone.Database.Configuration.Preferences;

public class ConnectionSettingsContext
{

    private static System.Configuration.Configuration GetConfiguration()
    {
        ConnectionSettingsPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Cornerstone.Database");
        if (!(System.IO.Directory.Exists(ConnectionSettingsPath)))
        {
            System.IO.Directory.CreateDirectory(ConnectionSettingsPath);
        }

        string strFileName = GetFileName();

        System.Configuration.ExeConfigurationFileMap fileMap = new System.Configuration.ExeConfigurationFileMap();
        fileMap.ExeConfigFilename = strFileName;

        System.Configuration.Configuration configuration;
        if (!System.IO.File.Exists(strFileName))
        {
            configuration = System.Configuration.ConfigurationManager.OpenMappedExeConfiguration(fileMap, ConfigurationUserLevel.None);
            foreach (var item in configuration.ConnectionStrings.ConnectionStrings.OfType<System.Configuration.ConnectionStringSettings>())
            {
                configuration.ConnectionStrings.ConnectionStrings.Remove(item);
            }
            SaveConfiguraiton(configuration);
        }

        configuration = System.Configuration.ConfigurationManager.OpenMappedExeConfiguration(fileMap, System.Configuration.ConfigurationUserLevel.None);

        return configuration;
    }

    private static void SaveConfiguraiton(System.Configuration.Configuration configuration)
    {
        EncryptConfigSection(configuration, "connectionStrings");
        configuration.Save();
    }

    static ConnectionSettingsContext()
    {
        _current = new ConnectionSettings();

        var connectionStrings = GetConfiguration().ConnectionStrings.ConnectionStrings.OfType<System.Configuration.ConnectionStringSettings>();

        if (!connectionStrings.Any() &&
            UserSettingsContext.Current.Connections.Any())
        {

            foreach (var connection in UserSettingsContext.Current.Connections)
            {
                _current.Connections.Add(connection);
            }

            UserSettingsContext.Current.Connections.Clear();

            UserSettingsContext.Save();

            Save();
        }
        else
        {
            foreach (var item in connectionStrings)
            {
                _current.Connections.Add(new Models.DatabaseConnection()
                {
                    Name = item.Name,
                    ConnectionString = item.ConnectionString,
                    ConnectionType = (from i in Models.DatabaseConnectionType.GetDatabaseConnectionTypes() where i.ProviderName == item.ProviderName select i.ConnectionType).FirstOrDefault()
                });
            }
        }

    }

    public static string ConnectionSettingsPath;

    private static readonly ConnectionSettings _current;
    public static ConnectionSettings Current
    {
        get
        {
            return _current;
        }
    }

    private static string GetFileName()
    {
        return System.IO.Path.Combine(ConnectionSettingsPath, "Cornerstone.Database.config");
    }

    public static void Save()
    {
        var configuration = GetConfiguration();

        configuration.ConnectionStrings.ConnectionStrings.Clear();

        foreach (var item in Current.Connections)
        {
            configuration.ConnectionStrings.ConnectionStrings.Add(new System.Configuration.ConnectionStringSettings()
            {
                Name = item.Name,
                ConnectionString = item.ConnectionString,
                ProviderName = (from i in Models.DatabaseConnectionType.GetDatabaseConnectionTypes() where i.ConnectionType == item.ConnectionType select i.ProviderName).FirstOrDefault()
            });
        }

        SaveConfiguraiton(configuration);
    }

    private static void EncryptConfigSection(System.Configuration.Configuration configuration, string sectionKey)
    {
        ConfigurationSection section = configuration.GetSection(sectionKey);
        if (section != null)
        {
            if (!section.SectionInformation.IsProtected)
            {
                if (!section.ElementInformation.IsLocked)
                {
                    section.SectionInformation.ProtectSection("DataProtectionConfigurationProvider");
                    section.SectionInformation.ForceSave = true;
                    configuration.Save();
                }
            }
        }
    }

}
