using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Temelie.Repository;
public interface IModelBuilderExtensions
{
    void UseIdentityColumn<TProperty>(PropertyBuilder<TProperty> propertyBuilder);
    void HasTrigger(TableBuilder tabeBuilder);
}
