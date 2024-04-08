namespace Cornerstone.Database
{
    namespace Configuration
    {
        public class ConnectionInfo
        {

            public static System.Configuration.ConnectionStringSettings GetConnectionStringSetting(string connectionStringName)
            {
                return Configuration.Current.ConnectionStrings.ConnectionStrings[connectionStringName];
            }

        }
    }

}
