using System.Text.Json;
using Temelie.Database.Models;
using Temelie.Database.Providers;
using Temelie.Database.Services;
using MssqlProvider = Temelie.Database.Providers.Mssql.DatabaseProvider;
using MySqlProvider = Temelie.Database.Providers.MySql.DatabaseProvider;

namespace Temelie.Database.UnitTests;

public class DatabaseDependencyTests
{
    private static IDatabaseProvider Provider(bool mysql = false) =>
        mysql ? new MySqlProvider([], []) : new MssqlProvider([], []);

    private static TableModel Table(string name, string schema = "dbo", bool changed = false) =>
        new()
        {
            SchemaName = schema,
            TableName = name,
            Columns = [new ColumnModel { SchemaName = schema, TableName = name, ColumnName = "Id", ColumnId = 1, ColumnType = changed ? "BIGINT" : "INT" }]
        };

    private static DefinitionModel Definition(string name, string sql, string schema = "dbo", string type = "V") =>
        new()
        {
            SchemaName = schema,
            DefinitionName = name,
            Definition = sql,
            XType = type,
            View = type == "V" ? new TableModel { SchemaName = schema, TableName = name, IsView = true } : null
        };

    private static DatabaseModel Model(IEnumerable<TableModel>? tables = null, IEnumerable<DefinitionModel>? definitions = null,
        IEnumerable<IndexModel>? indexes = null, IEnumerable<TriggerModel>? triggers = null,
        IEnumerable<CheckConstraintModel>? checks = null, IEnumerable<SecurityPolicyModel>? policies = null,
        IEnumerable<ForeignKeyModel>? foreignKeys = null)
    {
        var definitionList = definitions?.ToList() ?? [];
        return new(tables ?? [], definitionList.Where(d => d.View is not null).Select(d => d.View!),
            indexes ?? [], triggers ?? [], foreignKeys ?? [], checks ?? [], definitionList, policies ?? []);
    }

    [TestCase(false)]
    [TestCase(true)]
    public void TableChangeRecreatesOnlyTransitiveDependentsInDependencyOrder(bool mysql)
    {
        var schema = mysql ? "" : "dbo";
        DefinitionModel[] Definitions() =>
        [
            Definition("zBase", $"CREATE VIEW zBase AS SELECT Id FROM {Quote("T", mysql)}", schema),
            Definition("aChild", "CREATE VIEW aChild AS SELECT Id FROM zBase", schema),
            Definition("aProcedure", "CREATE PROCEDURE aProcedure AS SELECT Id FROM aChild", schema, "P"),
            Definition("unrelated", $"CREATE VIEW unrelated AS SELECT Id FROM {Quote("Other", mysql)}", schema)
        ];
        var plan = new DatabaseDependencyPlan(
            Model([Table("T", schema), Table("Other", schema)], Definitions()),
            Model([Table("T", schema, true), Table("Other", schema)], Definitions()), Provider(mysql), []);

        Assert.That(plan.Creates.Select(s => s.Key), Is.EqualTo(new[] { $"{schema}.zBase", $"{schema}.aChild", $"{schema}.aProcedure" }));
        Assert.That(plan.Drops.Select(s => s.Key), Is.EqualTo(plan.Creates.Select(s => s.Key).Reverse()));
    }

    private static string Quote(string name, bool mysql) => mysql ? $"`{name}`" : $"[dbo].[{name}]";

    [Test]
    public void ChangedFunctionOrdersAcrossViewAndProgrammabilityDirectories()
    {
        var current = Model(definitions:
        [
            Definition("zFunction", "CREATE FUNCTION dbo.zFunction() RETURNS int AS BEGIN RETURN 1 END", type: "FN"),
            Definition("aView", "CREATE VIEW dbo.aView AS SELECT dbo.zFunction() AS Value"),
            Definition("aProcedure", "CREATE PROCEDURE dbo.aProcedure AS SELECT * FROM dbo.aView", type: "P")
        ]);
        var updated = Model(definitions:
        [
            Definition("zFunction", "CREATE FUNCTION dbo.zFunction() RETURNS int AS BEGIN RETURN 2 END", type: "FN"),
            Definition("aView", "CREATE VIEW dbo.aView AS SELECT dbo.zFunction() AS Value"),
            Definition("aProcedure", "CREATE PROCEDURE dbo.aProcedure AS SELECT * FROM dbo.aView", type: "P")
        ]);
        var plan = new DatabaseDependencyPlan(current, updated, Provider(), []);
        Assert.That(plan.Creates.Select(s => s.Key), Is.EqualTo(new[] { "dbo.zFunction", "dbo.aView", "dbo.aProcedure" }));
        Assert.That(plan.Drops.Select(s => s.Key), Is.EqualTo(new[] { "dbo.aProcedure", "dbo.aView", "dbo.zFunction" }));
    }

    [Test]
    public void OldDependenciesAreUsedForDropsAndNewDependenciesForCreates()
    {
        var current = Model([Table("T")], [
            Definition("zOld", "CREATE VIEW dbo.zOld AS SELECT * FROM dbo.T"),
            Definition("aChild", "CREATE VIEW dbo.aChild AS SELECT * FROM dbo.zOld")]);
        var updated = Model([Table("T")], [
            Definition("zNew", "CREATE VIEW dbo.zNew AS SELECT * FROM dbo.T"),
            Definition("aChild", "CREATE VIEW dbo.aChild AS SELECT * FROM dbo.zNew")]);
        var plan = new DatabaseDependencyPlan(current, updated, Provider(), []);
        Assert.That(plan.Drops.Select(s => s.Key), Is.EqualTo(new[] { "dbo.aChild", "dbo.zOld" }));
        Assert.That(plan.Creates.Select(s => s.Key), Is.EqualTo(new[] { "dbo.zNew", "dbo.aChild" }));
    }

    [Test]
    public void SchemaQualifiedReferencesDoNotAffectSameNameInOtherSchemas()
    {
        DefinitionModel[] Definitions() => [
            Definition("v", "CREATE VIEW one.v AS SELECT * FROM [one].[T]", "one"),
            Definition("v", "CREATE VIEW two.v AS SELECT * FROM T", "two")];
        var plan = new DatabaseDependencyPlan(
            Model([Table("T", "one"), Table("T", "two")], Definitions()),
            Model([Table("T", "one", true), Table("T", "two")], Definitions()), Provider(), []);
        Assert.That(plan.Creates.Select(s => s.Key), Is.EqualTo(new[] { "one.v" }));
    }

    [Test]
    public void StaticSqlInferenceIgnoresCommentsStringsCtesAndAliases()
    {
        var sql = """
            /* FROM dbo.Fake /* JOIN dbo.Nested */ */
            WITH T AS (SELECT * FROM dbo.Real)
            SELECT 'FROM dbo.String', t.Id FROM T t
            JOIN [dbo].[Other] o ON t.Id = o.Id -- JOIN dbo.Comment
            WHERE EXISTS (SELECT 1 FROM `dbo`.`Third`)
            """;
        var references = SqlDependencyReader.Read(sql).ToList();
        Assert.That(references.Select(r => r.Name), Is.EquivalentTo(new[] { "Real", "Other", "Third" }));
    }

    [Test]
    public void InferenceHandlesCommaJoinsFunctionsAndProcedureCalls()
    {
        var references = SqlDependencyReader.Read("SELECT dbo.f(a.Id) FROM dbo.A a, dbo.B b CROSS APPLY dbo.g(a.Id); EXEC dbo.p; UPDATE dbo.A SET Id = 1; INSERT INTO dbo.B VALUES (1)").ToList();
        Assert.That(references.Select(r => r.Name), Does.Contain("A"));
        Assert.That(references.Select(r => r.Name), Does.Contain("B"));
        Assert.That(references.Select(r => r.Name), Does.Contain("f"));
        Assert.That(references.Select(r => r.Name), Does.Contain("g"));
        Assert.That(references.Select(r => r.Name), Does.Contain("p"));
    }

    [Test]
    public void InferenceHandlesReturnAssignmentsMysqlCallsAndStatementScopedCtes()
    {
        var references = SqlDependencyReader.Read("WITH T AS (SELECT * FROM dbo.Source) SELECT * FROM T; SELECT * FROM T; EXEC @result = dbo.p; CALL `f`(); # FROM Fake\n SELECT * FROM `Other`", mysql: true).ToList();
        Assert.That(references.Select(r => r.Name), Does.Contain("p"));
        Assert.That(references.Select(r => r.Name), Does.Contain("f"));
        Assert.That(references.Count(r => r.Name == "T"), Is.EqualTo(1));
        Assert.That(references.Select(r => r.Name), Does.Not.Contain("Fake"));
    }

    [Test]
    public void NewFunctionsAreCreatedBeforeTablesThatUseThem()
    {
        var table = Table("T");
        table.Columns[0].ColumnDefault = "(dbo.f())";
        var model = Model([table], [Definition("f", "CREATE FUNCTION dbo.f() RETURNS int AS BEGIN RETURN 1 END", type: "FN")]);
        var plan = new DatabaseDependencyPlan(Model(), model, Provider(), []);
        Assert.That(plan.Creates.Select(s => s.Key), Is.EqualTo(new[] { "dbo.f", "dbo.T" }));
        Assert.That(plan.Creates.Select(s => s.Slot), Is.All.EqualTo(SlotOf("02_Tables")), "new tables keep their folder position");
    }

    [Test]
    public void ViewsUsedByRoutinesMoveTheRoutineLater()
    {
        var model = Model(definitions: [
            Definition("f", "CREATE FUNCTION dbo.f() RETURNS TABLE AS RETURN SELECT * FROM dbo.v", type: "IF"),
            Definition("v", "CREATE VIEW dbo.v AS SELECT 1 AS Id")]);
        var plan = new DatabaseDependencyPlan(Model(), model, Provider(), []);
        Assert.That(plan.Creates.Select(s => s.Key), Is.EqualTo(new[] { "dbo.v", "dbo.f" }));
        Assert.That(plan.Creates.Select(s => s.Slot), Is.All.EqualTo(SlotOf("05_Views")));
    }
    private static int SlotOf(string folder) => DatabaseDependencyPlan.DefaultFolderOrder.ToList().IndexOf(folder);

    [Test]
    public void ChangedForeignKeyRecreatesOnlyThatForeignKey()
    {
        ForeignKeyModel ForeignKey(string column) => new()
        {
            SchemaName = "dbo",
            TableName = "T",
            ForeignKeyName = "FK_T",
            ReferencedSchemaName = "dbo",
            ReferencedTableName = "Other",
            DeleteAction = "NO_ACTION",
            UpdateAction = "NO_ACTION",
            Detail = [new ForeignKeyDetailModel { Column = column, ReferencedColumn = "Id" }]
        };
        var plan = new DatabaseDependencyPlan(Model([Table("T"), Table("Other")], foreignKeys: [ForeignKey("Id")]),
            Model([Table("T"), Table("Other")], foreignKeys: [ForeignKey("OtherId")]), Provider(), []);
        Assert.That(plan.Drops.Count, Is.EqualTo(1));
        Assert.That(plan.Creates.Count, Is.EqualTo(1));
        Assert.That(plan.Creates.Single().Name, Is.EqualTo("CREATE 08_ForeignKeys/dbo.T.FK_T"));
    }

    [TestCase(false)]
    [TestCase(true)]
    public void ExecutedMigrationChangesSeedDependentsEvenWhenModelsMatch(bool mysql)
    {
        var schema = mysql ? "" : "dbo";
        DatabaseModel Snapshot() => Model([Table("T", schema)], [Definition("v", "CREATE VIEW v AS SELECT * FROM T", schema)]);
        var plan = new DatabaseDependencyPlan(Snapshot(), Snapshot(), Provider(mysql), ["ALTER TABLE T ADD NewColumn INT"]);
        Assert.That(plan.Creates.Select(s => s.Key), Is.EqualTo(new[] { $"{schema}.v" }));
        Assert.That(plan.Creates.Single().Slot, Is.EqualTo(SlotOf("05_Views")));
    }

    [Test]
    public void ReadOnlyMigrationDoesNotForceRecreation()
    {
        DatabaseModel Snapshot() => Model([Table("T")], [Definition("v", "CREATE VIEW v AS SELECT * FROM T")]);
        var plan = new DatabaseDependencyPlan(Snapshot(), Snapshot(), Provider(), ["SELECT * FROM T"]);
        Assert.That(plan.Creates, Is.Empty);
        Assert.That(plan.Drops, Is.Empty);
    }

    [TestCase(false)]
    [TestCase(true)]
    public void LocalDatabaseQualificationIsResolvedButExternalDatabaseIsIgnored(bool mysql)
    {
        var schema = mysql ? "" : "dbo";
        var local = mysql ? "`Local`.`T`" : "[Local].[dbo].[T]";
        var external = mysql ? "`OtherDb`.`T`" : "[OtherDb].[dbo].[T]";
        DefinitionModel[] Definitions() => [
            Definition("localView", $"CREATE VIEW localView AS SELECT * FROM {local}", schema),
            Definition("externalView", $"CREATE VIEW externalView AS SELECT * FROM {external}", schema)];
        var plan = new DatabaseDependencyPlan(Model([Table("T", schema)], Definitions()),
            Model([Table("T", schema, true)], Definitions()), Provider(mysql), [], databaseName: "Local");
        Assert.That(plan.Creates.Select(s => s.Key), Is.EqualTo(new[] { $"{schema}.localView" }));
    }

    [Test]
    public void CollationOnlyChangesDoNotRecreateDependentsAndNewTablesKeepCollation()
    {
        var original = Table("T");
        var changed = Table("T");
        changed.CollationName = "Latin1_General_CI_AS";
        changed.Columns[0].CollationName = "Latin1_General_CI_AS";
        var plan = new DatabaseDependencyPlan(Model([original]), Model([changed]), Provider(), []);
        Assert.That(plan.Creates, Is.Empty);
        var mysqlTable = Table("NewTable", "");
        mysqlTable.CharacterSetName = "utf8mb4";
        mysqlTable.CollationName = "utf8mb4_general_ci";
        plan = new DatabaseDependencyPlan(Model(), Model([mysqlTable]), Provider(true), []);
        Assert.That(plan.Creates.Single().Contents, Does.Contain("COLLATE=utf8mb4_general_ci"));
    }

    private static DatabaseModel IndexedViewModel(bool tableChanged = false, string viewColumn = "Id", int functionValue = 1, bool indexUnique = true,
        IEnumerable<IndexModel>? tableIndexes = null) =>
        Model([Table("T", changed: tableChanged)],
            [
                Definition("f", $"CREATE FUNCTION dbo.f(@Id int) RETURNS int WITH SCHEMABINDING AS BEGIN RETURN {functionValue} END", type: "FN"),
                Definition("v", $"CREATE VIEW dbo.v WITH SCHEMABINDING AS SELECT {viewColumn}, dbo.f(Id) AS Value FROM dbo.T"),
                Definition("reader", "CREATE PROCEDURE dbo.reader AS SELECT * FROM dbo.v", type: "P")
            ],
            [new IndexModel { SchemaName = "dbo", TableName = "v", IndexName = "IX_v", IndexType = "CLUSTERED", IsUnique = indexUnique, Columns = [new IndexColumnModel { ColumnName = "Id" }] },
             .. tableIndexes ?? []]);

    [Test]
    public void IndexedViewsAreNotTouchedByTableChangesOrMigrations()
    {
        var plan = new DatabaseDependencyPlan(IndexedViewModel(), IndexedViewModel(tableChanged: true), Provider(),
            ["ALTER TABLE dbo.T ALTER COLUMN Id BIGINT"]);
        Assert.That(plan.Drops, Is.Empty);
        Assert.That(plan.Creates, Is.Empty);
    }

    [Test]
    public void UnchangedIndexedViewsAreNotScheduled()
    {
        var plan = new DatabaseDependencyPlan(IndexedViewModel(), IndexedViewModel(), Provider(), []);
        Assert.That(plan.Drops, Is.Empty);
        Assert.That(plan.Creates, Is.Empty);
    }

    [TestCase("view")]
    [TestCase("index")]
    [TestCase("removed")]
    public void ChangedIndexedViewsAreLeftToAMigration(string change)
    {
        var updated = change switch
        {
            "view" => IndexedViewModel(viewColumn: "Id AS Id"),
            "index" => IndexedViewModel(indexUnique: false),
            _ => Model([Table("T")], [Definition("f", "CREATE FUNCTION dbo.f(@Id int) RETURNS int WITH SCHEMABINDING AS BEGIN RETURN 1 END", type: "FN")])
        };
        var plan = new DatabaseDependencyPlan(IndexedViewModel(), updated, Provider(), []);
        Assert.That(plan.Drops.Select(s => s.Name), Is.Empty.Or.EqualTo(new[] { "DROP 05_Programmability/dbo.reader" }));
        Assert.That(plan.Drops.Concat(plan.Creates).Select(s => s.Name), Has.None.Contain("dbo.v"));
        Assert.That(plan.SkippedChanges, Is.Not.Empty);
        Assert.That(plan.SkippedChanges, Has.All.Contain("dbo.v"));
    }

    [Test]
    public void MissingIndexedViewsAndIndexesAreCreated()
    {
        var plan = new DatabaseDependencyPlan(Model([Table("T")]), IndexedViewModel(), Provider(), []);
        Assert.That(plan.Creates.Select(s => s.Name), Is.EqualTo(new[]
        {
            "CREATE 05_Programmability/dbo.f", "CREATE 05_Views/dbo.v", "CREATE 03_Indexes/dbo.v.IX_v", "CREATE 05_Programmability/dbo.reader"
        }));
        Assert.That(plan.Creates[2].Contents, Does.Contain("CREATE UNIQUE CLUSTERED INDEX"));
        Assert.That(plan.SkippedChanges, Is.Empty);
    }

    [Test]
    public void ChangedSchemaBoundFunctionUsedByAnIndexedViewIsLeftToAMigration()
    {
        // Functions used by an indexed view must be schema-bound themselves, so they are locked too.
        var plan = new DatabaseDependencyPlan(IndexedViewModel(), IndexedViewModel(functionValue: 2), Provider(), []);
        Assert.That(plan.Drops, Is.Empty);
        Assert.That(plan.Creates, Is.Empty);
        Assert.That(plan.SkippedChanges, Is.EqualTo(new[] { "05_Programmability/dbo.f" }));
    }

    [Test]
    public void ChangedFunctionUsedByASchemaBoundViewIsAppliedWithoutTouchingTheView()
    {
        // The database refuses to drop a function a schema-bound view uses, so the deployment fails there; no explicit check.
        DatabaseModel Snapshot(int value) => Model([Table("T")], [
            Definition("f", $"CREATE FUNCTION dbo.f(@Id int) RETURNS int AS BEGIN RETURN {value} END", type: "FN"),
            Definition("bound", "CREATE VIEW dbo.bound WITH SCHEMABINDING AS SELECT dbo.f(Id) AS Value FROM dbo.T")]);
        var plan = new DatabaseDependencyPlan(Snapshot(1), Snapshot(2), Provider(), []);
        Assert.That(plan.Drops.Select(s => s.Name), Is.EqualTo(new[] { "DROP 05_Programmability/dbo.f" }));
        Assert.That(plan.Creates.Select(s => s.Name), Is.EqualTo(new[] { "CREATE 05_Programmability/dbo.f" }));
        Assert.That(plan.SkippedChanges, Is.Empty);
    }

    [Test]
    public void ChangedSchemaBoundViewIsLeftToAMigration()
    {
        DatabaseModel Snapshot(string column) => Model([Table("T")], [
            Definition("bound", $"CREATE VIEW dbo.bound WITH SCHEMABINDING AS SELECT {column} FROM dbo.T"),
            Definition("caller", "CREATE PROCEDURE dbo.caller AS SELECT * FROM dbo.bound", type: "P")]);
        var plan = new DatabaseDependencyPlan(Snapshot("Id"), Snapshot("Id AS Value"), Provider(), []);
        Assert.That(plan.Drops, Is.Empty);
        Assert.That(plan.Creates, Is.Empty);
        Assert.That(plan.SkippedChanges, Is.EqualTo(new[] { "05_Views/dbo.bound" }));
    }
    [Test]
    public void TableAttachedObjectsAreOnlyRecreatedWhenTheirOwnDefinitionChanges()
    {
        IndexModel[] Indexes(string column) => [new IndexModel { SchemaName = "dbo", TableName = "T", IndexName = "IX_T", IndexType = "NONCLUSTERED", Columns = [new IndexColumnModel { ColumnName = column }] }];
        DatabaseModel Snapshot(bool changed, string column) => Model([Table("T", changed: changed)], indexes: Indexes(column),
            checks: [new CheckConstraintModel { SchemaName = "dbo", TableName = "T", CheckConstraintName = "CK_T", CheckConstraintDefinition = "([Id]>(0))" }]);

        var tableChange = new DatabaseDependencyPlan(Snapshot(false, "Id"), Snapshot(true, "Id"), Provider(), ["ALTER TABLE dbo.T ADD Other INT"]);
        Assert.That(tableChange.Drops, Is.Empty);
        Assert.That(tableChange.Creates, Is.Empty);
        Assert.That(tableChange.ChangedTables, Is.EqualTo(new[] { "T" }));

        var indexChange = new DatabaseDependencyPlan(Snapshot(false, "Id"), Snapshot(false, "Other"), Provider(), []);
        Assert.That(indexChange.Creates.Select(s => s.Name), Is.EqualTo(new[] { "CREATE 03_Indexes/dbo.T.IX_T" }));
    }
    [Test]
    public void TriggersPoliciesAndChecksFollowTheirDependencies()
    {
        DatabaseModel Snapshot(int value) => Model([Table("T")],
            [Definition("f", $"CREATE FUNCTION dbo.f(@Id int) RETURNS int AS BEGIN RETURN {value} END", type: "FN")],
            triggers: [new TriggerModel { SchemaName = "dbo", TableName = "T", TriggerName = "tr", Definition = "CREATE TRIGGER dbo.tr ON dbo.T AFTER INSERT AS SELECT dbo.f(1)" }],
            checks: [new CheckConstraintModel { SchemaName = "dbo", TableName = "T", CheckConstraintName = "CK_T", CheckConstraintDefinition = "dbo.f(Id) > 0" }],
            policies: [new SecurityPolicyModel { PolicySchema = "dbo", PolicyName = "pol", IsEnabled = true, Predicates = [new SecurityPolicyPredicate { TargetSchema = "dbo", TargetName = "T", PredicateDefinition = "dbo.f(Id)", PredicateType = "FILTER", Operation = "ALL" }] }]);
        var plan = new DatabaseDependencyPlan(Snapshot(1), Snapshot(2), Provider(), []);
        Assert.That(plan.Creates[0].Key, Is.EqualTo("dbo.f"));
        Assert.That(plan.Creates.Count, Is.EqualTo(4));
        Assert.That(plan.Drops[^1].Key, Is.EqualTo("dbo.f"));
        Assert.That(plan.Drops.Single(s => s.Name.Contains("CK_T")).Contents, Does.Not.Contain("IF NOT EXISTS"));
    }

    [Test]
    public void CyclesReportAnExplicitDependencyPathBeforeExecution()
    {
        var model = Model(definitions: [
            Definition("a", "CREATE VIEW dbo.a AS SELECT * FROM dbo.b"),
            Definition("b", "CREATE VIEW dbo.b AS SELECT * FROM dbo.a")]);
        Assert.That(() => new DatabaseDependencyPlan(Model(), model, Provider(), []),
            Throws.InvalidOperationException.With.Message.Contains("dbo.a -> dbo.b -> dbo.a"));
    }

    [Test]
    public void UnchangedCyclesAndUnmanagedCategoriesAreNotScheduled()
    {
        DatabaseModel Snapshot() => Model(definitions: [
            Definition("a", "CREATE VIEW dbo.a AS SELECT * FROM dbo.b"),
            Definition("b", "CREATE VIEW dbo.b AS SELECT * FROM dbo.a")]);
        Assert.That(new DatabaseDependencyPlan(Snapshot(), Snapshot(), Provider(), []).Creates, Is.Empty);
        Assert.That(new DatabaseDependencyPlan(Snapshot(), Model(), Provider(), [], folder => folder == "02_Tables" ? 0 : null).Drops, Is.Empty);
    }

    [TestCase(false, false, false)]
    [TestCase(false, true, false)]
    [TestCase(true, false, false)]
    [TestCase(true, true, false)]
    [TestCase(false, false, true)]
    public void ExecuteScriptsRefreshesTableDependentsInDependencyOrder(bool skipMigrations, bool mysql, bool schemaBound)
    {
        using var context = mysql ? ScriptTestContext.MySql() : ScriptTestContext.SqlServer();
        var root = context.Folder.Directory;
        var schema = mysql ? "" : "dbo";
        DefinitionModel[] Definitions() => [
            Definition("zBase", $"CREATE VIEW zBase {(schemaBound ? "WITH SCHEMABINDING " : "")}AS SELECT Id FROM {Quote("T", mysql)}", schema),
            Definition("aChild", "CREATE VIEW aChild AS SELECT Id FROM zBase", schema),
            Definition("aProcedure", mysql ? "CREATE PROCEDURE aProcedure() SELECT Id FROM aChild;" : "CREATE PROCEDURE dbo.aProcedure AS SELECT Id FROM dbo.aChild", schema, "P")];
        var current = Model([Table("T", schema)], Definitions());
        var updated = Model([Table("T", schema, true)], Definitions());
        WriteModel(root, updated);
        File.WriteAllText(Path.Combine(root.CreateSubdirectory("00_Before_Always").FullName, "Before.sql"), "SELECT 'Before'");
        File.WriteAllText(Path.Combine(root.CreateSubdirectory("10_After_Always").FullName, "After.sql"), "SELECT 'After'");
        var migrationDirectory = root.CreateSubdirectory("04_Migrations").CreateSubdirectory("2026-01-01");
        var migrationSql = mysql ? "ALTER TABLE T MODIFY COLUMN Id BIGINT" : "ALTER TABLE dbo.T ALTER COLUMN Id BIGINT";
        File.WriteAllText(Path.Combine(migrationDirectory.FullName, "01_T.sql"), migrationSql);
        var database = new FakeDatabase(DatabaseState.From(current))
            .WhenExecuted(migrationSql, db => db.Tables[0] = Table("T", schema, true));

        var run = context.Execute(database, continueOnError: false, skipMigrations: skipMigrations);

        Assert.That(run.Error, Is.Null);
        var expected = new List<string> { "00_Before_Always/Before.sql", "04_Migrations/2026-01-01/01_T.sql" };
        if (!schemaBound)
        {
            // Schema-bound views do not go stale, so nothing that depends on them needs a refresh either.
            expected.AddRange([
                $"DROP 05_Programmability/{schema}.aProcedure", $"DROP 05_Views/{schema}.aChild", $"DROP 05_Views/{schema}.zBase",
                $"CREATE 05_Views/{schema}.zBase", $"CREATE 05_Views/{schema}.aChild", $"CREATE 05_Programmability/{schema}.aProcedure"]);
        }

        expected.Add("10_After_Always/After.sql");
        Assert.That(run.Steps, Is.EqualTo(expected));
        Assert.That(run.TableChanges, Is.EqualTo(new[] { "T" }));
        Assert.That(database.Executed.Count(s => s == migrationSql), Is.EqualTo(skipMigrations ? 0 : 1));
        Assert.That(run.Progress[^1].ProgressPercentage, Is.EqualTo(100));
        Assert.That(database.Migrations, Has.Count.EqualTo(1));
    }

    [Test]
    public void ObjectFoldersThatAlreadyRanAreNotRevisitedAfterALaterMigration()
    {
        using var context = ScriptTestContext.SqlServer();
        var root = context.Folder.Directory;
        DatabaseModel Snapshot() => Model([Table("T")], [Definition("v", "CREATE VIEW dbo.v AS SELECT * FROM dbo.T")]);
        WriteModel(root, Snapshot());
        const string migrationSql = "ALTER TABLE dbo.T ADD Other INT";
        context.Folder.Write("05Z_Migrations/2026-01-01/01_T.sql", migrationSql);
        var database = new FakeDatabase(DatabaseState.From(Snapshot()));

        var run = context.Execute(database, continueOnError: false);

        Assert.That(run.Error, Is.Null);
        Assert.That(run.Steps, Is.EqualTo(new[] { "05Z_Migrations/2026-01-01/01_T.sql" }));
        Assert.That(database.ModelReads, Has.Count.EqualTo(2), "the schema is still read again after the migration");
    }

    [Test]
    public void SchemaBoundObjectsAndTheirDependentsAreLeftToTheMigrationAuthor()
    {
        DatabaseModel Snapshot(bool changed) => Model([Table("T", changed: changed)], [
            Definition("bound", "CREATE VIEW dbo.bound WITH SCHEMABINDING AS SELECT Id FROM dbo.T"),
            Definition("caller", "CREATE PROCEDURE dbo.caller AS SELECT Id FROM dbo.bound", type: "P"),
            Definition("available", "CREATE PROCEDURE dbo.available AS SELECT Id FROM dbo.T", type: "P")]);
        var plan = new DatabaseDependencyPlan(Snapshot(false), Snapshot(true), Provider(), ["ALTER TABLE dbo.T ALTER COLUMN Id BIGINT"]);
        Assert.That(plan.Drops.Select(s => s.Key), Is.EqualTo(new[] { "dbo.available" }));
        Assert.That(plan.Drops.Single().Slot, Is.EqualTo(SlotOf("05_Programmability")));
    }
    [Test]
    public void ExecuteScriptsKeepsFolderOrderAndInterleavesRawFolders()
    {
        using var context = ScriptTestContext.SqlServer();
        var root = context.Folder.Directory;
        {
            DatabaseModel Snapshot(bool changed) => Model([Table("T", changed: changed)],
                [
                    Definition("zFunction", "CREATE FUNCTION dbo.zFunction() RETURNS int AS BEGIN RETURN (SELECT COUNT(*) FROM dbo.T) END", type: "FN"),
                    Definition("aView", "CREATE VIEW dbo.aView AS SELECT Id FROM dbo.T")
                ],
                triggers: [new TriggerModel { SchemaName = "dbo", TableName = "T", TriggerName = "aTrigger", Definition = "CREATE TRIGGER dbo.aTrigger ON dbo.T AFTER INSERT AS SELECT 1" }]);
            WriteModel(root, Snapshot(true));
            File.WriteAllText(Path.Combine(root.CreateSubdirectory("07_Data_Always").FullName, "Data.sql"), "SELECT 'Data'");
            var run = context.Execute(new FakeDatabase(DatabaseState.From(Snapshot(false))), continueOnError: false);
            Assert.That(run.Error, Is.Null);
            var actions = run.Progress.Select(p => p.ProgressStatus)
                .Where(s => s.StartsWith("DROP ") || s.StartsWith("CREATE ") || s.Contains("Data.sql")).ToList();
            Assert.That(actions, Is.EqualTo(new[]
            {
                "DROP 05_Programmability/dbo.zFunction", "CREATE 05_Programmability/dbo.zFunction",
                "DROP 05_Views/dbo.aView", "CREATE 05_Views/dbo.aView",
                "DROP 06_Triggers/dbo.aTrigger", "CREATE 06_Triggers/dbo.aTrigger",
                "07_Data_Always/Data.sql"
            }));
        }
    }

    private static void WriteModel(DirectoryInfo root, DatabaseModel model)
    {
        void Write(string category, string name, object item)
        {
            var folder = root.CreateSubdirectory(category);
            var path = Path.Combine(folder.FullName, $"{name}.sql");
            File.WriteAllText(path, "-- SQL is represented in the existing JSON model");
            File.WriteAllText(path + ".json", JsonSerializer.Serialize(item, item.GetType(), ModelsJsonSerializerOptions.Default));
        }

        foreach (var table in model.Tables)
        {
            Write("02_Tables", table.SchemaAndTableName, table);
        }

        foreach (var definition in model.Definitions)
        {
            Write(definition.Type == "VIEW" ? "05_Views" : "05_Programmability", definition.DefinitionName, definition);
        }

        foreach (var trigger in model.Triggers)
        {
            Write("06_Triggers", trigger.TriggerName, trigger);
        }
    }
}
