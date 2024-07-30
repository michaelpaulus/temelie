using System.Data.Common;
using MySqlConnector;

namespace Temelie.Database.Providers.MySql;
internal class MySqlDbProviderFactory : DbProviderFactory
{

    public override DbConnection CreateConnection()
    {
        return new MySqlConnection();
    }

    public override DbCommand CreateCommand()
    {
        return new MySqlCommand();
    }

    public override DbConnectionStringBuilder CreateConnectionStringBuilder()
    {
        return new MySqlConnectionStringBuilder();
    }

    public override DbParameter CreateParameter()
    {
        return new MySqlParameter();
    }

}
