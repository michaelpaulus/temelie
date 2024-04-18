using System.Data;
using Temelie.Database.Services;
using Microsoft.Data.SqlClient;
using Temelie.DependencyInjection;

namespace Temelie.Database.Providers.Mssql;

[ExportTransient(typeof(IConnectionCreatedNotification))]
public class DefaultAzureCredentialConnectionCreatedNotification : IConnectionCreatedNotification
{
    public void Notify(IDbConnection connection)
    {
        if (connection is SqlConnection sqlConnection)
        {
            var csb = new SqlConnectionStringBuilder(sqlConnection.ConnectionString);
            if (!csb.IntegratedSecurity && (string.IsNullOrEmpty(csb.UserID) || string.IsNullOrEmpty(csb.Password)))
            {
                csb.UserID = "";
                csb.Password = "";
                connection.ConnectionString = csb.ConnectionString;
                sqlConnection.AccessToken = TokenService.GetToken("https://database.windows.net/.default");
            }
        }
    }
}
