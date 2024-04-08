using Cornerstone.Example.Entities;
using Microsoft.EntityFrameworkCore;

namespace Cornerstone.Repository.EntityFrameworkCore.UnitTests;

public partial class ExampleDbContext : IRepositoryContext<Customer>
{
    public DbSet<Customer> Customers { get; set; }
    DbContext IRepositoryContext<Customer>.DbContext => this;
    DbSet<Customer> IRepositoryContext<Customer>.DbSet => Customers;
}

