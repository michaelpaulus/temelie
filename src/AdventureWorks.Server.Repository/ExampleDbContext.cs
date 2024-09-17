using Microsoft.EntityFrameworkCore;
using Temelie.DependencyInjection;
using Temelie.Repository;

namespace AdventureWorks.Server.Repository;

[ExportTransient(typeof(IRepositoryContext))]
public partial class ExampleDbContext : DbContextBase, IRepositoryContext
{

    public ExampleDbContext(IServiceProvider serviceProvider, DbContextOptions<ExampleDbContext> dbContextOptions) : base(serviceProvider, dbContextOptions)
    {
    }

    public DbContext DbContext => this;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }

}
