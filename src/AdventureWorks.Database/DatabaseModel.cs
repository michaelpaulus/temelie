using Cornerstone.Database.Models;

namespace AdventureWorks.Database;
public class Database
{
    private static readonly Lazy<DatabaseModel> _model = new Lazy<DatabaseModel>(() =>
    {
        var assembly = typeof(Database).Assembly;
        return DatabaseModel.CreateFromAssembly(assembly);
    });

    public static DatabaseModel Model => _model.Value;
}
