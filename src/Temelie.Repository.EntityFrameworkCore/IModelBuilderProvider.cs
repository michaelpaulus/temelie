using Microsoft.EntityFrameworkCore;

namespace Temelie.Repository.EntityFrameworkCore;
public interface IModelBuilderProvider
{
    void OnModelCreating(ModelBuilder modelBuilder);
}
