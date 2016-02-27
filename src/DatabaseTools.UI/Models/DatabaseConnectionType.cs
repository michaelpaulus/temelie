using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DatabaseTools.Models
{
    public class DatabaseConnectionType : Model
    {

        public DatabaseConnectionType()
        {

        }

        public DatabaseConnectionType(string connectionType, string defaultConnectionString, string providerName)
        {
            ConnectionType = connectionType;
            DefaultConnectionString = defaultConnectionString;
            ProviderName = providerName;
        }

        public string ConnectionType { get; set; }
        public string DefaultConnectionString { get; set; }
        public string ProviderName { get; set; }

        public static IList<DatabaseConnectionType> GetDatabaseConnectionTypes()
        {
            var list = new List<DatabaseConnectionType>();

            list.Add(new DatabaseConnectionType("SQL Server", "Data Source=(local)\\MSSQL2014;Initial Catalog=Database;Integrated Security=True;User Id=;Password=;Encrypt=False;Application Name=DatabaseTools", "System.Data.SqlClient"));
            list.Add(new DatabaseConnectionType("MySql", "Database=database;Data Source=localhost;User Id=;Password=", "MySql.Data.MySqlClient"));
            list.Add(new DatabaseConnectionType("ODBC", "DSN=Database", "System.Data.Odbc"));
            list.Add(new DatabaseConnectionType("OLE", "Provider=;Data Source=", "System.Data.OleDb"));
            list.Add(new DatabaseConnectionType("OLE ACCESS", "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=", "System.Data.OleDb"));

            return list;
        }

    }
}
