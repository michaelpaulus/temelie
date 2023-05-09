using Azure.Core;
using Azure.Identity;
using DatabaseTools.Processes;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace DatabaseTools.Providers
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
                    var credential = new DefaultAzureCredential(true);
                    var result = credential.GetToken(new TokenRequestContext(new string[] { "https://database.windows.net/.default" }));
                    csb.UserID = "";
                    csb.Password = "";
                    connection.ConnectionString = csb.ConnectionString;
                    sqlConnection.AccessToken = result.Token;
                }
            }
        }
    }
}
