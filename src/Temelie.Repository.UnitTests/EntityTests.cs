using AdventureWorks.Entities;
using AwesomeAssertions;
using Temelie.Entities;

namespace Temelie.Repository.UnitTests;

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
