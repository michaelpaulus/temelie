using Temelie.Database.Models;
using MySqlProvider = Temelie.Database.Providers.MySql.DatabaseProvider;

namespace Temelie.Database.UnitTests;

/// <summary>Characterizes <c>IScriptService.CreateScripts</c> (scripting a live database to a folder).</summary>
public class CreateScriptsTests
{
    private static readonly string[] ObjectFolders =
        ["02_Tables", "03_Indexes", "04_CheckConstraints", "05_Views", "05_Programmability", "06_Triggers", "08_ForeignKeys", "09_SecurityPolicies"];

    private static string Normalize(string script) => script.Replace("\r\n", "\n").Trim('\n');

    [Test]
    public void CreatesEveryObjectFolderAndMatchingDropFolders()
    {
        using var context = ScriptTestContext.SqlServer();

        context.Generate(_ => { });

        foreach (var folder in ObjectFolders)
        {
            context.Folder.Exists(folder).Should().BeTrue(folder);
            context.Folder.Exists($"01_Drop/{folder}").Should().BeTrue($"01_Drop/{folder}");
        }
    }

    [Test]
    public void WritesASqlScriptAndJsonModelForEveryObject()
    {
        using var context = ScriptTestContext.SqlServer();

        context.Generate(Samples.Customers);

        context.Folder.Files().Should().Equal(
            "02_Tables/dbo.Customer.sql",
            "02_Tables/dbo.Customer.sql.json",
            "02_Tables/dbo.Order.sql",
            "02_Tables/dbo.Order.sql.json",
            "03_Indexes/dbo.IX_Customer_Name.sql",
            "03_Indexes/dbo.IX_Customer_Name.sql.json",
            "03_Indexes/dbo.PK_Customer.sql",
            "03_Indexes/dbo.PK_Customer.sql.json",
            "03_Indexes/dbo.PK_Order.sql",
            "03_Indexes/dbo.PK_Order.sql.json",
            "04_CheckConstraints/dbo.CK_Customer_Id.sql",
            "04_CheckConstraints/dbo.CK_Customer_Id.sql.json",
            "05_Programmability/dbo.GetCustomers.sql",
            "05_Programmability/dbo.GetCustomers.sql.json",
            "05_Views/dbo.CustomerView.sql",
            "05_Views/dbo.CustomerView.sql.json",
            "06_Triggers/dbo.trCustomer.sql",
            "06_Triggers/dbo.trCustomer.sql.json",
            "08_ForeignKeys/dbo.FK_Order_Customer.sql",
            "08_ForeignKeys/dbo.FK_Order_Customer.sql.json");
    }

    [Test]
    public void ScriptsMatchTheProviderAndModelsRoundTrip()
    {
        using var context = ScriptTestContext.SqlServer();
        var expected = new DatabaseState();
        Samples.Customers(expected);

        context.Generate(Samples.Customers);

        Normalize(context.Folder.Read("02_Tables/dbo.Customer.sql")).Should().Be(Normalize(context.Provider.GetScript(expected.Tables[0])!.CreateScript));
        Normalize(context.Folder.Read("05_Views/dbo.CustomerView.sql")).Should().Be(Normalize(context.Provider.GetScript(expected.Definitions[1])!.CreateScript));

        var model = DatabaseModel.CreateFromFiles(Directory
            .GetFiles(context.Folder.Directory.FullName, "*.sql.json", SearchOption.AllDirectories)
            .Select(i => (i, File.ReadAllText(i))));
        model.Tables.Select(i => i.TableName).Should().BeEquivalentTo("Customer", "Order");
        model.Tables.Single(i => i.TableName == "Customer").Columns.Select(i => i.ColumnName).Should().Equal("Id", "Name");
        model.PrimaryKeys.Select(i => i.IndexName).Should().BeEquivalentTo("PK_Customer", "PK_Order");
        model.Indexes.Select(i => i.IndexName).Should().Equal("IX_Customer_Name");
        model.CheckConstraints.Select(i => i.CheckConstraintName).Should().Equal("CK_Customer_Id");
        model.Definitions.Select(i => i.DefinitionName).Should().BeEquivalentTo("GetCustomers", "CustomerView");
        model.Views.Select(i => i.TableName).Should().Equal("CustomerView");
        model.Triggers.Select(i => i.TriggerName).Should().Equal("trCustomer");
        model.ForeignKeys.Select(i => i.ForeignKeyName).Should().Equal("FK_Order_Customer");
    }

    [Test]
    public void MySqlFileNamesHaveNoSchemaAndPrimaryKeysStayInline()
    {
        using var context = ScriptTestContext.MySql();

        context.Generate(db =>
        {
            db.Tables.Add(Db.Table("", "customers", ("id", "INT"), ("name", "VARCHAR")));
            db.Indexes.Add(Db.PrimaryKey("", "customers", "PK_customers", "id"));
            db.Indexes.Add(Db.Index("", "customers", "ix_customers_name", "name"));
            db.Definitions.Add(Db.View("", "customer_view", "CREATE VIEW `customer_view` AS SELECT `id` FROM `customers`;"));
        });

        context.Folder.Files().Should().Equal(
            "02_Tables/customers.sql",
            "02_Tables/customers.sql.json",
            "03_Indexes/ix_customers_name.sql",
            "03_Indexes/ix_customers_name.sql.json",
            "05_Views/customer_view.sql",
            "05_Views/customer_view.sql.json");
    }

    [Test]
    public void DuplicateIndexNamesAreQualifiedWithTheTableName()
    {
        using var context = ScriptTestContext.SqlServer();

        context.Generate(db =>
        {
            db.Tables.Add(Db.Table("dbo", "A", ("Name", "NVARCHAR")));
            db.Tables.Add(Db.Table("dbo", "B", ("Name", "NVARCHAR")));
            db.Indexes.Add(Db.Index("dbo", "A", "IX_Name", "Name"));
            db.Indexes.Add(Db.Index("dbo", "B", "IX_Name", "Name"));
        });

        context.Folder.Files().Where(i => i.StartsWith("03_Indexes/", StringComparison.Ordinal)).Should().Equal(
            "03_Indexes/dbo.B.IX_Name.sql",
            "03_Indexes/dbo.B.IX_Name.sql.json",
            "03_Indexes/dbo.IX_Name.sql",
            "03_Indexes/dbo.IX_Name.sql.json");
    }

    [Test]
    public void RemovesScriptsForDroppedObjectsAndLeavesOtherFilesAlone()
    {
        using var context = ScriptTestContext.SqlServer();
        context.Folder.Write("05_Views/dbo.OldView.sql", "CREATE VIEW dbo.OldView AS SELECT 1");
        context.Folder.Write("05_Views/dbo.OldView.sql.json", "{}");
        context.Folder.Write("05_Views/notes.txt", "kept");
        context.Folder.Write("04_Migrations/2026-01-01/01_Data.sql", "UPDATE dbo.Customer SET Name = ''");
        context.Folder.Write("01_Always/helpers.sql", "SELECT 1");

        context.Generate(Samples.Customers);

        context.Folder.Exists("05_Views/dbo.OldView.sql").Should().BeFalse();
        context.Folder.Exists("05_Views/dbo.OldView.sql.json").Should().BeFalse();
        context.Folder.Exists("05_Views/notes.txt").Should().BeTrue();
        context.Folder.Exists("04_Migrations/2026-01-01/01_Data.sql").Should().BeTrue();
        context.Folder.Exists("01_Always/helpers.sql").Should().BeTrue();
    }

    [Test]
    public void UnchangedFilesAreNotRewritten()
    {
        using var context = ScriptTestContext.SqlServer();
        context.Generate(Samples.Customers);
        var past = new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        foreach (var file in context.Folder.Files())
        {
            File.SetLastWriteTimeUtc(context.Folder.PathOf(file), past);
        }

        context.Generate(db =>
        {
            Samples.Customers(db);
            db.Definitions[1] = Db.View("dbo", "CustomerView", "CREATE VIEW [dbo].[CustomerView] AS SELECT [Id] FROM [dbo].[Customer]");
        });

        var rewritten = context.Folder.Files().Where(i => File.GetLastWriteTimeUtc(context.Folder.PathOf(i)) != past);
        rewritten.Should().Equal("05_Views/dbo.CustomerView.sql", "05_Views/dbo.CustomerView.sql.json");
    }

    [Test]
    public void TableChangesNeverCreateMigrations()
    {
        // Migrations are written by hand; scripting only updates the table script and model.
        using var context = ScriptTestContext.SqlServer();
        context.Generate(db => db.Tables.Add(Db.Table("dbo", "Customer", ("Id", "INT"), ("Name", "NVARCHAR"))));

        context.Generate(db =>
        {
            db.Tables.Add(Db.Table("dbo", "Customer", ("Id", "BIGINT"), ("Email", "NVARCHAR")));
            db.Tables.Add(Db.Table("dbo", "Order", ("Id", "INT")));
        });

        context.Folder.Read("02_Tables/dbo.Customer.sql").Should().Contain("Email").And.NotContain("[Name]");
        context.Folder.Exists("04_Migrations").Should().BeFalse();
    }

    [Test]
    public void ReadsTheDatabaseWithTheObjectFilterAndWithoutDoubleUnderscoreObjects()
    {
        using var context = ScriptTestContext.SqlServer();

        var database = context.Generate(Samples.Customers, objectFilter: "Customer");

        database.ModelReads.Should().ContainSingle();
        database.ModelReads[0]!.ObjectFilter.Should().Be("Customer");
        database.ModelReads[0]!.ExcludeDoubleUnderscoreObjects.Should().BeTrue();
    }

    [Test]
    public void CanScriptWithADifferentProvider()
    {
        using var context = ScriptTestContext.SqlServer();
        var mysql = new MySqlProvider([], []);

        context.Generate(db => db.Tables.Add(Db.Table("dbo", "Customer", ("Id", "INT"))), createScriptsProvider: mysql);

        Normalize(context.Folder.Read("02_Tables/dbo.Customer.sql")).Should().Be(Normalize(mysql.GetScript(Db.Table("dbo", "Customer", ("Id", "INT")))!.CreateScript));
    }

    [Test]
    public void ReportsProgressForEachFolderAndCompletes()
    {
        using var context = ScriptTestContext.SqlServer();
        var database = ScriptTestContext.Database(Samples.Customers);
        var progress = new List<Temelie.Database.Services.ScriptProgress>();

        context.CreateService(database).CreateScripts(context.Connection, context.Folder.Directory, progress.Add);

        progress.Select(i => i.ProgressStatus).Should().Equal([.. ObjectFolders, "Complete"]);
        progress[^1].ProgressPercentage.Should().Be(100);
    }
}
