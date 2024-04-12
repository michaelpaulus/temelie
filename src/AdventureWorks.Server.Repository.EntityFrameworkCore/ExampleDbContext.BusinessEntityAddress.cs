using AdventureWorks.Entities;
using Microsoft.EntityFrameworkCore;

namespace Cornerstone.Repository.EntityFrameworkCore.UnitTests;

public partial class ExampleDbContext : IRepositoryContext<BusinessEntityAddress>
{
    public DbSet<BusinessEntityAddress> BusinessEntityAddress { get; set; }
    DbContext IRepositoryContext<BusinessEntityAddress>.DbContext => this;
    DbSet<BusinessEntityAddress> IRepositoryContext<BusinessEntityAddress>.DbSet => BusinessEntityAddress;
}

