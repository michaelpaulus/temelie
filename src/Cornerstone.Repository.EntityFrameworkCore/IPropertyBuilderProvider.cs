using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cornerstone.Repository.EntityFrameworkCore;
public interface IPropertyBuilderProvider
{
    void UseIdentityColumn<TProperty>(PropertyBuilder<TProperty> propertyBuilder);
}
