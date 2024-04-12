using AdventureWorks.Entities;
using Microsoft.EntityFrameworkCore;

namespace Cornerstone.Repository.EntityFrameworkCore.UnitTests;

public partial class ExampleDbContext : DbContext
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.Entity<Customer>(builder =>
        {
            builder.HasKey(i => i.CustomerId);
            builder.Property(p => p.CustomerId)
                .HasConversion(id => id.Value, value => new CustomerId(value));
        });
        modelBuilder.Entity<BusinessEntityAddress>(builder =>
        {
            builder.HasKey(i => new { i.BusinessEntityId, i.AddressId, i.AddressTypeId });
            builder.Property(p => p.BusinessEntityId)
                .HasConversion(id => id.Value, value => new BusinessEntityId(value));
            builder.Property(p => p.AddressId)
                .HasConversion(id => id.Value, value => new AddressId(value));
            builder.Property(p => p.AddressTypeId)
                .HasConversion(id => id.Value, value => new AddressTypeId(value));
        });
    }

}
