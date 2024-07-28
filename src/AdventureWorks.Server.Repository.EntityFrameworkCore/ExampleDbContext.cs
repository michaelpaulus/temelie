using Microsoft.EntityFrameworkCore;

namespace AdventureWorks.Server.Repository.EntityFrameworkCore;

public partial class ExampleDbContext : DbContextBase
{

    public ExampleDbContext(IServiceProvider serviceProvider, DbContextOptions<ExampleDbContext> dbContextOptions) : base(serviceProvider, dbContextOptions)
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }

}
