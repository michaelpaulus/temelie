using Temelie.Database.Models;

namespace Temelie.Database.Testing;

/// <summary>Builders for database object models.</summary>
public static class Db
{
    public static TableModel Table(string schema, string name, params (string Name, string Type)[] columns)
    {
        var table = new TableModel { SchemaName = schema, TableName = name };
        var columnId = 1;
        foreach (var (columnName, columnType) in columns.Length == 0 ? [("Id", "INT")] : columns)
        {
            table.Columns.Add(Column(schema, name, columnName, columnType, columnId++));
        }

        return table;
    }

    public static ColumnModel Column(string schema, string table, string name, string type, int columnId = 1) => new()
    {
        SchemaName = schema,
        TableName = table,
        ColumnName = name,
        ColumnType = type,
        ColumnId = columnId,
        Precision = type.StartsWith("NVARCHAR", StringComparison.OrdinalIgnoreCase) || type.StartsWith("VARCHAR", StringComparison.OrdinalIgnoreCase) ? 50 : 0
    };

    public static DefinitionModel View(string schema, string name, string definition) => new()
    {
        SchemaName = schema,
        DefinitionName = name,
        XType = "V",
        Definition = definition,
        View = new TableModel { SchemaName = schema, TableName = name, IsView = true }
    };

    public static DefinitionModel Procedure(string schema, string name, string definition) => new()
    {
        SchemaName = schema,
        DefinitionName = name,
        XType = "P",
        Definition = definition
    };

    public static DefinitionModel Function(string schema, string name, string definition) => new()
    {
        SchemaName = schema,
        DefinitionName = name,
        XType = "FN",
        Definition = definition
    };

    public static IndexModel PrimaryKey(string schema, string table, string name, params string[] columns) => new()
    {
        SchemaName = schema,
        TableName = table,
        IndexName = name,
        IndexType = "CLUSTERED",
        IsPrimaryKey = true,
        IsUnique = true,
        Columns = columns.Select(i => new IndexColumnModel { ColumnName = i }).ToList()
    };

    public static IndexModel Index(string schema, string table, string name, params string[] columns) => new()
    {
        SchemaName = schema,
        TableName = table,
        IndexName = name,
        IndexType = "NONCLUSTERED",
        Columns = columns.Select(i => new IndexColumnModel { ColumnName = i }).ToList()
    };

    public static CheckConstraintModel Check(string schema, string table, string name, string definition) => new()
    {
        SchemaName = schema,
        TableName = table,
        CheckConstraintName = name,
        CheckConstraintDefinition = definition
    };

    public static TriggerModel Trigger(string schema, string table, string name, string definition) => new()
    {
        SchemaName = schema,
        TableName = table,
        TriggerName = name,
        Definition = definition
    };

    public static ForeignKeyModel ForeignKey(string schema, string table, string name, string column, string referencedTable, string referencedColumn) => new()
    {
        SchemaName = schema,
        TableName = table,
        ForeignKeyName = name,
        ReferencedSchemaName = schema,
        ReferencedTableName = referencedTable,
        DeleteAction = "NO_ACTION",
        UpdateAction = "NO_ACTION",
        Detail = [new ForeignKeyDetailModel { Column = column, ReferencedColumn = referencedColumn }]
    };
}
