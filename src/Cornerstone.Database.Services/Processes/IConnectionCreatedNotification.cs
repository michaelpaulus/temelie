using System.Data;

namespace Cornerstone.Database.Processes;

public interface IConnectionCreatedNotification
{
    void Notify(IDbConnection connection);

}
