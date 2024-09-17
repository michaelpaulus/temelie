using Microsoft.EntityFrameworkCore;

namespace Temelie.Repository;
public interface IModelBuilderProvider
{
    void OnModelCreating(ModelBuilder modelBuilder);
}
