
using Temelie.DependencyInjection;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Temelie.Repository.EntityFrameworkCore.UnitTests;

[ExportTransient(typeof(IPropertyBuilderProvider))]
public class SqlitePropertyBuilderProvider : IPropertyBuilderProvider
{
    public void UseIdentityColumn<TProperty>(PropertyBuilder<TProperty> propertyBuilder)
    {
        propertyBuilder.ValueGeneratedOnAdd();
    }
}
