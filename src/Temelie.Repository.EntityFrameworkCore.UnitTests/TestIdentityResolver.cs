using Temelie.DependencyInjection;

namespace Temelie.Repository.EntityFrameworkCore.UnitTests;

[ExportTransient(typeof(IIdentityResolver))]
public class TestIdentityResolver : IIdentityResolver
{
    public string GetIdentity()
    {
        return "";
    }
}
