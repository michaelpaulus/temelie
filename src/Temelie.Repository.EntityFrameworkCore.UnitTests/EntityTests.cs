using AdventureWorks.Entities;
using FluentAssertions;
using Temelie.Entities;

namespace Temelie.Repository.EntityFrameworkCore.UnitTests;

public class EntityTests : TestBase
{

    [Test]
    public void ModifiedBy()
    {
        var address = new Address();
        (address is IModifiedByEntity).Should().BeTrue();
    }

    [Test]
    public void CreatedBy()
    {
        var address = new Address();
        (address is ICreatedByEntity).Should().BeTrue();
    }


}
