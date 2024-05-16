-- ProductDocument

IF NOT EXISTS
    (
        SELECT
            1
        FROM
            sys.tables INNER JOIN
            sys.schemas ON
                tables.schema_id = schemas.schema_id
        WHERE
            tables.name = 'ProductDocument' AND
            schemas.name = 'Production'
    )
    CREATE TABLE [Production].[ProductDocument]
    (
        [ProductID] INT NOT NULL,
        [DocumentNode] HIERARCHYID NOT NULL,
        [ModifiedDate] DATETIME NOT NULL DEFAULT (GETDATE())
    )
GO
