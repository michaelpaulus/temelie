using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace DatabaseTools.Configuration.Preferences
{
    public class UserSettingsContext
    {

        static UserSettingsContext()
        {
            UserSettingsPath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "DatabaseTools");
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
                    settings = JsonSerializer.Deserialize<UserSettings>(System.IO.File.ReadAllText(strFileName), new JsonSerializerOptions() { PropertyNameCaseInsensitive = true });
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

        private static UserSettings _current;
        public static UserSettings Current
        {
            get
            {
                return _current;
            }
        }

        private static string GetFileName()
        {
            return System.IO.Path.Combine(UserSettingsPath, "databaseTools.json");
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
}
