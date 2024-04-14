
using Cornerstone.DependencyInjection;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cornerstone.Repository.EntityFrameworkCore.UnitTests;

[ExportTransient(typeof(IPropertyBuilderProvider))]
public class SqlitePropertyBuilderProvider : IPropertyBuilderProvider
{
    public void UseIdentityColumn<TProperty>(PropertyBuilder<TProperty> propertyBuilder)
    {
        propertyBuilder.ValueGeneratedOnAdd();
    }
}
