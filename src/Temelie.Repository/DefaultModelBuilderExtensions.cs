using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Temelie.DependencyInjection;

namespace Temelie.Repository;

[ExportSingleton(typeof(IModelBuilderExtensions))]
public class DefaultModelBuilderExtensions : IModelBuilderExtensions
{
    public virtual void HasTrigger(TableBuilder tabeBuilder)
    {

    }

    public virtual void UseIdentityColumn<TProperty>(PropertyBuilder<TProperty> propertyBuilder)
    {
        propertyBuilder.ValueGeneratedOnAdd();
    }
}
