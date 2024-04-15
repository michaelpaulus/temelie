using System;
using System.Text;
using System.Text.Json;

namespace Temelie.Database.Configuration.Preferences;

public class UserSettingsContext
{

#pragma warning disable CA1810 // Initialize reference type static fields inline
    static UserSettingsContext()
#pragma warning restore CA1810 // Initialize reference type static fields inline
    {
        UserSettingsPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Temelie.Database");
        if (!(System.IO.Directory.Exists(UserSettingsPath)))
        {
            System.IO.Directory.CreateDirectory(UserSettingsPath);
        }

        UserSettings settings = null;
        string strFileName = GetFileName();
        if (System.IO.File.Exists(strFileName))
        {
            try
            {
                var json = System.IO.File.ReadAllText(strFileName);
                settings = JsonSerializer.Deserialize<UserSettings>(json, new JsonSerializerOptions()
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
                    WriteIndented = true,
                    DictionaryKeyPolicy = JsonNamingPolicy.CamelCase
                });
            }
            catch
            {

            }
        }
        if (settings == null)
        {
            settings = new UserSettings();
        }

        _current = settings;
    }

    public static string UserSettingsPath;

    private static readonly UserSettings _current;
    public static UserSettings Current
    {
        get
        {
            return _current;
        }
    }

    private static string GetFileName()
    {
        return System.IO.Path.Combine(UserSettingsPath, "Temelie.Database.json");
    }

    public static void Save()
    {
        string strFileName = GetFileName();

        var fileContents = JsonSerializer.Serialize(Current, new JsonSerializerOptions()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
            WriteIndented = true,
            DictionaryKeyPolicy = JsonNamingPolicy.CamelCase
        });

        if (System.IO.File.Exists(strFileName))
        {
            System.IO.File.Delete(strFileName);
        }

        System.IO.File.WriteAllText(strFileName, fileContents, Encoding.UTF8);
    }

}
