using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Temelie.Repository;
public static class Extensions
{

    public static PropertyBuilder<TProperty> UseIdentityColumn<TProperty>(this PropertyBuilder<TProperty> propertyBuilder, IModelBuilderExtensions extensions)
    {
        extensions.UseIdentityColumn(propertyBuilder);
        return propertyBuilder;
    }

    public static TableBuilder HasTrigger(this TableBuilder tableBuilder, IModelBuilderExtensions extensions)
    {
        extensions.HasTrigger(tableBuilder);
        return tableBuilder;
    }

}
