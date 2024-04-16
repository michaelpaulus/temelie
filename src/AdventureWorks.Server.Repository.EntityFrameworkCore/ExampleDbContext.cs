using Microsoft.EntityFrameworkCore;

namespace AdventureWorks.Server.Repository.EntityFrameworkCore;

public class ExampleDbContext : BaseDbContext
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
