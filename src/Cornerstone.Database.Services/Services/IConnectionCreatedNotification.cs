using System.Data;

namespace Cornerstone.Database.Services;

public interface IConnectionCreatedNotification
{
    void Notify(IDbConnection connection);

}
