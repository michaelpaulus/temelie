using Temelie.DependencyInjection;

namespace Temelie.Repository;

[ExportSingleton(typeof(IModelBuilderContext))]
public class DefaultModelBuilderContext : IModelBuilderContext
{

    public DefaultModelBuilderContext(IEnumerable<IModelBuilderProvider> modelBuilderProviders)
    {
        Providers = modelBuilderProviders;
    }

    public IEnumerable<IModelBuilderProvider> Providers { get; }

}
