using Microsoft.EntityFrameworkCore;
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
            propertyBuilder.ValueGeneratedOnAdd();
        }
        else
        {
            foreach (var provider in providers)
            {
                provider.UseIdentityColumn(propertyBuilder);
            }
        }
        return propertyBuilder;
    }

#if NETSTANDARD

    public static PropertyBuilder<TProperty> HasPrecision<TProperty>(this PropertyBuilder<TProperty> propertyBuilder, int precision, int scale)
    {
        propertyBuilder.HasColumnType($"DECIMAL({precision},{scale})");
        return propertyBuilder;
    }

#endif


}
