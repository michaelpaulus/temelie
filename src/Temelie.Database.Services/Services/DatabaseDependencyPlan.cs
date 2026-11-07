using System.Diagnostics;
using System.Text.Json;
using Temelie.Database.Models;
using Temelie.Database.Providers;

namespace Temelie.Database.Services;

internal sealed class DatabaseDependencyPlan
{
    internal sealed record Script(string Name, string Contents, string Key, int Slot);

    private sealed record Object(string Key, string Schema, string Name, string? Parent, string Directory, int Order,
        string Create, string Drop, string Comparison, bool IsTable, bool IsSchemaBound, bool IsIndexedView,
        IEnumerable<SqlDependencyReader.Reference> References)
    {
        // Views, routines, and triggers can hold stale metadata after a table changes; schema-bound objects cannot.
        public bool RefreshesWithTables => !IsSchemaBound && Directory is "05_Views" or "05_Programmability" or "06_Triggers";

        // Rebuilding these can be very expensive (indexed views) or is blocked by schema binding, so changes are left to migrations.
        public bool IsLocked => IsSchemaBound || IsIndexedView;
    }

    internal static readonly IReadOnlyList<string> DefaultFolderOrder =
    [
        "02_Tables",
        "03_Indexes",
        "04_CheckConstraints",
        "04_Migrations",
        "05_Programmability",
        "05_Views",
        "06_Triggers",
        "08_ForeignKeys",
        "09_SecurityPolicies"
    ];

    internal IReadOnlyList<Script> Drops { get; }
    internal IReadOnlyList<Script> Creates { get; }

    /// <summary>Tables that exist in both models but differ (ignoring collation and character set).</summary>
    internal IReadOnlyList<string> ChangedTables { get; }

    /// <summary>
    /// Scheduled objects whose own definition is unchanged; they are recreated only because something they depend on changed.
    /// A later plan cannot see that from the schema alone (for example after a migration altered the table).
    /// </summary>
    internal IReadOnlyList<string> RefreshKeys { get; }

    /// <summary>Existing schema-bound or indexed-view objects whose changes were not applied; a migration has to make them.</summary>
    internal IReadOnlyList<string> SkippedChanges { get; }

    /// <param name="migrationSql">Migrations that have run since <paramref name="current"/> no longer reflects their table changes.</param>
    /// <param name="getSlot">Maps an object folder to its execution position; null means the folder is not managed.</param>
    /// <param name="pendingKeys">Objects that an earlier plan scheduled but that have not run yet.</param>
    internal DatabaseDependencyPlan(DatabaseModel current, DatabaseModel updated, IDatabaseProvider provider,
        IEnumerable<string> migrationSql, Func<string, int?>? getSlot = null, string? databaseName = null,
        IEnumerable<string>? pendingKeys = null)
    {
        getSlot ??= directory =>
        {
            var index = DefaultFolderOrder.ToList().IndexOf(directory);
            return index < 0 ? null : index;
        };
        int SlotOf(string directory) => getSlot(directory) ?? throw new UnreachableException();

        var views = current.Views.Concat(updated.Views)
            .Select(v => $"{v.SchemaName}.{v.TableName}").ToHashSet(StringComparer.OrdinalIgnoreCase);
        var indexedViews = IndexedViews(current).Concat(IndexedViews(updated)).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var oldObjects = GetObjects(current, provider, views, indexedViews);
        var newObjects = GetObjects(updated, provider, views, indexedViews);
        if (!updated.Tables.Any(t => t.TableName.Equals("Migrations", StringComparison.OrdinalIgnoreCase)))
        {
            foreach (var item in oldObjects.Values.Where(o =>
                (o.IsTable && o.Name.Equals("Migrations", StringComparison.OrdinalIgnoreCase)) ||
                (o.Directory == "03_Indexes" && o.References.Any(r => r.Name.Equals("Migrations", StringComparison.OrdinalIgnoreCase)))).ToList())
            {
                oldObjects.Remove(item.Key);
            }
        }

        Object Find(string key) => newObjects.TryGetValue(key, out var item) ? item : oldObjects[key];
        bool IsManaged(string key) => getSlot(Find(key).Directory) is not null;

        var oldGraph = BuildGraph(oldObjects, databaseName);
        var newGraph = BuildGraph(newObjects, databaseName);
        var changed = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var changedTables = new List<string>();
        foreach (var item in oldObjects.Values)
        {
            if (getSlot(item.Directory) is null)
            {
                continue;
            }

            if (!newObjects.TryGetValue(item.Key, out var replacement))
            {
                changed.Add(item.Key);
            }
            else if (item.Comparison != replacement.Comparison)
            {
                changed.Add(item.Key);
                if (item.IsTable)
                {
                    changedTables.Add(item.Name);
                }
            }
        }

        foreach (var item in newObjects.Values)
        {
            if (getSlot(item.Directory) is not null && !oldObjects.ContainsKey(item.Key))
            {
                changed.Add(item.Key);
            }
        }

        var ownChanges = changed.ToHashSet(StringComparer.OrdinalIgnoreCase);
        foreach (var sql in migrationSql)
        {
            foreach (var reference in SqlDependencyReader.Read(sql, tableChangesOnly: true, mysql: provider.QuoteCharacterStart == "`"))
            {
                foreach (var objects in new[] { oldObjects, newObjects })
                {
                    var key = Resolve(reference, "", objects, databaseName);
                    if (key != null && objects[key].IsTable && IsManaged(key))
                    {
                        changed.Add(key);
                    }
                }
            }
        }

        foreach (var key in pendingKeys ?? [])
        {
            if ((oldObjects.ContainsKey(key) || newObjects.ContainsKey(key)) && IsManaged(key))
            {
                changed.Add(key);
            }
        }

        ChangedTables = changedTables;

        // Existing schema-bound objects, indexed views, and their indexes are never dropped or recreated here; missing ones are still created.
        bool IsLocked(string key) => oldObjects.TryGetValue(key, out var item) && item.IsLocked;
        SkippedChanges = changed.Where(k => ownChanges.Contains(k) && IsLocked(k))
            .Select(k => DisplayName(Find(k))).Order(StringComparer.OrdinalIgnoreCase).ToList();
        changed.RemoveWhere(IsLocked);

        // Use both graphs: a changed object may have removed a dependency in the new SQL.
        var dependents = Invert(oldGraph);
        foreach (var item in Invert(newGraph))
        {
            if (dependents.TryGetValue(item.Key, out var children))
            {
                children.UnionWith(item.Value);
            }
            else
            {
                dependents.Add(item.Key, item.Value);
            }
        }

        // Tables are never recreated, so only a real table change reaches its dependents, and only those that need a refresh.
        // Any other recreated object is dropped, so everything that depends on it has to be recreated too, except locked objects:
        // if one depends on it, the database refuses the drop and the change has to be made in a migration.
        IEnumerable<string> Next(string key)
        {
            var item = Find(key);
            if (item.IsTable && !changed.Contains(key))
            {
                return [];
            }

            return (dependents.GetValueOrDefault(key) ?? []).Where(d => !IsLocked(d) && (!item.IsTable || Find(d).RefreshesWithTables));
        }

        var affected = Closure(changed, Next);

        var dropKeys = affected.Where(k => oldObjects.ContainsKey(k) && !oldObjects[k].IsTable &&
            getSlot(oldObjects[k].Directory) is not null).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var createKeys = affected.Where(k => newObjects.ContainsKey(k) &&
            getSlot(newObjects[k].Directory) is not null &&
            (!newObjects[k].IsTable || !oldObjects.ContainsKey(k))).ToHashSet(StringComparer.OrdinalIgnoreCase);

        var oldDependents = Invert(oldGraph);
        var newDependents = Invert(newGraph);
        IEnumerable<string> DropPrerequisites(string key) => (oldDependents.GetValueOrDefault(key) ?? []).Where(dropKeys.Contains);
        IEnumerable<string> CreatePrerequisites(string key) => newGraph[key].Where(createKeys.Contains);

        // A drop waits for its dependents' drops, so it may run in a later folder than its own.
        var dropSlots = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        foreach (var key in Sort(DropPrerequisites, dropKeys, null))
        {
            dropSlots[key] = DropPrerequisites(key).Select(d => dropSlots[d])
                .Append(SlotOf(oldObjects[key].Directory)).Max();
        }

        // New tables keep their folder position (migrations may rely on it), so anything they need moves ahead of them.
        var createOrder = Sort(CreatePrerequisites, createKeys, null);
        var limits = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        foreach (var key in Enumerable.Reverse(createOrder))
        {
            var own = SlotOf(newObjects[key].Directory);
            var anchored = (newDependents.GetValueOrDefault(key) ?? []).Where(limits.ContainsKey).Select(d => limits[d]).ToList();
            if (newObjects[key].IsTable)
            {
                limits[key] = own;
            }
            else if (anchored.Count > 0)
            {
                limits[key] = Math.Min(own, anchored.Min());
            }
        }

        // Otherwise a create waits for its prerequisites and its own drop, so it may run in a later folder than its own.
        var createSlots = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        foreach (var key in createOrder)
        {
            createSlots[key] = CreatePrerequisites(key).Select(d => createSlots[d])
                .Append(limits.TryGetValue(key, out var limit) ? limit : SlotOf(newObjects[key].Directory))
                .Append(dropSlots.TryGetValue(key, out var dropSlot) ? dropSlot : 0).Max();
        }

        Drops = Sort(DropPrerequisites, dropKeys, key => (dropSlots[key], SlotOf(oldObjects[key].Directory), oldObjects[key].Order))
            .Select(k => ToScript(oldObjects[k], false, dropSlots[k])).ToList();
        Creates = Sort(CreatePrerequisites, createKeys, key => (createSlots[key], SlotOf(newObjects[key].Directory), newObjects[key].Order))
            .Select(k => ToScript(newObjects[k], true, createSlots[k])).ToList();
        RefreshKeys = dropKeys.Union(createKeys, StringComparer.OrdinalIgnoreCase).Where(k => !ownChanges.Contains(k))
            .Order(StringComparer.OrdinalIgnoreCase).ToList();
    }

    private static IEnumerable<string> IndexedViews(DatabaseModel model)
    {
        var indexed = model.PrimaryKeys.Concat(model.Indexes)
            .Select(i => $"{i.SchemaName}.{i.TableName}").ToHashSet(StringComparer.OrdinalIgnoreCase);
        return model.Definitions.Where(d => d.View is not null)
            .Select(d => $"{d.SchemaName}.{d.DefinitionName}").Where(indexed.Contains);
    }
    private static Dictionary<string, HashSet<string>> Invert(Dictionary<string, HashSet<string>> graph)
    {
        var inverted = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);
        foreach (var item in graph)
        {
            foreach (var dependency in item.Value)
            {
                if (!inverted.TryGetValue(dependency, out var children))
                {
                    children = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    inverted.Add(dependency, children);
                }

                children.Add(item.Key);
            }
        }

        return inverted;
    }

    private static HashSet<string> Closure(IEnumerable<string> seed, Func<string, IEnumerable<string>> next)
    {
        var result = new HashSet<string>(seed, StringComparer.OrdinalIgnoreCase);
        var queue = new Queue<string>(result);
        while (queue.TryDequeue(out var key))
        {
            foreach (var item in next(key))
            {
                if (result.Add(item))
                {
                    queue.Enqueue(item);
                }
            }
        }

        return result;
    }

    private static string DisplayName(Object item) =>
        $"{item.Directory}/{item.Schema}.{(item.Parent == null ? "" : item.Parent + ".")}{item.Name}";

    private static Script ToScript(Object item, bool create, int slot) =>
        new($"{(create ? "CREATE" : "DROP")} {DisplayName(item)}", create ? item.Create : item.Drop, item.Key, slot);

    private static Dictionary<string, Object> GetObjects(DatabaseModel model, IDatabaseProvider provider,
        HashSet<string> views, HashSet<string> indexedViews)
    {
        var objects = new Dictionary<string, Object>(StringComparer.OrdinalIgnoreCase);
        IEnumerable<SqlDependencyReader.Reference> Read(string? sql) =>
            SqlDependencyReader.Read(sql, mysql: provider.QuoteCharacterStart == "`");
        void Add(string schema, string name, string directory, IDatabaseObjectScript? script,
            IEnumerable<SqlDependencyReader.Reference> references, bool table = false, string? parent = null,
            string? comparison = null, bool schemaBound = false, bool indexedView = false)
        {
            if (script == null)
            {
                throw new InvalidOperationException($"No script provider for {schema}.{name}.");
            }

            var key = parent == null ? $"{schema}.{name}" : $"{schema}.{parent}.{directory}.{name}";
            var create = script.CreateScript;
            objects.Add(key, new Object(key, schema, name, parent, directory, objects.Count, create, script.DropScript,
                comparison ?? create, table, schemaBound, indexedView, references));
        }

        foreach (var table in model.Tables)
        {
            var clone = JsonSerializer.Deserialize<TableModel>(JsonSerializer.Serialize(table, ModelsJsonSerializerOptions.Default), ModelsJsonSerializerOptions.Default) ??
                throw new UnreachableException();
            clone.CollationName = "";
            clone.CharacterSetName = "";
            foreach (var column in clone.Columns)
            {
                column.CollationName = "";
                column.CharacterSetName = "";
                column.TableName = clone.TableName;
            }

            var references = table.Columns.SelectMany(c => Read($"{c.ColumnDefault} {c.ComputedDefinition}")).ToList();
            Add(table.SchemaName, table.TableName, "02_Tables", provider.GetScript(table), references, true,
                comparison: provider.GetScript(clone)?.CreateScript);
        }

        foreach (var definition in model.Definitions)
        {
            var key = $"{definition.SchemaName}.{definition.DefinitionName}";
            Add(definition.SchemaName, definition.DefinitionName, definition.Type == "VIEW" ? "05_Views" : "05_Programmability",
                provider.GetScript(definition), Read(definition.Definition).ToList(),
                schemaBound: SqlDependencyReader.IsSchemaBound(definition.Definition),
                indexedView: definition.View is not null && indexedViews.Contains(key));
        }

        foreach (var trigger in model.Triggers)
        {
            Add(trigger.SchemaName, trigger.TriggerName, "06_Triggers", provider.GetScript(trigger),
                Read(trigger.Definition).Append(new(trigger.SchemaName, trigger.TableName)).ToList());
        }

        foreach (var index in model.PrimaryKeys.Concat(model.Indexes))
        {
            var table = $"{index.SchemaName}.{index.TableName}";
            // Script view indexes the same way on both sides so an unchanged index never looks different.
            var script = provider.GetScript(index, views.Contains(table));
            // MySQL primary keys are emitted inline with their table.
            if (index.IsPrimaryKey && script == null)
            {
                continue;
            }

            Add(index.SchemaName, index.IndexName, "03_Indexes", script,
                [new(index.SchemaName, index.TableName)], parent: index.TableName, indexedView: indexedViews.Contains(table));
        }

        foreach (var constraint in model.CheckConstraints)
        {
            Add(constraint.SchemaName, constraint.CheckConstraintName, "04_CheckConstraints", provider.GetScript(constraint),
                Read(constraint.CheckConstraintDefinition).Append(new(constraint.SchemaName, constraint.TableName)).ToList(),
                parent: constraint.TableName);
        }

        foreach (var foreignKey in model.ForeignKeys)
        {
            Add(foreignKey.SchemaName, foreignKey.ForeignKeyName, "08_ForeignKeys", provider.GetScript(foreignKey),
                [new(foreignKey.SchemaName, foreignKey.TableName), new(foreignKey.ReferencedSchemaName, foreignKey.ReferencedTableName)],
                parent: foreignKey.TableName);
        }

        foreach (var policy in model.SecurityPolicies)
        {
            Add(policy.PolicySchema, policy.PolicyName, "09_SecurityPolicies", provider.GetScript(policy),
                policy.Predicates.SelectMany(p => Read(p.PredicateDefinition)
                .Append(new(p.TargetSchema, p.TargetName))).ToList(), schemaBound: policy.IsSchemaBound);
        }

        return objects;
    }
    private static Dictionary<string, HashSet<string>> BuildGraph(Dictionary<string, Object> objects, string? databaseName)
    {
        var graph = new Dictionary<string, HashSet<string>>(StringComparer.OrdinalIgnoreCase);
        foreach (var item in objects.Values)
        {
            var dependencies = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var reference in item.References)
            {
                var key = Resolve(reference, item.Schema, objects, databaseName);
                if (key != null && !key.Equals(item.Key, StringComparison.OrdinalIgnoreCase))
                {
                    dependencies.Add(key);
                }
            }

            graph.Add(item.Key, dependencies);
        }

        return graph;
    }

    private static string? Resolve(SqlDependencyReader.Reference reference, string schema, Dictionary<string, Object> objects, string? databaseName)
    {
        if (reference.Database != null && !reference.Database.Equals(databaseName, StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        if (!string.IsNullOrEmpty(reference.Schema))
        {
            if (reference.Schema.Equals(databaseName, StringComparison.OrdinalIgnoreCase) && objects.ContainsKey($".{reference.Name}"))
            {
                return $".{reference.Name}";
            }

            return objects.ContainsKey($"{reference.Schema}.{reference.Name}") ? $"{reference.Schema}.{reference.Name}" : null;
        }

        foreach (var candidate in new[] { schema, "dbo", "" }.Distinct(StringComparer.OrdinalIgnoreCase))
        {
            if (objects.ContainsKey($"{candidate}.{reference.Name}"))
            {
                return $"{candidate}.{reference.Name}";
            }
        }

        return null;
    }

    // Kahn's algorithm: prerequisites first, then lowest slot / folder / model order / name among the ready items.
    private static List<string> Sort(Func<string, IEnumerable<string>> prerequisites, HashSet<string> selected,
        Func<string, (int Slot, int Folder, int Order)>? priority)
    {
        priority ??= _ => (0, 0, 0);
        var comparer = Comparer<(int Slot, int Folder, int Order, string Key)>.Create((a, b) =>
        {
            var result = a.Slot.CompareTo(b.Slot);
            if (result == 0)
            {
                result = a.Folder.CompareTo(b.Folder);
            }

            if (result == 0)
            {
                result = a.Order.CompareTo(b.Order);
            }

            return result == 0 ? StringComparer.OrdinalIgnoreCase.Compare(a.Key, b.Key) : result;
        });

        var result = new List<string>();
        var counts = selected.ToDictionary(k => k, k => prerequisites(k).Count(), StringComparer.OrdinalIgnoreCase);
        var waiting = selected.ToDictionary(k => k, _ => new List<string>(), StringComparer.OrdinalIgnoreCase);
        foreach (var key in selected)
        {
            foreach (var prerequisite in prerequisites(key))
            {
                waiting[prerequisite].Add(key);
            }
        }

        var ready = new PriorityQueue<string, (int Slot, int Folder, int Order, string Key)>(comparer);
        void Enqueue(string key)
        {
            var (slot, folder, order) = priority(key);
            ready.Enqueue(key, (slot, folder, order, key));
        }

        foreach (var key in selected)
        {
            if (counts[key] == 0)
            {
                Enqueue(key);
            }
        }

        while (ready.TryDequeue(out var key, out _))
        {
            result.Add(key);
            foreach (var child in waiting[key])
            {
                if (--counts[child] == 0)
                {
                    Enqueue(child);
                }
            }
        }

        if (result.Count != selected.Count)
        {
            var remaining = selected.Where(k => counts[k] > 0).ToHashSet(StringComparer.OrdinalIgnoreCase);
            var path = new List<string>();
            var positions = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            var key = remaining.Order(StringComparer.OrdinalIgnoreCase).First();
            while (!positions.ContainsKey(key))
            {
                positions.Add(key, path.Count);
                path.Add(key);
                key = prerequisites(key).Where(remaining.Contains).Order(StringComparer.OrdinalIgnoreCase).First();
            }

            throw new InvalidOperationException($"Circular SQL dependency: {string.Join(" -> ", path.Skip(positions[key]).Append(key))}");
        }

        return result;
    }
}
