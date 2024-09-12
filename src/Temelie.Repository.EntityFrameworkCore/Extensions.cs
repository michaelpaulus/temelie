using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.Extensions.DependencyInjection;

namespace Temelie.Repository.EntityFrameworkCore;
public static class Extensions
{

    public static PropertyBuilder<TProperty> UseIdentityColumn<TProperty>(this PropertyBuilder<TProperty> propertyBuilder, IServiceProvider serviceProvider)
    {
        var providers = serviceProvider.GetServices<IModelBuilderExtensionsProvider>();

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

    public static TableBuilder HasTrigger(this TableBuilder tableBuilder, IServiceProvider serviceProvider)
    {
        var providers = serviceProvider.GetServices<IModelBuilderExtensionsProvider>();

        foreach (var provider in providers)
        {
            provider.HasTrigger(tableBuilder);
        }

        return tableBuilder;
    }

}
