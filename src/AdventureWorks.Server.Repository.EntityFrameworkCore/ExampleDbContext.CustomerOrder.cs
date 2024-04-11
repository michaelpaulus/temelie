using Cornerstone.Example.Entities;
using Microsoft.EntityFrameworkCore;

namespace Cornerstone.Repository.EntityFrameworkCore.UnitTests;

public partial class ExampleDbContext : IRepositoryContext<CustomerOrder>
{
    public DbSet<CustomerOrder> CustomerOrders { get; set; }
    DbContext IRepositoryContext<CustomerOrder>.DbContext => this;
    DbSet<CustomerOrder> IRepositoryContext<CustomerOrder>.DbSet => CustomerOrders;
}

