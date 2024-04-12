using AdventureWorks.Entities;
using AdventureWorks.Repository;
using FluentAssertions;
using Microsoft.Data.Sqlite;

namespace Cornerstone.Repository.EntityFrameworkCore.UnitTests;

public class RespositoryTests
{

    private SqliteConnection _connection;

    [SetUp]
    public async Task SetupAsync()
    {
        //create global connection for test so each test uses the same "database"
        _connection = new SqliteConnection("Filename=:memory:");
        _connection.Open();
        using (var context = new TestDbContext(_connection))
        {
            await context.Database.EnsureCreatedAsync().ConfigureAwait(true);
        }
    }

    [Test]
    public async Task AddSingleKeyAsync()
    {
        using var repository = new Repository<Customer>(new TestDbContext(_connection));

        var customer = new Customer() { CustomerId = new CustomerId(1), AccountNumber = "Test" };

        await repository.AddAsync(customer).ConfigureAwait(true);

        var result = await repository.GetSingleAsync(new CustomerSingleQuery(customer.CustomerId)).ConfigureAwait(true);

        result.Should().NotBeNull();
    }

    [Test]
    public async Task UpdateSingleKeyAsync()
    {
        using var repository = new Repository<Customer>(new TestDbContext(_connection));

        var customer = new Customer() { CustomerId = new CustomerId(1), AccountNumber = "Test" };

        await repository.AddAsync(customer).ConfigureAwait(true);

        customer.AccountNumber = "Test2";

        await repository.UpdateAsync(customer).ConfigureAwait(true);

        var result = await repository.GetSingleAsync(new CustomerSingleQuery(customer.CustomerId)).ConfigureAwait(true);

        result.Should().NotBeNull();

        result!.AccountNumber.Should().Be(customer.AccountNumber);
    }

    [Test]
    public async Task DeleteSingleKeyAsync()
    {
        using var repository = new Repository<Customer>(new TestDbContext(_connection));

        var customer = new Customer() { CustomerId = new CustomerId(1), AccountNumber = "Test" };

        await repository.AddAsync(customer).ConfigureAwait(true);

        await repository.DeleteAsync(customer).ConfigureAwait(true);

        var result = await repository.GetSingleAsync(new CustomerSingleQuery(customer.CustomerId)).ConfigureAwait(true);

        result.Should().BeNull();
    }

    [Test]
    public async Task AddComplexKeyAsync()
    {
        using var repository = new Repository<BusinessEntityAddress>(new TestDbContext(_connection));

        var address = new BusinessEntityAddress() { BusinessEntityId = new BusinessEntityId(1), AddressId = new AddressId(1), AddressTypeId = new AddressTypeId(1), ModifiedDate = DateTime.UtcNow };

        await repository.AddAsync(address).ConfigureAwait(true);

        var result = await repository.GetSingleAsync(new BusinessEntityAddressSingleQuery(address.BusinessEntityId, address.AddressId, address.AddressTypeId)).ConfigureAwait(true);

        result.Should().NotBeNull();
    }

    [Test]
    public async Task UpdateComplexKeyAsync()
    {
        using var repository = new Repository<BusinessEntityAddress>(new TestDbContext(_connection));

        var address = new BusinessEntityAddress() { BusinessEntityId = new BusinessEntityId(1), AddressId = new AddressId(1), AddressTypeId = new AddressTypeId(1), ModifiedDate = DateTime.UtcNow };

        await repository.AddAsync(address).ConfigureAwait(true);

        address.ModifiedDate = DateTime.UtcNow;

        await repository.UpdateAsync(address).ConfigureAwait(true);

        var result = await repository.GetSingleAsync(new BusinessEntityAddressSingleQuery(address.BusinessEntityId, address.AddressId, address.AddressTypeId)).ConfigureAwait(true);

        result.Should().NotBeNull();

        result!.ModifiedDate.Should().Be(address.ModifiedDate);
    }

    [Test]
    public async Task DeleteComplexKeyAsync()
    {
        using var repository = new Repository<BusinessEntityAddress>(new TestDbContext(_connection));

        var address = new BusinessEntityAddress() { BusinessEntityId = new BusinessEntityId(1), AddressId = new AddressId(1), AddressTypeId = new AddressTypeId(1), ModifiedDate = DateTime.UtcNow };

        await repository.AddAsync(address).ConfigureAwait(true);

        await repository.DeleteAsync(address).ConfigureAwait(true);

        var result = await repository.GetSingleAsync(new BusinessEntityAddressSingleQuery(address.BusinessEntityId, address.AddressId, address.AddressTypeId)).ConfigureAwait(true);

        result.Should().BeNull();
    }

    [Test]
    public async Task AddRangeAsync()
    {
        using var repository = new Repository<Customer>(new TestDbContext(_connection));

        var list = new List<Customer>();

        var count = 10;

        foreach (var i in Enumerable.Range(1, count))
        {
            var customer = new Customer() { CustomerId = new CustomerId(i), AccountNumber = "Test" };
            list.Add(customer);
        }

        await repository.AddRangeAsync(list).ConfigureAwait(true);

        var result = await repository.GetCountAsync(new CustomerByAccountNumberQuery("Test")).ConfigureAwait(true);

        result.Should().Be(count);
    }

    [Test]
    public async Task UpdateRangeAsync()
    {
        var list = new List<Customer>();
        var count = 10;

        using (var repository = new Repository<Customer>(new TestDbContext(_connection)))
        {
            foreach (var i in Enumerable.Range(1, count))
            {
                var customer = new Customer() { CustomerId = new CustomerId(i), AccountNumber = "Test" };
                list.Add(customer);
            }

            await repository.AddRangeAsync(list).ConfigureAwait(true);

            var result = await repository.GetCountAsync(new CustomerByAccountNumberQuery("Test")).ConfigureAwait(true);

            result.Should().Be(count);
        }

        using (var repository = new Repository<Customer>(new TestDbContext(_connection)))
        {
            foreach (var item in list)
            {
                item.AccountNumber = "Test1";
            }

            await repository.UpdateRangeAsync(list).ConfigureAwait(true);

            var result = await repository.GetCountAsync(new CustomerByAccountNumberQuery("Test1")).ConfigureAwait(true);

            result.Should().Be(count);
        }
    }

    [Test]
    public async Task DeleteRangeAsync()
    {
        using var repository = new Repository<Customer>(new TestDbContext(_connection));

        var list = new List<Customer>();

        var count = 10;

        foreach (var i in Enumerable.Range(1, count))
        {
            var customer = new Customer() { CustomerId = new CustomerId(i), AccountNumber = "Test" };
            list.Add(customer);
        }

        await repository.AddRangeAsync(list).ConfigureAwait(true);

        var result = await repository.GetCountAsync(new CustomerByAccountNumberQuery("Test")).ConfigureAwait(true);

        result.Should().Be(count);

        await repository.DeleteRangeAsync(list).ConfigureAwait(true);

        result = await repository.GetCountAsync(new CustomerByAccountNumberQuery("Test")).ConfigureAwait(true);

        result.Should().Be(0);
    }

    [Test]
    public async Task GetListAsync()
    {
        using var repository = new Repository<Customer>(new TestDbContext(_connection));
        var count = 10;
        foreach (var i in Enumerable.Range(1, count))
        {
            var customer = new Customer() { CustomerId = new CustomerId(i), AccountNumber = $"Test" };
            await repository.AddAsync(customer).ConfigureAwait(true);
        }

        var result = await repository.GetListAsync(new CustomerByAccountNumberQuery("Test")).ConfigureAwait(true);

        result.Should().HaveCount(count);
    }

    [Test]
    public async Task GetCountAsync()
    {
        using var repository = new Repository<Customer>(new TestDbContext(_connection));

        var count = 10;

        foreach (var i in Enumerable.Range(1, count))
        {
            var customer = new Customer() { CustomerId = new CustomerId(i), AccountNumber = $"Test" };
            await repository.AddAsync(customer).ConfigureAwait(true);
        }

        var result = await repository.GetCountAsync(new CustomerByAccountNumberQuery("Test")).ConfigureAwait(true);

        result.Should().Be(count);
    }

    [TearDown]
    public async Task TearDownAsync()
    {
        if (_connection is not null)
        {
            using (var context = new TestDbContext(_connection))
            {
                await context.DisposeAsync().ConfigureAwait(true);
            }
            _connection.Dispose();
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            _connection = null;
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
        }
    }

}
