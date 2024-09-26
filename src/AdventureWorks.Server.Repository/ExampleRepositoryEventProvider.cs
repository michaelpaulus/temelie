using AdventureWorks.Entities;
using Microsoft.Extensions.Logging;
using Temelie.DependencyInjection;
using Temelie.Repository;

namespace AdventureWorks.Server.Repository;
[ExportProvider(typeof(IRepositoryEventProvider<Person>))]
public class ExampleRepositoryEventProvider : IRepositoryEventProvider<Person>
{
    private readonly ILogger<ExampleRepositoryEventProvider> _logger;

    public ExampleRepositoryEventProvider(ILogger<ExampleRepositoryEventProvider> logger)
    {
        _logger = logger;
    }

    public Task OnAddedAsync(Person entity)
    {
        _logger.LogInformation("Person {0} added", entity.FirstName);
        return Task.CompletedTask;
    }

    public Task OnAddingAsync(Person entity)
    {
        _logger.LogInformation("Person {0} adding", entity.FirstName);
        return Task.CompletedTask;
    }

    public Task OnDeletedAsync(Person entity)
    {
        _logger.LogInformation("Person {0} Deleted", entity.FirstName);
        return Task.CompletedTask;
    }

    public Task OnDeletingAsync(Person entity)
    {
        _logger.LogInformation("Person {0} Deleting", entity.FirstName);
        return Task.CompletedTask;
    }

    public Task<IQueryable<Person>> OnQueryAsync(IRepositoryContext context, IQueryable<Person> query)
    {
        _logger.LogInformation("Person OnQueryAsync");
        return Task.FromResult(query);
    }

    public Task OnUpdatedAsync(Person entity)
    {
        _logger.LogInformation("Person {0} Updated", entity.FirstName);
        return Task.CompletedTask;
    }

    public Task OnUpdatingAsync(Person entity)
    {
        _logger.LogInformation("Person {0} Updating", entity.FirstName);
        return Task.CompletedTask;
    }
}
