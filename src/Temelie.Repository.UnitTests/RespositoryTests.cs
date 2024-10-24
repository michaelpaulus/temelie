using AdventureWorks.Entities;
using AdventureWorks.Server.Repository;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace Temelie.Repository.UnitTests;

public class RespositoryTests : TestBase
{

    [Test]
    public async Task AddSingleKeyIdentityIntAsync()
    {
        var repository = ServiceProvider.GetRequiredService<IRepository>();

        var value = new BusinessEntity() { };

        await repository.AddAsync(value).ConfigureAwait(true);

        var result = await repository.GetSingleAsync(new BusinessEntitySingleQuery(value.BusinessEntityId)).ConfigureAwait(true);

        result.Should().NotBeNull();
    }

    [Test]
    public async Task AddSingleKeyIdentityGuidAsync()
    {
        var repository = ServiceProvider.GetRequiredService<IRepository>();

        var value = new Address1() { Address1Id = Guid.NewGuid() };

        await repository.AddAsync(value).ConfigureAwait(true);

        var result = await repository.GetSingleAsync(new Address1SingleQuery(value.Address1Id)).ConfigureAwait(true);

        result.Should().NotBeNull();
    }

    [Test]
    public async Task AddSingleKeyAsync()
    {
        var repository = ServiceProvider.GetRequiredService<IRepository>();

        var person = new Person() { BusinessEntityId = 1, FirstName = "Test" };

        await repository.AddAsync(person).ConfigureAwait(true);

        var result = await repository.GetSingleAsync(new PersonSingleQuery(person.BusinessEntityId)).ConfigureAwait(true);

        result.Should().NotBeNull();
    }

    [Test]
    public async Task UpdateSingleKeyAsync()
    {
        var repository = ServiceProvider.GetRequiredService<IRepository>();

        var person = new Person() { BusinessEntityId = 1, FirstName = "Test" };

        await repository.AddAsync(person).ConfigureAwait(true);

        person.FirstName = "Test2";

        await repository.UpdateAsync(person).ConfigureAwait(true);

        var result = await repository.GetSingleAsync(new PersonSingleQuery(person.BusinessEntityId)).ConfigureAwait(true);

        result.Should().NotBeNull();

        result!.FirstName.Should().Be(person.FirstName);
    }

    [Test]
    public async Task DeleteSingleKeyAsync()
    {
        var repository = ServiceProvider.GetRequiredService<IRepository>();

        var person = new Person() { BusinessEntityId = 1, FirstName = "Test" };

        await repository.AddAsync(person).ConfigureAwait(true);

        await repository.DeleteAsync(person).ConfigureAwait(true);

        var result = await repository.GetSingleAsync(new PersonSingleQuery(person.BusinessEntityId)).ConfigureAwait(true);

        result.Should().BeNull();
    }

    [Test]
    public async Task AddComplexKeyAsync()
    {
        var repository = ServiceProvider.GetRequiredService<IRepository>();

        var address = new BusinessEntityAddress() { BusinessEntityId = 1, AddressId = 1, AddressTypeId = 1, ModifiedDate = DateTime.UtcNow };

        await repository.AddAsync(address).ConfigureAwait(true);

        var result = await repository.GetSingleAsync(new BusinessEntityAddressSingleQuery(address.BusinessEntityId, address.AddressId, address.AddressTypeId)).ConfigureAwait(true);

        result.Should().NotBeNull();
    }

    [Test]
    public async Task UpdateComplexKeyAsync()
    {
        var repository = ServiceProvider.GetRequiredService<IRepository>();

        var address = new BusinessEntityAddress() { BusinessEntityId = 1, AddressId = 1, AddressTypeId = 1, ModifiedDate = DateTime.UtcNow };

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
        var repository = ServiceProvider.GetRequiredService<IRepository>();

        var address = new BusinessEntityAddress() { BusinessEntityId = 1, AddressId = 1, AddressTypeId = 1, ModifiedDate = DateTime.UtcNow };

        await repository.AddAsync(address).ConfigureAwait(true);

        await repository.DeleteAsync(address).ConfigureAwait(true);

        var result = await repository.GetSingleAsync(new BusinessEntityAddressSingleQuery(address.BusinessEntityId, address.AddressId, address.AddressTypeId)).ConfigureAwait(true);

        result.Should().BeNull();
    }

    [Test]
    public async Task AddRangeAsync()
    {
        var repository = ServiceProvider.GetRequiredService<IRepository>();

        var list = new List<Person>();

        var count = 10;

        foreach (var i in Enumerable.Range(1, count))
        {
            var person = new Person() { BusinessEntityId = i, FirstName = "Test" };
            list.Add(person);
        }

        await repository.AddRangeAsync(list).ConfigureAwait(true);

        var result = await repository.GetCountAsync<Person>(i => i.FirstName == "Test", i => i.OrderBy(i2 => i2.BusinessEntityId)).ConfigureAwait(true);

        result.Should().Be(count);
    }

    [Test]
    public async Task UpdateRangeAsync()
    {
        var list = new List<Person>();
        var count = 10;

        var repository = ServiceProvider.GetRequiredService<IRepository>();

        foreach (var i in Enumerable.Range(1, count))
        {
            var person = new Person() { BusinessEntityId = i, FirstName = "Test" };
            list.Add(person);
        }

        await repository.AddRangeAsync(list).ConfigureAwait(true);

        var result = await repository.GetCountAsync<Person>(i => i.FirstName == "Test").ConfigureAwait(true);

        result.Should().Be(count);

        foreach (var item in list)
        {
            item.FirstName = "Test1";
        }

        await repository.UpdateRangeAsync(list).ConfigureAwait(true);

        var result1 = await repository.GetCountAsync<Person>(i => i.FirstName == "Test1").ConfigureAwait(true);

        result1.Should().Be(count);

    }

    [Test]
    public async Task DeleteRangeAsync()
    {
        var repository = ServiceProvider.GetRequiredService<IRepository>();

        var list = new List<Person>();

        var count = 10;

        foreach (var i in Enumerable.Range(1, count))
        {
            var person = new Person() { BusinessEntityId = i, FirstName = $"Test" };
            list.Add(person);
        }

        await repository.AddRangeAsync(list).ConfigureAwait(true);

        var result = await repository.GetCountAsync<Person>(i => i.FirstName == "Test").ConfigureAwait(true);

        result.Should().Be(count);

        await repository.DeleteRangeAsync(list).ConfigureAwait(true);

        result = await repository.GetCountAsync<Person>(i => i.FirstName == "Test").ConfigureAwait(true);

        result.Should().Be(0);
    }

    [Test]
    public async Task GetListAsync()
    {
        var repository = ServiceProvider.GetRequiredService<IRepository>();
        var count = 10;
        foreach (var i in Enumerable.Range(1, count))
        {
            var person = new Person() { BusinessEntityId = i, FirstName = $"Test" };
            await repository.AddAsync(person).ConfigureAwait(true);
        }

        var result = await repository.GetListAsync<Person>(i => i.FirstName == "Test").ConfigureAwait(true);

        result.Should().HaveCount(count);
    }

    [Test]
    public async Task GetCountAsync()
    {
        var repository = ServiceProvider.GetRequiredService<IRepository>();

        var count = 10;

        foreach (var i in Enumerable.Range(1, count))
        {
            var person = new Person() { BusinessEntityId = i, FirstName = $"Test" };
            await repository.AddAsync(person).ConfigureAwait(true);
        }

        var result = await repository.GetCountAsync<Person>(i => i.FirstName == "Test").ConfigureAwait(true);

        result.Should().Be(count);
    }

    [Test]
    public async Task GroupByAsync()
    {
        var repository = ServiceProvider.GetRequiredService<IRepository>();

        var count = 10;

        foreach (var i in Enumerable.Range(1, count))
        {
            var customer = new Customer() { TerritoryId = 1 };
            await repository.AddAsync(customer).ConfigureAwait(true);
        }

        var result = await repository.GetListAsync(new CustomerByTerritoryQuery()).ConfigureAwait(true);
        result.Should().HaveCount(1);
        result.First().Count.Should().Be(count);
    }

    [Test]
    public async Task ExistsAsync()
    {
        var storId = 1;
        var terrirotyName = "TEST";

        var repository = ServiceProvider.GetRequiredService<IRepository>();

        var count = 10;

        foreach (var i in Enumerable.Range(1, count))
        {
            var customer = new Customer() { TerritoryId = 1, StoreId = storId };
            await repository.AddAsync(customer).ConfigureAwait(true);
        }

        var spec = new TestExistsQuery(storId, terrirotyName);

        var result = await repository.GetCountAsync(spec).ConfigureAwait(false);
        result.Should().Be(0);
        await repository.AddAsync(new SalesTerritory() { TerritoryId = 1, Name = terrirotyName }).ConfigureAwait(true);
        result = await repository.GetCountAsync(spec).ConfigureAwait(false);
        result.Should().Be(count);

    }

}
