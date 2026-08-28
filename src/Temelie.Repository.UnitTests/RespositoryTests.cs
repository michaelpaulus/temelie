using AdventureWorks.Entities;
using AdventureWorks.Server;
using AdventureWorks.Server.Repository;
using AwesomeAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Linq.Expressions;

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
            (Expression<Func<Person, bool>>?)null,
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
            (Expression<Func<Person, bool>>?)null,
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

    [Test]
    public async Task DeleteFromQueryNotExistsInOtherTableAsync()
    {
        var repository = ServiceProvider.GetRequiredService<IExampleRepository>();

        // Persons 1..3 exist; addresses reference 1..5, so 4 and 5 are orphaned.
        foreach (var i in Enumerable.Range(1, 3))
        {
            await repository.AddAsync(new Person() { BusinessEntityId = i, FirstName = "Test" }).ConfigureAwait(true);
        }

        foreach (var i in Enumerable.Range(1, 5))
        {
            await repository.AddAsync(new BusinessEntityAddress()
            {
                BusinessEntityId = i,
                AddressId = 1,
                AddressTypeId = 1,
                ModifiedDate = DateTime.UtcNow
            }).ConfigureAwait(true);
        }

        var deleted = await repository.DeleteFromQueryAsync(new OrphanedBusinessEntityAddressQuery()).ConfigureAwait(true);

        deleted.Should().Be(2);

        var remaining = await repository.GetCountAsync<BusinessEntityAddress>().ConfigureAwait(true);
        remaining.Should().Be(3);

        var orphan = await repository.GetSingleAsync<BusinessEntityAddress>(i => i.BusinessEntityId == 4).ConfigureAwait(true);
        orphan.Should().BeNull();

        var kept = await repository.GetSingleAsync<BusinessEntityAddress>(i => i.BusinessEntityId == 1).ConfigureAwait(true);
        kept.Should().NotBeNull();
    }

    [Test]
    public async Task UpdateFromQueryExistsInOtherTableAsync()
    {
        var repository = ServiceProvider.GetRequiredService<IExampleRepository>();

        // Persons 1..3 exist; addresses reference 1..5, so 4 and 5 have no matching Person.
        foreach (var i in Enumerable.Range(1, 3))
        {
            await repository.AddAsync(new Person() { BusinessEntityId = i, FirstName = "Test" }).ConfigureAwait(true);
        }

        foreach (var i in Enumerable.Range(1, 5))
        {
            await repository.AddAsync(new BusinessEntityAddress()
            {
                BusinessEntityId = i,
                AddressId = 1,
                AddressTypeId = 1,
                rowguid = Guid.Empty,
                ModifiedDate = DateTime.UtcNow
            }).ConfigureAwait(true);
        }

        var updatedGuid = Guid.NewGuid();

        await repository.UpdateFromQueryAsync(
            new ExistingBusinessEntityAddressQuery(),
            b => b.SetProperty(a => a.rowguid, updatedGuid)).ConfigureAwait(true);

        var updated = await repository.GetSingleAsync<BusinessEntityAddress>(i => i.BusinessEntityId == 1).ConfigureAwait(true);
        updated.Should().NotBeNull();
        updated!.rowguid.Should().Be(updatedGuid);

        var untouched = await repository.GetSingleAsync<BusinessEntityAddress>(i => i.BusinessEntityId == 4).ConfigureAwait(true);
        untouched.Should().NotBeNull();
        untouched!.rowguid.Should().Be(Guid.Empty);
    }

    [Test]
    public async Task InsertFromQueryNotExistsInTargetAsync()
    {
        var repository = ServiceProvider.GetRequiredService<IExampleRepository>();

        // Persons 1..5 exist; addresses already exist for 1..2, so only 3..5 should be inserted.
        foreach (var i in Enumerable.Range(1, 5))
        {
            await repository.AddAsync(new Person() { BusinessEntityId = i, FirstName = "Test" }).ConfigureAwait(true);
        }

        foreach (var i in Enumerable.Range(1, 2))
        {
            await repository.AddAsync(new BusinessEntityAddress()
            {
                BusinessEntityId = i,
                AddressId = 1,
                AddressTypeId = 1,
                ModifiedDate = DateTime.UtcNow
            }).ConfigureAwait(true);
        }

        var rowguid = Guid.NewGuid();
        var modifiedDate = DateTime.UtcNow;

        var inserted = await repository.InsertFromQueryAsync<Person, BusinessEntityAddress>(
            new PersonWithoutAddressQuery(),
            p => new BusinessEntityAddress
            {
                BusinessEntityId = p.BusinessEntityId,
                AddressId = 1,
                AddressTypeId = 1,
                rowguid = rowguid,
                ModifiedDate = modifiedDate
            }).ConfigureAwait(true);

        inserted.Should().Be(3);

        var total = await repository.GetCountAsync<BusinessEntityAddress>().ConfigureAwait(true);
        total.Should().Be(5);
    }

    [Test]
    public async Task MergeFromQueryInsertUpdateDeleteAsync()
    {
        var repository = ServiceProvider.GetRequiredService<IExampleRepository>();

        // Persons 1..5 exist. Addresses exist for 1 and 2 (matched) plus 99 (orphan, no person).
        foreach (var i in Enumerable.Range(1, 5))
        {
            await repository.AddAsync(new Person() { BusinessEntityId = i, FirstName = "Test" }).ConfigureAwait(true);
        }

        foreach (var i in new[] { 1, 2, 99 })
        {
            await repository.AddAsync(new BusinessEntityAddress()
            {
                BusinessEntityId = i,
                AddressId = 1,
                AddressTypeId = 1,
                rowguid = Guid.Empty,
                ModifiedDate = DateTime.UtcNow
            }).ConfigureAwait(true);
        }

        var rowguid = Guid.NewGuid();
        var updatedGuid = Guid.NewGuid();
        var modifiedDate = DateTime.UtcNow;

        var result = await repository.MergeFromQueryAsync<Person, BusinessEntityAddress>(
            (p, a) => a.BusinessEntityId == p.BusinessEntityId,
            p => new BusinessEntityAddress
            {
                BusinessEntityId = p.BusinessEntityId,
                AddressId = 1,
                AddressTypeId = 1,
                rowguid = rowguid,
                ModifiedDate = modifiedDate
            },
            b => b.SetProperty(a => a.rowguid, updatedGuid),
            deleteMissing: true).ConfigureAwait(true);

        result.Inserted.Should().Be(3);
        result.Updated.Should().Be(2);
        result.Deleted.Should().Be(1);

        var total = await repository.GetCountAsync<BusinessEntityAddress>().ConfigureAwait(true);
        total.Should().Be(5);

        // Orphan removed.
        var orphan = await repository.GetSingleAsync<BusinessEntityAddress>(i => i.BusinessEntityId == 99).ConfigureAwait(true);
        orphan.Should().BeNull();

        // Matched row updated.
        var matched = await repository.GetSingleAsync<BusinessEntityAddress>(i => i.BusinessEntityId == 1).ConfigureAwait(true);
        matched.Should().NotBeNull();
        matched!.rowguid.Should().Be(updatedGuid);

        // New row inserted with the insert selector's value.
        var insertedRow = await repository.GetSingleAsync<BusinessEntityAddress>(i => i.BusinessEntityId == 5).ConfigureAwait(true);
        insertedRow.Should().NotBeNull();
        insertedRow!.rowguid.Should().Be(rowguid);
    }

    [Test]
    public async Task InsertFromQueryStampsModifiedDateAsync()
    {
        var repository = ServiceProvider.GetRequiredService<IExampleRepository>();

        await repository.AddAsync(new Person() { BusinessEntityId = 1, FirstName = "Test" }).ConfigureAwait(true);

        var rowguid = Guid.NewGuid();
        var staleDate = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var before = DateTime.UtcNow;

        // The selector supplies a stale ModifiedDate; the insert should overwrite it with the server time.
        await repository.InsertFromQueryAsync<Person, BusinessEntityAddress>(
            (Expression<Func<Person, bool>>?)null,
            p => new BusinessEntityAddress
            {
                BusinessEntityId = p.BusinessEntityId,
                AddressId = 1,
                AddressTypeId = 1,
                rowguid = rowguid,
                ModifiedDate = staleDate
            }).ConfigureAwait(true);

        var row = await repository.GetSingleAsync<BusinessEntityAddress>(i => i.BusinessEntityId == 1).ConfigureAwait(true);
        row.Should().NotBeNull();
        row!.ModifiedDate.Should().BeOnOrAfter(before);
    }

    [Test]
    public async Task UpdateFromQueryStampsModifiedDateAsync()
    {
        var repository = ServiceProvider.GetRequiredService<IExampleRepository>();

        await repository.AddAsync(new Person() { BusinessEntityId = 1, FirstName = "Test" }).ConfigureAwait(true);
        await repository.AddAsync(new BusinessEntityAddress()
        {
            BusinessEntityId = 1,
            AddressId = 1,
            AddressTypeId = 1,
            ModifiedDate = DateTime.UtcNow
        }).ConfigureAwait(true);

        // Force a stale ModifiedDate directly in the database (bypassing the repository's stamping).
        var staleDate = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        using (var db = ServiceProvider.GetRequiredService<IDbContextFactory<ExampleDbContext>>().CreateDbContext())
        {
            await db.Set<BusinessEntityAddress>()
                .ExecuteUpdateAsync(b => b.SetProperty(a => a.ModifiedDate, staleDate)).ConfigureAwait(true);
        }

        var before = DateTime.UtcNow;

        // The update only sets rowguid; ModifiedDate should still be stamped automatically.
        await repository.UpdateFromQueryAsync(
            new ExistingBusinessEntityAddressQuery(),
            b => b.SetProperty(a => a.rowguid, Guid.NewGuid())).ConfigureAwait(true);

        var row = await repository.GetSingleAsync<BusinessEntityAddress>(i => i.BusinessEntityId == 1).ConfigureAwait(true);
        row.Should().NotBeNull();
        row!.ModifiedDate.Should().BeOnOrAfter(before);
    }

    [Test]
    public async Task AddAsyncStampsAllAuditFieldsAsync()
    {
        var repository = ServiceProvider.GetRequiredService<IExampleRepository>();

        var before = DateTime.UtcNow;

        await repository.AddAsync(new AuditTest() { AuditTestId = 1 }).ConfigureAwait(true);

        var row = await repository.GetSingleAsync<AuditTest>(i => i.AuditTestId == 1).ConfigureAwait(true);
        row.Should().NotBeNull();
        row!.CreatedDate.Should().BeOnOrAfter(before);
        row.ModifiedDate.Should().BeOnOrAfter(before);
        row.CreatedBy.Should().Be(ExampleRepository.CreatedModifiedBy);
        row.ModifiedBy.Should().Be(ExampleRepository.CreatedModifiedBy);
    }

    [Test]
    public async Task AddRangeAsyncStampsAllAuditFieldsAsync()
    {
        var repository = ServiceProvider.GetRequiredService<IExampleRepository>();

        var before = DateTime.UtcNow;

        var list = Enumerable.Range(1, 3).Select(i => new AuditTest() { AuditTestId = i }).ToList();
        await repository.AddRangeAsync(list).ConfigureAwait(true);

        var rows = await repository.GetListAsync<AuditTest>().ConfigureAwait(true);
        rows.Should().HaveCount(3);
        foreach (var row in rows)
        {
            row.CreatedDate.Should().BeOnOrAfter(before);
            row.ModifiedDate.Should().BeOnOrAfter(before);
            row.CreatedBy.Should().Be(ExampleRepository.CreatedModifiedBy);
            row.ModifiedBy.Should().Be(ExampleRepository.CreatedModifiedBy);
        }
    }

    [Test]
    public async Task UpdateAsyncStampsModifiedAuditFieldsAsync()
    {
        var repository = ServiceProvider.GetRequiredService<IExampleRepository>();

        await repository.AddAsync(new AuditTest() { AuditTestId = 1 }).ConfigureAwait(true);
        var added = await repository.GetSingleAsync<AuditTest>(i => i.AuditTestId == 1).ConfigureAwait(true);
        var createdDate = added!.CreatedDate;

        var before = DateTime.UtcNow;
        // Blank the modified fields to prove the update repopulates them.
        added.ModifiedBy = "";
        await repository.UpdateAsync(added).ConfigureAwait(true);

        var row = await repository.GetSingleAsync<AuditTest>(i => i.AuditTestId == 1).ConfigureAwait(true);
        row.Should().NotBeNull();
        row!.ModifiedDate.Should().BeOnOrAfter(before);
        row.ModifiedBy.Should().Be(ExampleRepository.CreatedModifiedBy);
        // Created audit is preserved on update.
        row.CreatedDate.Should().Be(createdDate);
        row.CreatedBy.Should().Be(ExampleRepository.CreatedModifiedBy);
    }

    [Test]
    public async Task UpdateRangeAsyncStampsModifiedAuditFieldsAsync()
    {
        var repository = ServiceProvider.GetRequiredService<IExampleRepository>();

        var list = Enumerable.Range(1, 3).Select(i => new AuditTest() { AuditTestId = i }).ToList();
        await repository.AddRangeAsync(list).ConfigureAwait(true);

        var before = DateTime.UtcNow;
        await repository.UpdateRangeAsync(list).ConfigureAwait(true);

        var rows = await repository.GetListAsync<AuditTest>().ConfigureAwait(true);
        foreach (var row in rows)
        {
            row.ModifiedDate.Should().BeOnOrAfter(before);
            row.ModifiedBy.Should().Be(ExampleRepository.CreatedModifiedBy);
        }
    }

    [Test]
    public async Task InsertFromQueryStampsAllAuditFieldsAsync()
    {
        var repository = ServiceProvider.GetRequiredService<IExampleRepository>();

        await repository.AddAsync(new Person() { BusinessEntityId = 1, FirstName = "Test" }).ConfigureAwait(true);

        var before = DateTime.UtcNow;

        // Selector sets only the key; every audit column should be stamped automatically.
        await repository.InsertFromQueryAsync<Person, AuditTest>(
            (Expression<Func<Person, bool>>?)null,
            p => new AuditTest { AuditTestId = p.BusinessEntityId }).ConfigureAwait(true);

        var row = await repository.GetSingleAsync<AuditTest>(i => i.AuditTestId == 1).ConfigureAwait(true);
        row.Should().NotBeNull();
        row!.CreatedDate.Should().BeOnOrAfter(before);
        row.ModifiedDate.Should().BeOnOrAfter(before);
        row.CreatedBy.Should().Be(ExampleRepository.CreatedModifiedBy);
        row.ModifiedBy.Should().Be(ExampleRepository.CreatedModifiedBy);
    }

    [Test]
    public async Task UpdateFromQueryStampsModifiedAuditFieldsAsync()
    {
        var repository = ServiceProvider.GetRequiredService<IExampleRepository>();

        await repository.AddAsync(new AuditTest() { AuditTestId = 1 }).ConfigureAwait(true);

        // Force stale modified audit directly in the database (bypassing the repository's stamping).
        var staleDate = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        using (var db = ServiceProvider.GetRequiredService<IDbContextFactory<ExampleDbContext>>().CreateDbContext())
        {
            await db.Set<AuditTest>().ExecuteUpdateAsync(b =>
            {
                b.SetProperty(a => a.ModifiedDate, staleDate);
                b.SetProperty(a => a.ModifiedBy, "stale");
            }).ConfigureAwait(true);
        }

        var before = DateTime.UtcNow;

        // The caller changes an unrelated column; modified audit should still be stamped.
        await repository.UpdateFromQueryAsync(
            new AllRowsQuery<AuditTest>(),
            b => b.SetProperty(a => a.CreatedBy, a => a.CreatedBy)).ConfigureAwait(true);

        var row = await repository.GetSingleAsync<AuditTest>(i => i.AuditTestId == 1).ConfigureAwait(true);
        row.Should().NotBeNull();
        row!.ModifiedDate.Should().BeOnOrAfter(before);
        row.ModifiedBy.Should().Be(ExampleRepository.CreatedModifiedBy);
    }

    [Test]
    public async Task MergeFromQueryStampsAuditFieldsAsync()
    {
        var repository = ServiceProvider.GetRequiredService<IExampleRepository>();

        // Person 1 already has an audit row (matched -> update); Person 2 has none (insert).
        await repository.AddAsync(new Person() { BusinessEntityId = 1, FirstName = "Test" }).ConfigureAwait(true);
        await repository.AddAsync(new Person() { BusinessEntityId = 2, FirstName = "Test" }).ConfigureAwait(true);
        await repository.AddAsync(new AuditTest() { AuditTestId = 1 }).ConfigureAwait(true);

        // Force stale modified audit on the matched row.
        var staleDate = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        using (var db = ServiceProvider.GetRequiredService<IDbContextFactory<ExampleDbContext>>().CreateDbContext())
        {
            await db.Set<AuditTest>().ExecuteUpdateAsync(b =>
            {
                b.SetProperty(a => a.ModifiedDate, staleDate);
                b.SetProperty(a => a.ModifiedBy, "stale");
            }).ConfigureAwait(true);
        }

        var before = DateTime.UtcNow;

        var result = await repository.MergeFromQueryAsync<Person, AuditTest>(
            (p, a) => a.AuditTestId == p.BusinessEntityId,
            p => new AuditTest { AuditTestId = p.BusinessEntityId },
            b => b.SetProperty(a => a.CreatedBy, a => a.CreatedBy)).ConfigureAwait(true);

        result.Inserted.Should().Be(1);
        result.Updated.Should().Be(1);

        // Inserted row gets all four audit fields.
        var inserted = await repository.GetSingleAsync<AuditTest>(i => i.AuditTestId == 2).ConfigureAwait(true);
        inserted.Should().NotBeNull();
        inserted!.CreatedDate.Should().BeOnOrAfter(before);
        inserted.ModifiedDate.Should().BeOnOrAfter(before);
        inserted.CreatedBy.Should().Be(ExampleRepository.CreatedModifiedBy);
        inserted.ModifiedBy.Should().Be(ExampleRepository.CreatedModifiedBy);

        // Matched row gets modified audit stamped.
        var updated = await repository.GetSingleAsync<AuditTest>(i => i.AuditTestId == 1).ConfigureAwait(true);
        updated.Should().NotBeNull();
        updated!.ModifiedDate.Should().BeOnOrAfter(before);
        updated.ModifiedBy.Should().Be(ExampleRepository.CreatedModifiedBy);
    }

    [Test]
    public async Task MergeFromQueryWithKeySelectorAsync()
    {
        var repository = ServiceProvider.GetRequiredService<IExampleRepository>();

        await repository.AddAsync(new Person() { BusinessEntityId = 1, FirstName = "Test" }).ConfigureAwait(true);
        await repository.AddAsync(new Person() { BusinessEntityId = 2, FirstName = "Test" }).ConfigureAwait(true);
        await repository.AddAsync(new AuditTest() { AuditTestId = 1 }).ConfigureAwait(true);

        var result = await repository.MergeFromQueryAsync<Person, AuditTest, int>(
            p => p.BusinessEntityId,
            a => a.AuditTestId,
            p => new AuditTest { AuditTestId = p.BusinessEntityId },
            b => b.SetProperty(a => a.CreatedBy, a => a.CreatedBy),
            deleteMissing: true).ConfigureAwait(true);

        result.Inserted.Should().Be(1);
        result.Updated.Should().Be(1);
        result.Deleted.Should().Be(0);

        var total = await repository.GetCountAsync<AuditTest>().ConfigureAwait(true);
        total.Should().Be(2);
    }

    [Test]
    public async Task MergeFromQueryWithCompositeKeySelectorAsync()
    {
        var repository = ServiceProvider.GetRequiredService<IExampleRepository>();

        await repository.AddAsync(new Person() { BusinessEntityId = 1, FirstName = "Test" }).ConfigureAwait(true);
        await repository.AddAsync(new Person() { BusinessEntityId = 2, FirstName = "Test" }).ConfigureAwait(true);
        await repository.AddAsync(new AuditTest() { AuditTestId = 1 }).ConfigureAwait(true);

        // Composite key selectors compare each member.
        var result = await repository.MergeFromQueryAsync(
            (Person p) => new { A = p.BusinessEntityId, B = p.BusinessEntityId },
            (AuditTest a) => new { A = a.AuditTestId, B = a.AuditTestId },
            p => new AuditTest { AuditTestId = p.BusinessEntityId }).ConfigureAwait(true);

        result.Inserted.Should().Be(1);
        result.Updated.Should().Be(0);

        var total = await repository.GetCountAsync<AuditTest>().ConfigureAwait(true);
        total.Should().Be(2);
    }

}
