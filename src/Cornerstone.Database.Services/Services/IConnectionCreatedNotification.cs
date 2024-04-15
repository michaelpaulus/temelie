using System.Data;

namespace Temelie.Database.Services;

public interface IConnectionCreatedNotification
{
    void Notify(IDbConnection connection);

}
