using AdventureWorks.Entities;
using AdventureWorks.Server;
using AdventureWorks.Server.Repository;
using AwesomeAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace Temelie.Repository.UnitTests;

public class RespositoryTests : TestBase
{

    [Test]
    public async Task AddSingleKeyIdentityIntAsync()
    {
        var repository = ServiceProvider.GetRequiredService<IExampleRepository>();

        var value = new BusinessEntity() { };

        await repository.AddAsync(value).ConfigureAwait(true);

        var result = await repository.GetSingleAsync(new BusinessEntitySingleQuery(value.BusinessEntityId)).ConfigureAwait(true);

        result.Should().NotBeNull();
    }

    [Test]
    public async Task AddSingleKeyIdentityGuidAsync()
    {
        var repository = ServiceProvider.GetRequiredService<IExampleRepository>();

        var value = new Address1() { Address1Id = Guid.NewGuid() };

        await repository.AddAsync(value).ConfigureAwait(true);

        var result = await repository.GetSingleAsync(new Address1SingleQuery(value.Address1Id)).ConfigureAwait(true);

        result.Should().NotBeNull();
    }

    [Test]
    public async Task AddSingleKeyAsync()
    {
        var repository = ServiceProvider.GetRequiredService<IExampleRepository>();

        var person = new Person() { BusinessEntityId = 1, FirstName = "Test" };

        await repository.AddAsync(person).ConfigureAwait(true);

        var result = await repository.GetSingleAsync(new PersonSingleQuery(person.BusinessEntityId)).ConfigureAwait(true);

        result.Should().NotBeNull();
    }

    [Test]
    public async Task UpdateSingleKeyAsync()
    {
        var repository = ServiceProvider.GetRequiredService<IExampleRepository>();

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
        var repository = ServiceProvider.GetRequiredService<IExampleRepository>();

        var person = new Person() { BusinessEntityId = 1, FirstName = "Test" };

        await repository.AddAsync(person).ConfigureAwait(true);

        await repository.DeleteAsync(person).ConfigureAwait(true);

        var result = await repository.GetSingleAsync(new PersonSingleQuery(person.BusinessEntityId)).ConfigureAwait(true);

        result.Should().BeNull();
    }

    [Test]
    public async Task AddComplexKeyAsync()
    {
        var repository = ServiceProvider.GetRequiredService<IExampleRepository>();

        var address = new BusinessEntityAddress() { BusinessEntityId = 1, AddressId = 1, AddressTypeId = 1, ModifiedDate = DateTime.UtcNow };

        await repository.AddAsync(address).ConfigureAwait(true);

        var result = await repository.GetSingleAsync(new BusinessEntityAddressSingleQuery(address.BusinessEntityId, address.AddressId, address.AddressTypeId)).ConfigureAwait(true);

        result.Should().NotBeNull();
    }

    [Test]
    public async Task UpdateComplexKeyAsync()
    {
        var repository = ServiceProvider.GetRequiredService<IExampleRepository>();

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
        var repository = ServiceProvider.GetRequiredService<IExampleRepository>();

        var address = new BusinessEntityAddress() { BusinessEntityId = 1, AddressId = 1, AddressTypeId = 1, ModifiedDate = DateTime.UtcNow };

        await repository.AddAsync(address).ConfigureAwait(true);

        await repository.DeleteAsync(address).ConfigureAwait(true);

        var result = await repository.GetSingleAsync(new BusinessEntityAddressSingleQuery(address.BusinessEntityId, address.AddressId, address.AddressTypeId)).ConfigureAwait(true);

        result.Should().BeNull();
    }

    [Test]
    public async Task AddRangeAsync()
    {
        var repository = ServiceProvider.GetRequiredService<IExampleRepository>();

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

        var repository = ServiceProvider.GetRequiredService<IExampleRepository>();

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
        var repository = ServiceProvider.GetRequiredService<IExampleRepository>();

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
        var repository = ServiceProvider.GetRequiredService<IExampleRepository>();
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
        var repository = ServiceProvider.GetRequiredService<IExampleRepository>();

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
        var repository = ServiceProvider.GetRequiredService<IExampleRepository>();

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

        var repository = ServiceProvider.GetRequiredService<IExampleRepository>();

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

    [Test]
    public async Task GetAnyAsync()
    {
        var repository = ServiceProvider.GetRequiredService<IExampleRepository>();

        // Test when no records exist
        var anyResult = await repository.GetAnyAsync<Person>(i => i.FirstName == "NonExistent").ConfigureAwait(true);
        anyResult.Should().BeFalse();

        // Add test data
        var count = 5;
        foreach (var i in Enumerable.Range(1, count))
        {
            var person = new Person() { BusinessEntityId = i, FirstName = $"Test{i}" };
            await repository.AddAsync(person).ConfigureAwait(true);
        }

        // Test with filter - should find records
        anyResult = await repository.GetAnyAsync<Person>(i => i.FirstName == "Test1").ConfigureAwait(true);
        anyResult.Should().BeTrue();

        // Test with filter - should not find records
        anyResult = await repository.GetAnyAsync<Person>(i => i.FirstName == "NotFound").ConfigureAwait(true);
        anyResult.Should().BeFalse();

        // Test with filter and query
        anyResult = await repository.GetAnyAsync<Person>(i => i.FirstName.StartsWith("Test"), i => i.OrderBy(i2 => i2.BusinessEntityId)).ConfigureAwait(true);
        anyResult.Should().BeTrue();

        // Test with null filter (should find any records)
        anyResult = await repository.GetAnyAsync<Person>(null, null).ConfigureAwait(true);
        anyResult.Should().BeTrue();
    }

    [Test]
    public async Task InsertFromQueryWithFilterAsync()
    {
        var repository = ServiceProvider.GetRequiredService<IExampleRepository>();

        var count = 5;

        foreach (var i in Enumerable.Range(1, count))
        {
            await repository.AddAsync(new Person() { BusinessEntityId = i, FirstName = "Match" }).ConfigureAwait(true);
        }

        await repository.AddAsync(new Person() { BusinessEntityId = 100, FirstName = "NoMatch" }).ConfigureAwait(true);

        var rowguid = Guid.NewGuid();
        var modifiedDate = DateTime.UtcNow;

        var inserted = await repository.InsertFromQueryAsync<Person, BusinessEntityAddress>(
            p => p.FirstName == "Match",
            p => new BusinessEntityAddress
            {
                BusinessEntityId = p.BusinessEntityId,
                AddressId = 1,
                AddressTypeId = 2,
                rowguid = rowguid,
                ModifiedDate = modifiedDate
            }).ConfigureAwait(true);

        inserted.Should().Be(count);

        var total = await repository.GetCountAsync<BusinessEntityAddress>().ConfigureAwait(true);
        total.Should().Be(count);

        var single = await repository.GetSingleAsync<BusinessEntityAddress>(i => i.BusinessEntityId == 1).ConfigureAwait(true);
        single.Should().NotBeNull();
        single!.AddressId.Should().Be(1);
        single.AddressTypeId.Should().Be(2);

        var excluded = await repository.GetSingleAsync<BusinessEntityAddress>(i => i.BusinessEntityId == 100).ConfigureAwait(true);
        excluded.Should().BeNull();
    }

    [Test]
    public async Task InsertFromQueryNullFilterAsync()
    {
        var repository = ServiceProvider.GetRequiredService<IExampleRepository>();

        var count = 5;

        foreach (var i in Enumerable.Range(1, count))
        {
            await repository.AddAsync(new Person() { BusinessEntityId = i, FirstName = "Test" }).ConfigureAwait(true);
        }

        var rowguid = Guid.NewGuid();
        var modifiedDate = DateTime.UtcNow;

        var inserted = await repository.InsertFromQueryAsync<Person, BusinessEntityAddress>(
            null,
            p => new BusinessEntityAddress
            {
                BusinessEntityId = p.BusinessEntityId,
                AddressId = 2,
                AddressTypeId = 3,
                rowguid = rowguid,
                ModifiedDate = modifiedDate
            }).ConfigureAwait(true);

        inserted.Should().Be(count);

        var total = await repository.GetCountAsync<BusinessEntityAddress>().ConfigureAwait(true);
        total.Should().Be(count);
    }

    [Test]
    public async Task InsertFromQueryDuplicateConstantColumnsAsync()
    {
        var repository = ServiceProvider.GetRequiredService<IExampleRepository>();

        var count = 5;

        foreach (var i in Enumerable.Range(1, count))
        {
            await repository.AddAsync(new Person() { BusinessEntityId = i, FirstName = "Test" }).ConfigureAwait(true);
        }

        var rowguid = Guid.NewGuid();
        var modifiedDate = DateTime.UtcNow;

        // AddressId and AddressTypeId project the same constant value; EF Core would otherwise
        // collapse the duplicate literal into a single column and break the INSERT column mapping.
        var inserted = await repository.InsertFromQueryAsync<Person, BusinessEntityAddress>(
            null,
            p => new BusinessEntityAddress
            {
                BusinessEntityId = p.BusinessEntityId,
                AddressId = 1,
                AddressTypeId = 1,
                rowguid = rowguid,
                ModifiedDate = modifiedDate
            }).ConfigureAwait(true);

        inserted.Should().Be(count);

        var single = await repository.GetSingleAsync<BusinessEntityAddress>(i => i.BusinessEntityId == 1).ConfigureAwait(true);
        single.Should().NotBeNull();
        single!.AddressId.Should().Be(1);
        single.AddressTypeId.Should().Be(1);
    }

}
