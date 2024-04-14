using Microsoft.EntityFrameworkCore;

namespace AdventureWorks.Server.Repository.EntityFrameworkCore;

public partial class ExampleDbContext : BaseDbContext
{

    public ExampleDbContext(IServiceProvider serviceProvider) : base(serviceProvider)
    {

    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    }

}
