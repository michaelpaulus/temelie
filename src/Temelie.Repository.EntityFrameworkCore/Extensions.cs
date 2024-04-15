using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.Extensions.DependencyInjection;

namespace Temelie.Repository.EntityFrameworkCore;
public static class Extensions
{

    public static PropertyBuilder<TProperty> UseIdentityColumn<TProperty>(this PropertyBuilder<TProperty> propertyBuilder, IServiceProvider serviceProvider)
    {
        var providers = serviceProvider.GetServices<IPropertyBuilderProvider>();

        if (!providers.Any())
        {
            throw new NotImplementedException($"Must register at least one {nameof(IPropertyBuilderProvider)} when using identities");
        }

        foreach (var provider in providers)
        {
            provider.UseIdentityColumn(propertyBuilder);
        }

        return propertyBuilder;
    }


}
