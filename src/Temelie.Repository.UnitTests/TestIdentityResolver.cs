using Temelie.DependencyInjection;

namespace Temelie.Repository.UnitTests;

[ExportTransient(typeof(IIdentityResolver))]
public class TestIdentityResolver : IIdentityResolver
{
    public string GetIdentity()
    {
        return "";
    }
}
