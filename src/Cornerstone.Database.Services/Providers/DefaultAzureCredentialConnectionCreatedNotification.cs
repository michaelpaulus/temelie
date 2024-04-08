using Azure.Core;
using Azure.Identity;
using Cornerstone.Database.Processes;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Cornerstone.Database.Providers
{
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
                    sqlConnection.AccessToken =  TokenService.GetToken("https://database.windows.net/.default");
                }
            }
        }
    }
}
