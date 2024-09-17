using Microsoft.EntityFrameworkCore;

namespace Temelie.Repository;

public interface IRepositoryContext : IDisposable
{
    DbContext DbContext { get; }
}
