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
        modelBuilder.Entity<CustomerOrder>(builder =>
        {
            builder.HasKey(i => new { i.CustomerId, i.OrderId });
            builder.Property(p => p.OrderId)
                .HasConversion(id => id.Value, value => new OrderId(value));
            builder.Property(p => p.CustomerId)
                .HasConversion(id => id.Value, value => new CustomerId(value));
        });
    }

}
