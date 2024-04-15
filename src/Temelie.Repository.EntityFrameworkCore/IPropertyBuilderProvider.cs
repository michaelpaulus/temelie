using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Temelie.Repository.EntityFrameworkCore;
public interface IPropertyBuilderProvider
{
    void UseIdentityColumn<TProperty>(PropertyBuilder<TProperty> propertyBuilder);
}
