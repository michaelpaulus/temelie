using Temelie.Database.Providers;
using Temelie.Database.Services;

namespace Temelie.Database.UnitTests;

/// <summary>Characterizes how <c>DatabaseExecutionService.ExecuteFile</c> splits scripts into batches.</summary>
public class ExecuteFileTests
{
    private static List<string> Batches(string script)
    {
        var batches = new List<string>();
        var connection = new FakeDbConnection(sql =>
        {
            batches.Add(sql);
            return 0;
        });
        new DatabaseExecutionService(new DatabaseFactory([], [])).ExecuteFile(connection, script);
        return batches;
    }

    [Test]
    public void SplitsOnGoLines()
    {
        Batches("DROP VIEW IF EXISTS dbo.A;\nGO\nCREATE VIEW dbo.A AS SELECT 1 AS Id\nGO\n")
            .Select(i => i.Trim())
            .Should().Equal("DROP VIEW IF EXISTS dbo.A;", "CREATE VIEW dbo.A AS SELECT 1 AS Id");
    }

    [Test]
    public void GoIsCaseInsensitiveAndMayBeIndentedOrFollowedByAComment()
    {
        Batches("SELECT 1\n  go\nSELECT 2\nGo -- next\nSELECT 3\n")
            .Select(i => i.Trim())
            .Should().Equal("SELECT 1", "SELECT 2", "-- next\nSELECT 3");
    }

    [Test]
    public void EmptyBatchesAreSkipped()
    {
        Batches("GO\n\nGO\nSELECT 1\nGO\n\n").Select(i => i.Trim()).Should().Equal("SELECT 1");
    }

    [Test]
    public void WordsThatStartWithGoAreNotSeparators()
    {
        Batches("SELECT 1\nGOTO Done\nGOAL\n").Should().ContainSingle();
    }

    [Test]
    public void GoOnTheLastLineWithoutANewLineIsNotASeparator()
    {
        // Current behavior: the separator needs a following character, so a trailing "GO"
        // with no line break stays in the last batch.
        Batches("SELECT 1\nGO\nSELECT 2\nGO").Select(i => i.Trim()).Should().Equal("SELECT 1", "SELECT 2\nGO");
    }

    [Test]
    public void EmptyScriptsRunNothing()
    {
        Batches("").Should().BeEmpty();
    }
}
