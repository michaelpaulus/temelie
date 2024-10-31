using Microsoft.EntityFrameworkCore;
using Temelie.DependencyInjection;
using Temelie.Repository;

namespace AdventureWorks.Server.Repository;

[ExportTransient(typeof(IRepositoryContext))]
public partial class ExampleDbContext : DbContextBase, IRepositoryContext
{

    public ExampleDbContext(IModelBuilderContext context, DbContextOptions<ExampleDbContext> dbContextOptions) : base(context, dbContextOptions)
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
