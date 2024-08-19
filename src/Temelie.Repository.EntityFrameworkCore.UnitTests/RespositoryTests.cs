using AdventureWorks.Entities;
using AdventureWorks.Server.Repository;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace Temelie.Repository.EntityFrameworkCore.UnitTests;

public class RespositoryTests : TestBase
{

    [Test]
    public async Task AddSingleKeyIdentityIntAsync()
    {
        using var repository = ServiceProvider.GetRequiredService<IRepository>();

        var value = new BusinessEntity() { };

        await repository.AddAsync(value).ConfigureAwait(true);

        var result = await repository.GetSingleAsync(new BusinessEntitySingleQuery(value.BusinessEntityId)).ConfigureAwait(true);

        result.Should().NotBeNull();
    }

    [Test]
    public async Task AddSingleKeyIdentityGuidAsync()
    {
        using var repository = ServiceProvider.GetRequiredService<IRepository>();

        var value = new Address1() { Address1Id = Guid.NewGuid() };

        await repository.AddAsync(value).ConfigureAwait(true);

        var result = await repository.GetSingleAsync(new Address1SingleQuery(value.Address1Id)).ConfigureAwait(true);

        result.Should().NotBeNull();
    }

    [Test]
    public async Task AddSingleKeyAsync()
    {
        using var repository = ServiceProvider.GetRequiredService<IRepository>();

        var person = new Person() { BusinessEntityId = 1, FirstName = "Test" };

        await repository.AddAsync(person).ConfigureAwait(true);

        var result = await repository.GetSingleAsync(new PersonSingleQuery(person.BusinessEntityId)).ConfigureAwait(true);

        result.Should().NotBeNull();
    }

    [Test]
    public async Task UpdateSingleKeyAsync()
    {
        using var repository = ServiceProvider.GetRequiredService<IRepository>();

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
        using var repository = ServiceProvider.GetRequiredService<IRepository>();

        var person = new Person() { BusinessEntityId = 1, FirstName = "Test" };

        await repository.AddAsync(person).ConfigureAwait(true);

        await repository.DeleteAsync(person).ConfigureAwait(true);

        var result = await repository.GetSingleAsync(new PersonSingleQuery(person.BusinessEntityId)).ConfigureAwait(true);

        result.Should().BeNull();
    }

    [Test]
    public async Task AddComplexKeyAsync()
    {
        using var repository = ServiceProvider.GetRequiredService<IRepository>();

        var address = new BusinessEntityAddress() { BusinessEntityId = 1, AddressId = 1, AddressTypeId = 1, ModifiedDate = DateTime.UtcNow };

        await repository.AddAsync(address).ConfigureAwait(true);

        var result = await repository.GetSingleAsync(new BusinessEntityAddressSingleQuery(address.BusinessEntityId, address.AddressId, address.AddressTypeId)).ConfigureAwait(true);

        result.Should().NotBeNull();
    }

    [Test]
    public async Task UpdateComplexKeyAsync()
    {
        using var repository = ServiceProvider.GetRequiredService<IRepository>();

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
        using var repository = ServiceProvider.GetRequiredService<IRepository>();

        var address = new BusinessEntityAddress() { BusinessEntityId = 1, AddressId = 1, AddressTypeId = 1, ModifiedDate = DateTime.UtcNow };

        await repository.AddAsync(address).ConfigureAwait(true);

        await repository.DeleteAsync(address).ConfigureAwait(true);

        var result = await repository.GetSingleAsync(new BusinessEntityAddressSingleQuery(address.BusinessEntityId, address.AddressId, address.AddressTypeId)).ConfigureAwait(true);

        result.Should().BeNull();
    }

    [Test]
    public async Task AddRangeAsync()
    {
        using var repository = ServiceProvider.GetRequiredService<IRepository>();

        var list = new List<Person>();

        var count = 10;

        foreach (var i in Enumerable.Range(1, count))
        {
            var person = new Person() { BusinessEntityId = i, FirstName = "Test" };
            list.Add(person);
        }

        await repository.AddRangeAsync(list).ConfigureAwait(true);

        var result = await repository.GetCountAsync<Person>(q => q.Where(i => i.FirstName == "Test").OrderBy(i => i.BusinessEntityId)).ConfigureAwait(true);

        result.Should().Be(count);
    }

    [Test]
    public async Task UpdateRangeAsync()
    {
        var list = new List<Person>();
        var count = 10;

        using (var repository = ServiceProvider.GetRequiredService<IRepository>())
        {
            foreach (var i in Enumerable.Range(1, count))
            {
                var person = new Person() { BusinessEntityId = i, FirstName = "Test" };
                list.Add(person);
            }

            await repository.AddRangeAsync(list).ConfigureAwait(true);

            var result = await repository.GetCountAsync<Person>(q => q.Where(i => i.FirstName == "Test").OrderBy(i => i.BusinessEntityId)).ConfigureAwait(true);

            result.Should().Be(count);
        }

        using (var repository = ServiceProvider.GetRequiredService<IRepository>())
        {
            foreach (var item in list)
            {
                item.FirstName = "Test1";
            }

            await repository.UpdateRangeAsync(list).ConfigureAwait(true);

            var result = await repository.GetCountAsync<Person>(q => q.Where(i => i.FirstName == "Test1").OrderBy(i => i.BusinessEntityId)).ConfigureAwait(true);

            result.Should().Be(count);
        }
    }

    [Test]
    public async Task DeleteRangeAsync()
    {
        using var repository = ServiceProvider.GetRequiredService<IRepository>();

        var list = new List<Person>();

        var count = 10;

        foreach (var i in Enumerable.Range(1, count))
        {
            var person = new Person() { BusinessEntityId = i, FirstName = $"Test" };
            list.Add(person);
        }

        await repository.AddRangeAsync(list).ConfigureAwait(true);

        var result = await repository.GetCountAsync<Person>(q => q.Where(i => i.FirstName == "Test").OrderBy(i => i.BusinessEntityId)).ConfigureAwait(true);

        result.Should().Be(count);

        await repository.DeleteRangeAsync(list).ConfigureAwait(true);

        result = await repository.GetCountAsync<Person>(q => q.Where(i => i.FirstName == "Test").OrderBy(i => i.BusinessEntityId)).ConfigureAwait(true);

        result.Should().Be(0);
    }

    [Test]
    public async Task GetListAsync()
    {
        using var repository = ServiceProvider.GetRequiredService<IRepository>();
        var count = 10;
        foreach (var i in Enumerable.Range(1, count))
        {
            var person = new Person() { BusinessEntityId = i, FirstName = $"Test" };
            await repository.AddAsync(person).ConfigureAwait(true);
        }

        var result = await repository.GetListAsync<Person>(q => q.Where(i => i.FirstName == "Test").OrderBy(i => i.BusinessEntityId)).ConfigureAwait(true);

        result.Should().HaveCount(count);
    }

    [Test]
    public async Task GetCountAsync()
    {
        using var repository = ServiceProvider.GetRequiredService<IRepository>();

        var count = 10;

        foreach (var i in Enumerable.Range(1, count))
        {
            var person = new Person() {  BusinessEntityId = i, FirstName = $"Test" };
            await repository.AddAsync(person).ConfigureAwait(true);
        }

        var result = await repository.GetCountAsync<Person>(q => q.Where(i => i.FirstName == "Test").OrderBy(i => i.BusinessEntityId)).ConfigureAwait(true);

        result.Should().Be(count);
    }

}
