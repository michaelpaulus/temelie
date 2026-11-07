using Temelie.Database.Models;

namespace Temelie.Database.UnitTests;

/// <summary>Characterizes <c>IScriptService.ExecuteScripts</c> (deploying a script folder to a database).</summary>
public class ExecuteScriptsTests
{
    private static readonly string[] CustomerObjectSteps =
    [
        "CREATE 02_Tables/dbo.Customer",
        "CREATE 02_Tables/dbo.Order",
        "CREATE 03_Indexes/dbo.Customer.PK_Customer",
        "CREATE 03_Indexes/dbo.Order.PK_Order",
        "CREATE 03_Indexes/dbo.Customer.IX_Customer_Name",
        "CREATE 04_CheckConstraints/dbo.Customer.CK_Customer_Id",
        "CREATE 05_Programmability/dbo.GetCustomers",
        "CREATE 05_Views/dbo.CustomerView",
        "CREATE 06_Triggers/dbo.trCustomer",
        "CREATE 08_ForeignKeys/dbo.Order.FK_Order_Customer"
    ];

    [Test]
    public void FreshDatabaseCreatesEveryObjectInFolderOrder()
    {
        using var context = ScriptTestContext.SqlServer();
        context.Generate(Samples.Customers);
        var database = ScriptTestContext.Database();

        var run = context.Execute(database);

        run.Error.Should().BeNull();
        run.Steps.Should().Equal(CustomerObjectSteps);
        var expected = new DatabaseState();
        Samples.Customers(expected);
        database.Executed.Should().Contain(context.Provider.GetScript(expected.Tables[0])!.CreateScript);
        database.Executed.Should().Contain(context.Provider.GetScript(expected.Definitions[1])!.CreateScript);
        run.Progress[^1].ProgressStatus.Should().Be("Complete");
        run.Progress[^1].ProgressPercentage.Should().Be(100);
    }

    [Test]
    public void UpToDateDatabaseRunsNoObjectScripts()
    {
        using var context = ScriptTestContext.SqlServer();
        context.Generate(Samples.Customers);

        var run = context.Execute(ScriptTestContext.Database(Samples.Customers));

        run.Error.Should().BeNull();
        run.Steps.Should().BeEmpty();
        run.TableChanges.Should().BeEmpty();
    }

    [Test]
    public void ChangedObjectsAreDroppedAndRecreated()
    {
        using var context = ScriptTestContext.SqlServer();
        context.Generate(db =>
        {
            Samples.Customers(db);
            db.Definitions[1] = Db.View("dbo", "CustomerView", "CREATE VIEW [dbo].[CustomerView] AS SELECT [Id] FROM [dbo].[Customer]");
        });

        var run = context.Execute(ScriptTestContext.Database(Samples.Customers));

        run.Steps.Should().Equal("DROP 05_Views/dbo.CustomerView", "CREATE 05_Views/dbo.CustomerView");
    }

    [Test]
    public void RemovedObjectsAreDropped()
    {
        using var context = ScriptTestContext.SqlServer();
        context.Generate(Samples.Customers);
        var database = ScriptTestContext.Database(db =>
        {
            Samples.Customers(db);
            db.Indexes.Add(Db.Index("dbo", "Customer", "IX_Old", "Id"));
            db.Definitions.Add(Db.View("dbo", "OldView", "CREATE VIEW [dbo].[OldView] AS SELECT 1 AS [Id]"));
        });

        var run = context.Execute(database);

        run.Steps.Should().Equal("DROP 03_Indexes/dbo.Customer.IX_Old", "DROP 05_Views/dbo.OldView");
        database.Executed.Should().Contain(context.Provider.GetScript(Db.Index("dbo", "Customer", "IX_Old", "Id"), false)!.DropScript);
    }

    [Test]
    public void TableChangesRecreateProgrammabilityViewsWithoutIndexesAndTriggers()
    {
        using var context = ScriptTestContext.SqlServer();
        const string indexedView = "CREATE VIEW [dbo].[IndexedView] WITH SCHEMABINDING AS SELECT [Id] FROM [dbo].[Customer]";
        void IndexedView(DatabaseState db)
        {
            db.Definitions.Add(Db.View("dbo", "IndexedView", indexedView));
            db.Indexes.Add(Db.Index("dbo", "IndexedView", "IX_IndexedView", "Id"));
        }

        context.Generate(db =>
        {
            Samples.Customers(db);
            IndexedView(db);
            db.Tables[0].Columns.Add(Db.Column("dbo", "Customer", "Email", "NVARCHAR", 3));
        });

        var run = context.Execute(ScriptTestContext.Database(db =>
        {
            Samples.Customers(db);
            IndexedView(db);
        }));

        run.TableChanges.Should().Equal("Customer");
        run.Steps.Should().Equal(
            "DROP 05_Programmability/dbo.GetCustomers",
            "CREATE 05_Programmability/dbo.GetCustomers",
            "DROP 05_Views/dbo.CustomerView",
            "CREATE 05_Views/dbo.CustomerView",
            "DROP 06_Triggers/dbo.trCustomer",
            "CREATE 06_Triggers/dbo.trCustomer");
    }

    [Test]
    public void CollationOnlyDifferencesAreNotTableChanges()
    {
        using var context = ScriptTestContext.SqlServer();
        context.Generate(db =>
        {
            Samples.Customers(db);
            db.Tables[0].CollationName = "Latin1_General_CI_AS";
            db.Tables[0].Columns[1].CollationName = "Latin1_General_CI_AS";
        });

        var run = context.Execute(ScriptTestContext.Database(Samples.Customers));

        run.TableChanges.Should().BeEmpty();
        run.Steps.Should().BeEmpty();
    }

    [Test]
    public void RemovedTablesAreRenamedWithADoubleUnderscoreBeforeAnyScript()
    {
        using var context = ScriptTestContext.SqlServer();
        context.Generate(Samples.Customers);
        context.Folder.Write("01_Always/01_Before.sql", "SELECT 1");
        var legacy = Db.Table("dbo", "Legacy");
        var rename = context.Provider.GetRenameScript(legacy, "__Legacy", dropNewTableIfExists: true);
        var database = ScriptTestContext.Database(db =>
        {
            Samples.Customers(db);
            db.Tables.Add(Db.Table("dbo", "Legacy"));
        }).WhenExecuted(rename, db => db.Tables.RemoveAll(i => i.TableName == "Legacy"));

        var run = context.Execute(database);

        run.Steps.Should().Equal("__Legacy", "01_Always/01_Before.sql");
        database.Executed.Should().Contain(rename);
        database.ModelReads.Should().HaveCount(2, "the schema is read again after renaming");
    }

    [Test]
    public void ScriptFolderWithoutTablesDoesNotRenameExistingTables()
    {
        using var context = ScriptTestContext.SqlServer();
        context.Folder.Write("01_Always/01_Before.sql", "SELECT 1");

        var run = context.Execute(ScriptTestContext.Database(db => db.Tables.Add(Db.Table("dbo", "Customer"))));

        run.Steps.Should().Equal("01_Always/01_Before.sql");
    }

    [Test]
    public void UnscriptedMigrationsTableIsNeverRenamedOrTreatedAsATableChange()
    {
        using var context = ScriptTestContext.SqlServer();
        context.Generate(Samples.Customers);

        var run = context.Execute(ScriptTestContext.Database(db =>
        {
            Samples.Customers(db);
            db.Tables.Add(Db.Table("dbo", "Migrations", ("Id", "NVARCHAR")));
        }));

        run.Steps.Should().BeEmpty();
        run.TableChanges.Should().BeEmpty();
    }

    /// <summary>A SQL Server project layout with interleaved migration folders and script-only folders.</summary>
    private static void WriteSqlServerProjectScripts(ScriptFolder folder)
    {
        folder.Write("01_Always/01_Cleanup.sql", "SELECT 'always'");
        folder.Write("01_Manual/01_Setup.sql", "SELECT 'manual'");
        folder.Write("02_Migrations/2026-01-05-1001/01_Add_Column.sql", "SELECT 'm1'");
        folder.Write("02_Migrations/2026-02-10-1002/01_First.sql", "SELECT 'm2'");
        folder.Write("02_Migrations/2026-02-10-1002/02_Second.sql", "SELECT 'm3'");
        folder.Write("02Z_Migrations/2026-03-15-1003/01_Seed.sql", "SELECT 'm4'");
        folder.Write("04_Custom/dbo.Types.sql", "SELECT 'custom'");
        folder.Write("04_Migrations/2026-04-20-1004/01_Migrate.sql", "SELECT 'm5'");
        folder.Write("05Z_Migrations/2026-05-25-1005/01_Backfill.sql", "SELECT 'm6'");
        folder.Write("09_Always/09_Refresh.sql", "SELECT 'refresh'");
        folder.Write("bin/Debug/net10.0/04_Migrations/ignored.sql", "SELECT 'bin'");
    }

    private static readonly string[] SqlServerProjectMigrations =
    [
        "02_Migrations/2026-01-05-1001/01_Add_Column.sql",
        "02_Migrations/2026-02-10-1002/01_First.sql",
        "02_Migrations/2026-02-10-1002/02_Second.sql",
        "02Z_Migrations/2026-03-15-1003/01_Seed.sql",
        "04_Migrations/2026-04-20-1004/01_Migrate.sql",
        "05Z_Migrations/2026-05-25-1005/01_Backfill.sql"
    ];

    [Test]
    public void FoldersRunInNameOrderWithMigrationsInterleaved()
    {
        using var context = ScriptTestContext.SqlServer();
        context.Generate(Samples.Customers);
        WriteSqlServerProjectScripts(context.Folder);
        var database = ScriptTestContext.Database();

        var run = context.Execute(database);

        run.Error.Should().BeNull();
        run.Steps.Should().Equal(
            "01_Always/01_Cleanup.sql",
            "01_Manual/01_Setup.sql",
            "02_Migrations/2026-01-05-1001/01_Add_Column.sql",
            "02_Migrations/2026-02-10-1002/01_First.sql",
            "02_Migrations/2026-02-10-1002/02_Second.sql",
            "CREATE 02_Tables/dbo.Customer",
            "CREATE 02_Tables/dbo.Order",
            "02Z_Migrations/2026-03-15-1003/01_Seed.sql",
            "CREATE 03_Indexes/dbo.Customer.PK_Customer",
            "CREATE 03_Indexes/dbo.Order.PK_Order",
            "CREATE 03_Indexes/dbo.Customer.IX_Customer_Name",
            "CREATE 04_CheckConstraints/dbo.Customer.CK_Customer_Id",
            "04_Custom/dbo.Types.sql",
            "04_Migrations/2026-04-20-1004/01_Migrate.sql",
            "CREATE 05_Programmability/dbo.GetCustomers",
            "CREATE 05_Views/dbo.CustomerView",
            "05Z_Migrations/2026-05-25-1005/01_Backfill.sql",
            "CREATE 06_Triggers/dbo.trCustomer",
            "CREATE 08_ForeignKeys/dbo.Order.FK_Order_Customer",
            "09_Always/09_Refresh.sql");
        database.Migrations.Should().Equal(SqlServerProjectMigrations.Select(i => new MigrationRecord(i, false)));
        database.Executed.Should().NotContain("SELECT 'bin'");
    }

    [Test]
    public void LaterRunsSkipRecordedMigrationsButRerunOtherScripts()
    {
        using var context = ScriptTestContext.SqlServer();
        context.Generate(Samples.Customers);
        WriteSqlServerProjectScripts(context.Folder);
        var database = ScriptTestContext.Database(Samples.Customers).WithMigrations(SqlServerProjectMigrations);

        var run = context.Execute(database);

        run.Steps.Should().Equal(
            "01_Always/01_Cleanup.sql",
            "01_Manual/01_Setup.sql",
            "04_Custom/dbo.Types.sql",
            "09_Always/09_Refresh.sql");
        database.Migrations.Should().HaveCount(SqlServerProjectMigrations.Length);
    }

    [Test]
    public void SkipMigrationsRecordsMigrationsWithoutRunningThem()
    {
        using var context = ScriptTestContext.SqlServer();
        context.Folder.Write("01_Always/01_Before.sql", "SELECT 'always'");
        context.Folder.Write("04_Migrations/2026-01-01/01_First.sql", "SELECT 'first'");

        var database = ScriptTestContext.Database();
        var run = context.Execute(database, skipMigrations: true);

        run.Steps.Should().Equal("01_Always/01_Before.sql", "04_Migrations/2026-01-01/01_First.sql");
        database.Executed.Should().Contain("SELECT 'always'").And.NotContain("SELECT 'first'");
        database.Migrations.Should().Equal(new MigrationRecord("04_Migrations/2026-01-01/01_First.sql", true));
    }

    [Test]
    public void SchemaIsReadAgainAfterEachMigrationFolder()
    {
        using var context = ScriptTestContext.SqlServer();
        const string newView = "CREATE VIEW [dbo].[CustomerView] AS SELECT [Id] FROM [dbo].[Customer]";
        context.Generate(db =>
        {
            Samples.Customers(db);
            db.Definitions[1] = Db.View("dbo", "CustomerView", newView);
        });
        const string migration = "CREATE OR ALTER VIEW [dbo].[CustomerView] AS SELECT [Id] FROM [dbo].[Customer]";
        context.Folder.Write("04_Migrations/2026-06-30-1006/01_CustomerView.sql", migration);
        var database = ScriptTestContext.Database(Samples.Customers)
            .WhenExecuted(migration, db => db.Definitions[1] = Db.View("dbo", "CustomerView", newView));

        var run = context.Execute(database);

        run.Steps.Should().Equal("04_Migrations/2026-06-30-1006/01_CustomerView.sql");
        database.ModelReads.Should().HaveCount(2);
    }

    [Test]
    public void FailedScriptsAreRetriedWhenContinuingOnError()
    {
        using var context = ScriptTestContext.SqlServer();
        var a = Db.View("dbo", "a", "CREATE VIEW [dbo].[a] AS SELECT [Id] FROM [dbo].[b]");
        var b = Db.View("dbo", "b", "CREATE VIEW [dbo].[b] AS SELECT 1 AS [Id]");
        context.Generate(db => db.Definitions.AddRange([a, b]));
        var createA = context.Provider.GetScript(Db.View("dbo", "a", a.Definition))!.CreateScript;
        var createB = context.Provider.GetScript(Db.View("dbo", "b", b.Definition))!.CreateScript;
        var failures = 1;
        var database = ScriptTestContext.Database()
            .FailWhen((sql, _) => sql == createA && failures-- > 0, "Transaction was deadlocked.")
            .WhenExecuted(createA, db => db.Definitions.Add(Db.View("dbo", "a", a.Definition)))
            .WhenExecuted(createB, db => db.Definitions.Add(Db.View("dbo", "b", b.Definition)));

        var run = context.Execute(database);

        run.Error.Should().BeNull();
        run.Failed.Should().Equal("CREATE 05_Views/dbo.a");
        run.Steps.Should().Equal("CREATE 05_Views/dbo.b", "CREATE 05_Views/dbo.a", "Retry 1 of 5", "CREATE 05_Views/dbo.a");
    }

    [Test]
    public void FirstFailureStopsWhenNotContinuingOnError()
    {
        using var context = ScriptTestContext.SqlServer();
        context.Generate(db => db.Definitions.AddRange([
            Db.View("dbo", "a", "CREATE VIEW [dbo].[a] AS SELECT [Id] FROM [dbo].[b]"),
            Db.View("dbo", "b", "CREATE VIEW [dbo].[b] AS SELECT 1 AS [Id]")]));
        var database = ScriptTestContext.Database().FailWhen((sql, _) => sql.Contains("CREATE VIEW [dbo].[b]"), "Transaction was deadlocked.");

        var run = context.Execute(database, continueOnError: false);

        run.Error.Should().NotBeNull();
        run.Error!.Message.Should().Contain("CREATE 05_Views/dbo.b").And.Contain("Transaction was deadlocked.");
        run.Steps.Should().Equal("CREATE 05_Views/dbo.b");
    }

    [Test]
    public void FailedMigrationStopsEachAttemptAndIsNotRecorded()
    {
        using var context = ScriptTestContext.SqlServer();
        context.Folder.Write("01_Always/01_Before.sql", "SELECT 'before'");
        context.Folder.Write("04_Migrations/2026-01-01/01_Bad.sql", "SELECT 'bad'");
        context.Folder.Write("09_Always/09_After.sql", "SELECT 'after'");
        var database = ScriptTestContext.Database().FailWhen((sql, _) => sql == "SELECT 'bad'");

        var run = context.Execute(database);

        run.Error.Should().NotBeNull();
        run.Steps.Count(i => i == "04_Migrations/2026-01-01/01_Bad.sql").Should().Be(6, "the first attempt plus 5 retries");
        run.Steps.Should().NotContain("09_Always/09_After.sql");
        database.Executed.Count(i => i == "SELECT 'before'").Should().Be(6);
        database.Migrations.Should().BeEmpty();
    }

    [Test]
    public void CreatesTheMigrationsTableFirst()
    {
        using var context = ScriptTestContext.SqlServer();
        var database = ScriptTestContext.Database();

        context.Execute(database);

        database.Executed[0].Should().Contain(context.Provider.GetScript(MigrationsTable())!.CreateScript.Trim());
    }

    [Test]
    public void AddsMissingColumnsToAnExistingMigrationsTable()
    {
        using var context = ScriptTestContext.SqlServer();
        var database = ScriptTestContext.Database(db =>
        {
            var table = MigrationsTable();
            table.Columns.RemoveAt(2);
            db.Tables.Add(table);
        });

        context.Execute(database);

        var skipped = Db.Column("dbo", "Migrations", "Skipped", "BIT", 3);
        skipped.IsNullable = true;
        database.Executed[0].Should().Contain(context.Provider.GetColumnScript(skipped)!.CreateScript.Trim());
    }

    private static TableModel MigrationsTable()
    {
        var table = new TableModel { SchemaName = "dbo", TableName = "Migrations" };
        table.Columns.Add(new ColumnModel { IsPrimaryKey = true, ColumnName = "Id", ColumnType = "NVARCHAR(500)", ColumnId = 1, SchemaName = "dbo" });
        table.Columns.Add(new ColumnModel { ColumnName = "Date", ColumnType = "DATETIME", ColumnId = 2, SchemaName = "dbo" });
        table.Columns.Add(new ColumnModel { ColumnName = "Skipped", ColumnType = "BIT", IsNullable = true, TableName = "Migrations", ColumnId = 3, SchemaName = "dbo" });
        return table;
    }

    [Test]
    public void MySqlProjectLayoutRunsInFolderOrder()
    {
        // A MySQL project layout with migrations before tables, between indexes and views, and before foreign keys.
        using var context = ScriptTestContext.MySql();
        context.Generate(db =>
        {
            db.Tables.Add(Db.Table("", "customers", ("id", "INT")));
            db.Tables.Add(Db.Table("", "orders", ("id", "INT"), ("customer_id", "INT")));
            db.Indexes.Add(Db.Index("", "orders", "ix_orders_customer_id", "customer_id"));
            db.Definitions.Add(Db.View("", "customer_view", "CREATE VIEW `customer_view` AS SELECT `id` FROM `customers`;"));
            db.Triggers.Add(Db.Trigger("", "customers", "tr_customers", "CREATE TRIGGER `tr_customers` BEFORE INSERT ON `customers` FOR EACH ROW SET NEW.id = NEW.id;"));
            db.ForeignKeys.Add(Db.ForeignKey("", "orders", "fk_orders_customers", "customer_id", "customers", "id"));
        });
        const string procedure = "DROP PROCEDURE IF EXISTS `AddColumnIfMissing`\nGO\n\nCREATE PROCEDURE `AddColumnIfMissing`() BEGIN END;";
        context.Folder.Write("01_Always/AddColumnIfMissing.sql", procedure);
        context.Folder.Write("02_Migrations/20260105-1001/01_prepare.sql", "SELECT 'm1'");
        context.Folder.Write("04_Migrations/20260210-1002/01_columns.sql", "SELECT 'm2'");
        context.Folder.Write("07_Migrations/20260210-1002/01_seed_types.sql", "SELECT 'm3'");
        var database = ScriptTestContext.Database();

        var run = context.Execute(database);

        run.Error.Should().BeNull();
        run.Steps.Should().Equal(
            "01_Always/AddColumnIfMissing.sql",
            "02_Migrations/20260105-1001/01_prepare.sql",
            "CREATE 02_Tables/.customers",
            "CREATE 02_Tables/.orders",
            "CREATE 03_Indexes/.orders.ix_orders_customer_id",
            "04_Migrations/20260210-1002/01_columns.sql",
            "CREATE 05_Views/.customer_view",
            "CREATE 06_Triggers/.tr_customers",
            "07_Migrations/20260210-1002/01_seed_types.sql",
            "CREATE 08_ForeignKeys/.orders.fk_orders_customers");
        database.Executed.Should().Contain(procedure, "each script file is passed as a whole; GO batches are split when it executes");
    }
}
