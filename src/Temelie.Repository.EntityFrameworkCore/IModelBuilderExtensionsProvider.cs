using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Temelie.Repository.EntityFrameworkCore;
public interface IModelBuilderExtensionsProvider
{
    void UseIdentityColumn<TProperty>(PropertyBuilder<TProperty> propertyBuilder);
    void HasTrigger(TableBuilder tabeBuilder);
}
